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
        private readonly Type _xamlFilePathAttributeType;
        private readonly BindableProperty _rendererProperty;
        private readonly PropertyInfo _rendererPropertyChangedInfo;
        private readonly BindableProperty.BindingPropertyChangedDelegate _originalRendererPropertyChanged;
        private readonly object _requestLocker;

        private HotReloader()
        {
            _xamlFilePathAttributeType = typeof(XamlFilePathAttribute);

            var platformAssembly = DependencyService.Get<ISystemResourcesProvider>()
                .GetType()
                .Assembly;

            var platformType = platformAssembly
                .GetTypes()
                .FirstOrDefault(t => t.Name == "Platform");

            var rendererPropInfo = platformType.GetField("RendererProperty", BindingFlags.NonPublic | BindingFlags.Static);
            _rendererProperty = rendererPropInfo.GetValue(null) as BindableProperty;

            _rendererPropertyChangedInfo = _rendererProperty.GetType().GetProperty("PropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            _originalRendererPropertyChanged = _rendererPropertyChangedInfo.GetValue(_rendererProperty) as BindableProperty.BindingPropertyChangedDelegate;

            _requestLocker = new object();
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

            var rendererPopertyChangedWrapper = new BindableProperty.BindingPropertyChangedDelegate((bindable, oldValue, newValue) =>
            {
                _originalRendererPropertyChanged?.Invoke(bindable, oldValue, newValue);

                if (!bindable.GetType().CustomAttributes.Any(x => x.AttributeType == _xamlFilePathAttributeType))
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
            _rendererPropertyChangedInfo.SetValue(_rendererProperty, rendererPopertyChangedWrapper);

            _fileMapping = new ConcurrentDictionary<string, ReloadItem>();

            InitializeElement(app);

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
                    using (var reader = new StreamReader(stream))
                    {
                        var xaml = reader.ReadToEnd();
                        item.Xaml.LoadXml(xaml);
                    }
                }
            }

            if (!(obj is ResourceDictionary))
            {
                item.Objects.Add(obj);
            }

            if (!item.IsReloaded)
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

            RebuildElement(obj, xamlDoc);

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
                RebuildElement(obj, modifiedXml);
            }

            SetupNamedChildren(obj);
            OnLoaded(obj);
            reloadItem.IsReloaded = true;
        }

        private void RebuildElement(object obj, XmlDocument xmlDoc)
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
                case ViewCell viewCell:
                    viewCell.View = null;
                    break;
                case Layout<View> layout:
                    layout.Children.Clear();
                    break;
                case Application app:
                    app.Resources.Clear();
                    break;
            }

            if (obj is View view)
            {
                view.Behaviors.Clear();
                view.GestureRecognizers.Clear();
                view.Effects.Clear();
                view.Triggers.Clear();
                view.Style = null;
            }
            if (obj is Page page)
            {
                page.ToolbarItems.Clear();
            }

            obj.LoadFromXaml(xmlDoc.InnerXml);
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

            foreach (var md in rootDict.MergedDictionaries)
            {
                foreach (var x in GetResourceDictionaries(md))
                {
                    yield return x;
                }
            }
        }

        private string RetrieveClassName(string xaml)
            => Regex.Match(xaml ?? string.Empty, "x:Class=\"(.+)\"").Groups[1].Value;

        private string RetrieveClassName(Type type)
            => type.FullName;

        private void OnLoaded(object element)
            => (element as IReloadable)?.OnLoaded();
    }
}