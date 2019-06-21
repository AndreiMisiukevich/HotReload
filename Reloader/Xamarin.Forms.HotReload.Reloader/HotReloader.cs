using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.Http;
using System.Collections.Concurrent;
using System.Linq;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Xamarin.Forms
{
    public sealed partial class HotReloader
    {
        private static readonly Lazy<HotReloader> _lazyHotReloader;

        static HotReloader() => _lazyHotReloader = new Lazy<HotReloader>(() => new HotReloader());

        public static HotReloader Current => _lazyHotReloader.Value;

        private string _prevXaml;
        private Thread _daemonThread;
        private ConcurrentDictionary<string, ReloadItem> _fileMapping;
        private readonly Type _xamlFilePathAttributeType = typeof(XamlFilePathAttribute);
        private readonly object _requestLocker = new object();
        private Application _app;

        private HashSet<string> _cellViewReloadProps = new HashSet<string> { "Orientation", "Spacing", "IsClippedToBounds", "Padding", "HorizontalOptions", "Margin", "VerticalOptions", "Visual", "FlowDirection", "AnchorX", "AnchorY", "BackgroundColor", "HeightRequest", "InputTransparent", "IsEnabled", "IsVisible", "MinimumHeightRequest", "MinimumWidthRequest", "Opacity", "Rotation", "RotationX", "RotationY", "Scale", "ScaleX", "ScaleY", "Style", "TabIndex", "IsTabStop", "StyleClass", "TranslationX", "TranslationY", "WidthRequest", "DisableLayout", "Resources", "AutomationId", "ClassId", "StyleId" };

        private HotReloader()
        {
        }

        public bool IsRunning { get; private set; }

        public void Stop()
        {
            IsRunning = false;
            _daemonThread?.Abort();
            _daemonThread = null;
            _fileMapping = null;
        }

        /// <summary>
        /// Run HotReload. Use config for specifying settings
        /// </summary>
        /// <param name="app">App.</param>
        /// <param name="config">Config.</param>
        public void Run(Application app, Configuration config = null)
        {
            config = config ?? new Configuration();
            var devicePort = config.DeviceUrlPort;

            Stop();
            _app = app;
            IsRunning = true;

            TrySubscribeRendererPropertyChanged("Platform.RendererProperty", "CellRenderer.RendererProperty", "CellRenderer.RealCellProperty");

            _fileMapping = new ConcurrentDictionary<string, ReloadItem>();

            if (HasCodegenAttribute(app))
            {
                InitializeElement(app);
            }

            HttpListener listener = null;
            var maxPort = devicePort + 1000;
            while (devicePort < maxPort)
            {
                listener = new HttpListener
                {
                    Prefixes =
                    {
                        $"http://*:{devicePort}/"
                    }
                };
                try
                {
                    listener.Start();
                    break;
                }
                catch
                {
                    ++devicePort;
                }
            }

            _daemonThread = new Thread(() =>
            {
                do
                {
                    var context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem((_) => HandleReloadRequest(context));
                } while (true);
            });
            _daemonThread.Start();

            var addresses = NetworkInterface.GetAllNetworkInterfaces()
                          .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                          .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                          .Select(x => x.Address.MapToIPv4())
                          .Where(x => x.ToString() != "127.0.0.1")
                          .ToArray();

            foreach (var addr in addresses)
            {
                Console.WriteLine($"### HOTRELOAD DEVICE's IP: {addr} ###");
            }

            try
            {
                Console.WriteLine($"### HOTRELOAD STARTED ON DEVICE's PORT: {devicePort} ###");

                Task.Run(async () =>
                {
                    var portsRange = Enumerable.Range(15000, 2).Union(Enumerable.Range(17502, 18));

                    var isFirstTry = true;

                    while (IsRunning)
                    {
                        foreach (var possiblePort in portsRange.Take(isFirstTry ? 20 : 5))
                        {
                            if (Device.RuntimePlatform == Device.Android)
                            {
                                try
                                {
                                    using (var client = new UdpClient { EnableBroadcast = true })
                                    {
                                        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                                        var emulatorData = Encoding.ASCII.GetBytes($"http://127.0.0.1:{devicePort}");
                                        client.Send(emulatorData, emulatorData.Length, new IPEndPoint(IPAddress.Parse("10.0.2.2"), possiblePort));
                                    }
                                }
                                catch { }
                            }

                            foreach (var ip in addresses)
                            {
                                try
                                { 
                                    var remoteIp = new IPEndPoint(config.ExtensionIpAddress, possiblePort);
                                    using (var client = new UdpClient(new IPEndPoint(ip, 0)) { EnableBroadcast = true })
                                    {
                                        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                                        var data = Encoding.ASCII.GetBytes($"http://{ip}:{devicePort}");
                                        client.Send(data, data.Length, remoteIp);
                                    }
                                }
                                catch { }
                            }
                        }
                        isFirstTry = false;
                        await Task.Delay(12000);
                    }
                });
            }
            catch
            {
                Console.WriteLine($"### HOTRELOAD AUTODISCOVERY ERROR ###");
            }
        }

        #region Obsolete
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Please use Run method for configuring and runnig HotReload. Visit github for more info.", true)]
        public void Start(Application app, int devicePort = 8000, int extensionPort = 15000)
            => Run(app, new Configuration
            {
                DeviceUrlPort = devicePort
            });
        #endregion

        private void TrySubscribeRendererPropertyChanged(params string[] paths)
        {
            var platformAssembly = DependencyService.Get<ISystemResourcesProvider>()
                .GetType()
                .Assembly;

            foreach(var path in paths)
            {
                var parts = path.Split('.');
                var typeName = string.Join(".", parts.Take(parts.Length - 1));
                var propName = parts.Last();

                var type = platformAssembly
                    .GetTypes()
                    .FirstOrDefault(t => t.Name == typeName);

                var rendererPropInfo = type?.GetField(propName, BindingFlags.NonPublic | BindingFlags.Static);
                var rendererProperty = rendererPropInfo?.GetValue(null) as BindableProperty;

                var rendererPropertyChangedInfo = rendererProperty?.GetType().GetProperty("PropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);
                if(rendererPropertyChangedInfo == null)
                {
                    continue;
                }
                var originalRendererPropertyChanged = rendererPropertyChangedInfo?.GetValue(rendererProperty) as BindableProperty.BindingPropertyChangedDelegate;

                var rendererPopertyChangedWrapper = new BindableProperty.BindingPropertyChangedDelegate((bindable, oldValue, newValue) =>
                {
                    originalRendererPropertyChanged?.Invoke(bindable, oldValue, newValue);

                    if (!HasCodegenAttribute(bindable))
                    {
                        return;
                    }

                    if (newValue != null)
                    {
                        InitializeElement(bindable);
                        return;
                    }

                    DestroyElement(bindable);
                });

                rendererPropertyChangedInfo.SetValue(rendererProperty, rendererPopertyChangedWrapper);
            }
        }

        private void HandleReloadRequest(HttpListenerContext context)
        {
            lock (_requestLocker)
            {
                try
                {
                    var request = context.Request;
                    if (request.HttpMethod == HttpMethod.Post.Method &&
                        request.HasEntityBody &&
                        request.RawUrl.StartsWith("/reload", StringComparison.InvariantCulture))
                    {
                        using (var bodyStream = request.InputStream)
                        {
                            using (var bodyStreamReader = new StreamReader(bodyStream, request.ContentEncoding))
                            {
                                var xaml = bodyStreamReader.ReadToEnd();
                                if (xaml == _prevXaml)
                                {
                                    return;
                                }
                                _prevXaml = xaml;

                                var path = request.QueryString["path"];
                                var unescapedPath = string.IsNullOrWhiteSpace(path)
                                    ? null
                                    : Uri.UnescapeDataString(path);

                                ReloadElements(xaml, unescapedPath);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }

                context.Response.Close();
            }
        }

        private void InitializeElement(object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (obj is Cell cell)
            {
                cell.PropertyChanged += OnCellPropertyChanged;
            }

            var elementType = obj.GetType();
            var className = RetrieveClassName(elementType);
            if (!_fileMapping.TryGetValue(className, out ReloadItem item))
            {
                item = new ReloadItem();
                _fileMapping[className] = item;

                var type = obj.GetType();
                var stream = type.Assembly.GetManifestResourceStream(type.FullName + ".xaml");
                try
                {
                    if (stream == null)
                    {
                        var appResName = type.Assembly.GetManifestResourceNames()
                            .FirstOrDefault(x => (x.Contains("obj.Debug.") || x.Contains("obj.Release")) && x.Contains(type.Name));

                        if (!string.IsNullOrWhiteSpace(appResName))
                        {
                            stream = type.Assembly.GetManifestResourceStream(appResName);
                        }
                    }
                    if (stream != null && stream != Stream.Null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var xaml = reader.ReadToEnd();
                            item.Xaml.LoadXml(xaml);
                        }
                    }
                }
                finally
                {
                    stream?.Dispose();
                }
            }

            if (!(obj is ResourceDictionary))
            {
                item.Objects.Add(obj);
            }

            if (!item.HasUpdates)
            {
                OnLoaded(obj);
                return;
            }

            ReloadElement(obj, item);
        }

        private void DestroyElement(object obj)
        {
            if(obj is Cell cell)
            {
                cell.PropertyChanged -= OnCellPropertyChanged;
            }
            var className = RetrieveClassName(obj.GetType());
            if (!_fileMapping.TryGetValue(className, out ReloadItem item))
            {
                return;
            }
            var entry = item.Objects.FirstOrDefault(x => x == obj);
            if (entry == null)
            {
                return;
            }
            item.Objects.Remove(entry);
        }

        private void ReloadElements(string content, string path)
        {
            var isCss = Path.GetExtension(path) == ".css";
            var resKey = RetrieveClassName(content);

            if (string.IsNullOrWhiteSpace(resKey))
            {
                resKey = path.Replace("\\", ".").Replace("/", ".");
            }

            ReloadItem item = null;
            try
            {
                if (!_fileMapping.TryGetValue(resKey, out item))
                {
                    item = new ReloadItem();
                    _fileMapping[resKey] = item;
                }
                if (isCss)
                {
                    item.Css = content;
                }
                else
                {
                    item.Xaml.LoadXml(content);
                    //Remove ReSharper attributes
                    foreach (XmlNode node in item.Xaml.ChildNodes)
                    {
                        node?.Attributes?.RemoveNamedItem("d:DataContext");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("### HOTRELOAD ERROR: CANNOT PARSE XAML ###");
                return;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    item.HasUpdates = true;
                    foreach (var element in item.Objects)
                    {
                        ReloadElement(element, item);
                    }

                    IEnumerable<ReloadItem> affectedItems;
                    if (isCss)
                    {
                        affectedItems = _fileMapping.Values.Where(e => ContainsResourceDictionary(e, resKey));
                    }
                    else
                    {
                        switch (item.Xaml.DocumentElement.Name)
                        {
                            case "Application":
                                affectedItems = _fileMapping.Values.Where(x => x.Xaml.InnerXml.Contains("StaticResource"));
                                break;
                            case "ResourceDictionary":
                                affectedItems = _fileMapping.Values.Where(
                                    e => ContainsResourceDictionary(e, resKey));
                                break;
                            default:
                                return;
                        }

                        //reload all data in case of app resources update
                        if(affectedItems.Any(x => x.Objects.Any(r => r is Application)))
                        {
                            affectedItems = affectedItems.Union(_fileMapping.Values.Where(x => x.Xaml.InnerXml.Contains("StaticResource")));
                        }
                    }

                    foreach (var affectedItem in affectedItems)
                    {
                        foreach (var obj in affectedItem.Objects)
                        {
                            ReloadElement(obj, affectedItem);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("### HOTRELOAD ERROR: CANNOT PARSE XAML ###");
                }
            });
        }

        private void ReloadElement(object obj, ReloadItem reloadItem)
        {
            var xamlDoc = reloadItem.Xaml;

            //TODO: make sure it doesn't break anything
            if (obj is VisualElement ve)
            {
                ve.Resources = null;
            }

            //[0] Parse new xaml with resources
            var rebuildEx = RebuildElement(obj, xamlDoc);
            if(!(obj is VisualElement) && !(obj is Application))
            {
                if(rebuildEx != null)
                {
                    throw rebuildEx;
                }
                OnLoaded(obj);
                return;
            }

            //[1] Check if any dictionary was updated before
            foreach (var dict in GetResourceDictionaries((obj as VisualElement)?.Resources ?? (obj as Application)?.Resources).ToArray())
            {
                var name = dict.GetType().FullName;

                //[1.0] update own res
                if (_fileMapping.TryGetValue(name, out ReloadItem item))
                {
                    dict.Clear();
                    dict.LoadFromXaml(item.Xaml.InnerXml);
                }

                //[1.1] Update Source resources
                var sourceItem = GetItemForReloadingSourceRes(dict.Source, obj);
                if (sourceItem != null)
                {
                    //TODO: (?): Seems no need in this stuff
                    //dict.GetType().GetField("_source", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(dict, null);
                    //var resType = obj.GetType().Assembly.GetType(RetrieveClassName(sourceItem.Xaml.InnerXml));
                    //var rd = Activator.CreateInstance(resType) as ResourceDictionary;
                    //rd.Clear();
                    //rd.LoadFromXaml(sourceItem.Xaml.InnerXml);
                    //dict.Add(rd);
                    var rd = new ResourceDictionary();
                    rd.LoadFromXaml(sourceItem.Xaml.InnerXml);
                    foreach(var key in rd.Keys)
                    {
                        dict.Remove(key);
                    }
                    dict.LoadFromXaml(sourceItem.Xaml.InnerXml);
                }
                else if (dict.Source != null)
                {
                    var dId = GetResId(dict.Source, obj);
                    if(dId != null)
                    {
                        sourceItem = _fileMapping.FirstOrDefault(it => it.Key.EndsWith(dId, StringComparison.Ordinal)).Value;
                        if (sourceItem != null)
                        {
                            var rd = new ResourceDictionary();
                            rd.LoadFromXaml(sourceItem.Xaml.InnerXml);
                            foreach (var key in rd.Keys)
                            {
                                dict.Remove(key);
                            }
                            dict.LoadFromXaml(sourceItem.Xaml.InnerXml);
                        }
                    }
                }

                var styleSheets = dict.GetType().GetProperty("StyleSheets", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dict) as IList<StyleSheets.StyleSheet>;
                if(styleSheets != null)
                {
                    var sheets = xamlDoc.GetElementsByTagName("StyleSheet");
                    for(var i = 0; i < styleSheets.Count; ++i)
                    {
                        var src = sheets[i].Attributes["Source"];
                        if(src == null)
                        {
                            continue;
                        }
                        var rId = GetResId(new Uri(src.Value, UriKind.Relative), obj);
                        if(rId == null)
                        {
                            continue;
                        }
                        var rItem = _fileMapping.FirstOrDefault(it => it.Key.EndsWith(rId, StringComparison.Ordinal)).Value;
                        if(rItem == null)
                        {
                            continue;
                        }
                        styleSheets.RemoveAt(i);
                        var newSheet = StyleSheets.StyleSheet.FromString(rItem.Css);
                        styleSheets.Insert(i, newSheet);
                        break;
                    }
                }
            }

            var modifiedXml = new XmlDocument();
            modifiedXml.LoadXml(xamlDoc.InnerXml);

            var isResourceFound = false;

            if (!(obj is Application))
            {
                foreach (XmlNode node in modifiedXml.LastChild)
                {
                    if (node.Name.EndsWith(".Resources", StringComparison.CurrentCulture))
                    {
                        node.ParentNode.RemoveChild(node);
                        isResourceFound = true;
                        break;
                    }
                }
            }

            //[2] Update object without resources (Force to re-apply all styles)
            if (isResourceFound)
            {
                rebuildEx = RebuildElement(obj, modifiedXml);
            }

            if(rebuildEx != null)
            {
                throw rebuildEx;
            }

            SetupNamedChildren(obj);
            OnLoaded(obj);
        }

        private ReloadItem GetItemForReloadingSourceRes(Uri source, object belongObj)
        {
            if (source?.IsWellFormedOriginalString() ?? false)
            {
                var resourceId = GetResId(source, belongObj);
                using (var resStream = belongObj.GetType().Assembly.GetManifestResourceStream(resourceId))
                {
                    if (resStream != null && resStream != Stream.Null)
                    {
                        using (var resReader = new StreamReader(resStream))
                        {
                            var resClassName = RetrieveClassName(resReader.ReadToEnd());

                            if (_fileMapping.TryGetValue(resClassName, out ReloadItem resItem))
                            {
                                return resItem;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private string GetResId(Uri source, object belongObj)
        {
            var rootTargetPath = typeof(XamlResourceIdAttribute).GetMethod("GetPathForType", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { belongObj.GetType() });
            var resourcePath = typeof(ResourceDictionary.RDSourceTypeConverter).GetMethod("GetResourcePath", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { source, rootTargetPath })
                .ToString()
                .Replace("\\", ".")
                .Replace("/", ".");

            return $"{belongObj.GetType().Assembly.FullName.Split(',')[0]}.{resourcePath}";
        }

        private Exception RebuildElement(object obj, XmlDocument xmlDoc)
        {
            try
            {
                switch (obj)
                {
                    case MultiPage<Page> multiPage:
                        multiPage.Children.Clear();
                        break;
                    case ContentPage contentPage:
                        contentPage.Content = null;
                        break;
                    case ContentView contentView:
                        contentView.Content = null;
                        break;
                    case ScrollView scrollView:
                        scrollView.Content = null;
                        break;
                    case Layout<View> layout:
                        layout.Children.Clear();
                        break;
                    case Application app:
                        app.Resources.Clear();
                        break;
                }

                if(IsSubclassOfShell(obj))
                {
                    var shellType = obj.GetType();
                    shellType.GetProperty("FlyoutHeaderTemplate", BindingFlags.Instance | BindingFlags.Public).SetValue(obj, null);
                    shellType.GetProperty("FlyoutHeader", BindingFlags.Instance | BindingFlags.Public).SetValue(obj, null);
                    var items = shellType.GetProperty("Items", BindingFlags.Instance | BindingFlags.Public).GetValue(obj, null);
                    items.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public).Invoke(items, null);
                }

                if(obj is Grid grid)
                {
                    grid.RowDefinitions.Clear();
                    grid.ColumnDefinitions.Clear();
                }

                if (obj is View view)
                {
                    ClearView(view);
                }
                if (obj is Page page)
                {
                    page.ToolbarItems.Clear();
                }

                if (obj is ViewCell cell)
                {
                    return UpdateViewCell(cell, xmlDoc.InnerXml);
                }

                obj.LoadFromXaml(xmlDoc.InnerXml);
                return null;
            }
            catch(Exception ex)
            {
                return ex;
            }
        }

        private bool IsSubclassOfShell(object obj)
        {
            var t = obj.GetType();
            while (t != null)
            {
                if(t.FullName == "Xamarin.Forms.Shell")
                {
                    return true;
                }
                t = t.BaseType;
            }
            return false;
        }

        private void ClearView(View view)
        {
            view.Behaviors.Clear();
            view.GestureRecognizers.Clear();
            view.Effects.Clear();
            view.Triggers.Clear();
            view.Style = null;
        }

        private Exception UpdateViewCell(ViewCell cell, string xaml)
        {
            var xDoc = new XmlDocument();
            xDoc.LoadXml(xaml);
            var node = xDoc.LastChild?.LastChild?.ChildNodes?.Cast<XmlNode>().FirstOrDefault(n => n.Name.Contains(".Resources"));
            if (node != null)
            {
                foreach(XmlNode st in node)
                {
                    if(st.Attributes["Source"] != null)
                    {
                        node.RemoveChild(st);
                    }
                }
                xaml = xDoc.InnerXml;
            }
            var newCell = new ViewCell().LoadFromXaml(xaml);
            var newCellView = newCell.View;

            if (newCellView?.GetType() != cell.View?.GetType())
            {
                return new XmlException("HOTRELOAD: YOU CANNOT CHANGE ROOT VIEW TYPE");
            }

            ClearView(cell.View);

            foreach (var i in newCellView.Behaviors)
            {
                cell.View.Behaviors.Add(i);
            }
            foreach (var i in newCellView.GestureRecognizers)
            {
                cell.View.GestureRecognizers.Add(i);
            }
            foreach (var i in newCellView.Triggers)
            {
                cell.View.Triggers.Add(i);
            }
            foreach (var i in newCellView.Effects)
            {
                cell.View.Effects.Add(i);
            }
 
            foreach (var prop in newCellView
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => _cellViewReloadProps.Contains(x.Name)))
            {
                var newVal = prop.GetValue(newCellView);
                prop.SetValue(cell.View, newVal);
            }

            if (cell.View is ContentView cellView)
            {
                cellView.Content = (newCellView as ContentView).Content;
            }
            if (cell.View is Layout<View> cellLayout)
            {
                cellLayout.Children.Clear();
                var children = (newCellView as Layout<View>).Children;
                foreach (var child in children.ToArray())
                {
                    children.Remove(child);
                    cellLayout.Children.Add(child);
                }
            }
            if(Math.Abs(cell.Height - newCell.Height) > double.Epsilon)
            {
                cell.Height = newCell.Height;
                cell.ForceUpdateSize();
            }

            return null;
        }

        private void SetupNamedChildren(object obj)
        {
            var element = obj as Element;
            if (element == null)
            {
                return;
            }
            var fields = obj.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.IsDefined(typeof(GeneratedCodeAttribute), true));

            foreach (var field in fields)
            {
                var value = element.FindByName<object>(field.Name);
                field.SetValue(obj, value);
            }
        }

        private bool ContainsResourceDictionary(ReloadItem item, string dictName)
        {
            var isCss = Path.GetExtension(dictName) == ".css";
            var element = item.Objects.FirstOrDefault() as VisualElement;
            var matches = Regex.Matches(item.Xaml.InnerXml, @"Source[\n\r[:space:]]*=[\n\r[:space:]]*\""([^\""]+\.(xaml|css))\""", RegexOptions.Compiled);
            foreach (Match match in matches)
            {
                var value = match.Groups[1].Value;
                var resId = GetResId(new Uri(value, UriKind.Relative), element ?? (object)_app);
                if (dictName.EndsWith(resId, StringComparison.Ordinal))
                {
                    return true;
                }
                if (isCss)
                {
                    continue;
                }
                var checkItem = GetItemForReloadingSourceRes(new Uri(value, UriKind.Relative), element ?? (object)_app);
                if(checkItem != null)
                {
                    return true;
                }
            }
            return GetResourceDictionaries(element?.Resources).Any(x => x.GetType().FullName == dictName);
        }

        private IEnumerable<ResourceDictionary> GetResourceDictionaries(ResourceDictionary rootDict)
        {
            if (rootDict == null)
            {
                yield break;
            }

            yield return rootDict;

            if (rootDict.MergedDictionaries == null)
            {
                yield break;
            }

            foreach (var dict in rootDict.MergedDictionaries)
            {
                foreach (var x in GetResourceDictionaries(dict))
                {
                    yield return x;
                }
            }
        }

        private void OnCellPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Parent" && (sender as Cell).Parent == null)
            {
                DestroyElement(sender);
            }
        }

        private string RetrieveClassName(string xaml)
            => Regex.Match(xaml ?? string.Empty, "x:Class[\n\r[:space:]]*=[\n\r[:space:]]*\"([^\"]+)\"", RegexOptions.Compiled).Groups[1].Value;

        private string RetrieveClassName(Type type)
            => type.FullName;

        private bool HasCodegenAttribute(BindableObject bindable)
            => bindable.GetType().CustomAttributes.Any(x => x.AttributeType == _xamlFilePathAttributeType);

        private void OnLoaded(object element)
            => (element as IReloadable)?.OnLoaded();
    }
}