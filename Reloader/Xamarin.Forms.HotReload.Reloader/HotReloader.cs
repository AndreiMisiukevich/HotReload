using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.Http;
using System.Collections.Concurrent;
using Xamarin.Forms.HotReload.Reloader;
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

namespace Xamarin.Forms
{
    public sealed class HotReloader
    {
        private static readonly Lazy<HotReloader> _lazyHotReloader;

        static HotReloader() => _lazyHotReloader = new Lazy<HotReloader>(() => new HotReloader());

        public static HotReloader Current => _lazyHotReloader.Value;

        private string _prevXaml;
        private Thread _daemonThread;
        private ConcurrentDictionary<string, ReloadItem> _fileMapping;
        private readonly Type _xamlFilePathAttributeType = typeof(XamlFilePathAttribute);
        private readonly object _requestLocker = new object();

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

        public void Start(Application app, int port = 8000)
            => Start(app, port, ReloaderScheme.Http);

        public void Start(Application app, int port, ReloaderScheme scheme)
        {
            Stop();
            IsRunning = true;

            TrySubscribeRendererPropertyChanged("Platform.RendererProperty", "CellRenderer.RendererProperty", "CellRenderer.RealCellProperty");

            _fileMapping = new ConcurrentDictionary<string, ReloadItem>();

            if (HasCodegenAttribute(app))
            {
                InitializeElement(app);
            }

            var listener = new HttpListener
            {
                Prefixes =
                {
                    $"{scheme.ToString().ToLower()}://*:{port}/"
                }
            };
            listener.Start();

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

            foreach (var address in addresses)
            {
                Console.WriteLine($"### AVAILABLE DEVICE's IP: {address} ###");
            }

            Console.WriteLine($"### HOTRELOAD STARTED ###");
        }

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
                    Console.WriteLine($"#### HOTRELOAD COULD NOT FIND {path}");
                    continue;
                }
                var originalRendererPropertyChanged = rendererPropertyChangedInfo?.GetValue(rendererProperty) as BindableProperty.BindingPropertyChangedDelegate;

                var rendererPopertyChangedWrapper = new BindableProperty.BindingPropertyChangedDelegate((bindable, oldValue, newValue) =>
                {
                    originalRendererPropertyChanged?.Invoke(bindable, oldValue, newValue);
                    //TODO: check if resource dictionary update needed for all elements
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

                                ReloadElements(xaml);
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

            var elementType = obj.GetType();
            var className = RetrieveClassName(elementType);
            if (!_fileMapping.TryGetValue(className, out ReloadItem item))
            {
                item = new ReloadItem();
                _fileMapping[className] = item;

                var type = obj.GetType();
                using (var stream = type.Assembly.GetManifestResourceStream(type.FullName + ".xaml"))
                {
                    if (stream != null && stream != Stream.Null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var xaml = reader.ReadToEnd();
                            item.Xaml.LoadXml(xaml);
                        }
                    }
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

        private void ReloadElements(string xaml)
        {
            var className = RetrieveClassName(xaml);

            if (string.IsNullOrWhiteSpace(className))
            {
                Console.WriteLine("### HOTRELOAD ERROR: 'x:Class' NOT FOUND ###");
                return;
            }

            ReloadItem item = null;
            try
            {
                if (!_fileMapping.TryGetValue(className, out item))
                {
                    item = new ReloadItem();
                    _fileMapping[className] = item;
                }
                item.Xaml.LoadXml(xaml);
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
                    switch (item.Xaml.DocumentElement.Name)
                    {
                        case "Application":
                            affectedItems = _fileMapping.Values.Where(x => x.Xaml.InnerXml.Contains("StaticResource"));
                            break;
                        case "ResourceDictionary":
                            affectedItems = _fileMapping.Values.Where(
                                e => ContainsResourceDictionary((e.Objects.FirstOrDefault() as VisualElement)?.Resources, className));
                            break;
                        default:
                            return;
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

            var rebuildEx = RebuildElement(obj, xamlDoc);
            if(!(obj is VisualElement))
            {
                if(rebuildEx != null)
                {
                    throw rebuildEx;
                }
                OnLoaded(obj);
                return;
            }

            //Update resources
            foreach (var dict in GetResourceDictionaries((obj as VisualElement)?.Resources ?? (obj as Application)?.Resources))
            {
                var name = dict.GetType().FullName;
                if (_fileMapping.TryGetValue(name, out ReloadItem item))
                {
                    dict.Clear();
                    dict.LoadFromXaml(item.Xaml.InnerXml);
                }
            }

            var modifiedXml = new XmlDocument();
            modifiedXml.LoadXml(xamlDoc.InnerXml);

            var isResourceFound = false;
            foreach (XmlNode node in modifiedXml.LastChild)
            {
                if (node.Name.EndsWith(".Resources", StringComparison.CurrentCulture))
                {
                    node.ParentNode.RemoveChild(node);
                    isResourceFound = true;
                    break;
                }
            }

            //Update object without resources
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

        private bool ContainsResourceDictionary(ResourceDictionary rootDict, string dictName)
            => GetResourceDictionaries(rootDict).Any(x => x.GetType().FullName == dictName);

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

        private string RetrieveClassName(string xaml)
            => Regex.Match(xaml ?? string.Empty, "x:Class=\"(.+)\"").Groups[1].Value;

        private string RetrieveClassName(Type type)
            => type.FullName;

        private bool HasCodegenAttribute(BindableObject bindable)
            => bindable.GetType().CustomAttributes.Any(x => x.AttributeType == _xamlFilePathAttributeType);

        private void OnLoaded(object element)
            => (element as IReloadable)?.OnLoaded();
    }
}