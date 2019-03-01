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
using static System.Math;

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

            var lastUpdateTime = DateTime.UtcNow;
            _daemonThread = new Thread(() =>
            {
                do
                {
                    var context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem((_) => HandleRequest(context));
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

        public void RegisterReloadableNonElementComponent(object obj)
        {
            if (obj is Element || !obj.GetType().CustomAttributes.Any(x => x.AttributeType == _xamlFilePathAttributeType))
            {
                return;
            }
            InitializeElement(obj);
        }

        private void HandleRequest(HttpListenerContext context)
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
                                var className = RetriveClassName(xaml);

                                if (string.IsNullOrWhiteSpace(className))
                                {
                                    Console.WriteLine("### HOTRELOAD ERROR: 'x:Class' NOT FOUND ###");
                                    return;
                                }

                                if (!_fileMapping.TryGetValue(className, out ReloadItem item))
                                {
                                    item = new ReloadItem();
                                    _fileMapping[className] = item;
                                }

                                item.Xaml = xaml;
                                ReloadElements(className, item);
                            }
                        }
                    }
                }
                catch
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                context.Response.Close();
            }
        }

        private string RetriveClassName(string xaml)
        => Regex.Match(xaml, "x:Class=\"(.+)\"").Groups[1].Value;

        private string RetriveClassName(Type type)
        => type.FullName;

        private void InitializeElement(object obj)
        {
            if (obj == null)
            {
                return;
            }

            var elementType = obj.GetType();
            var className = RetriveClassName(elementType);
            if (!_fileMapping.TryGetValue(className, out ReloadItem item))
            {
                item = new ReloadItem();
                _fileMapping[className] = item;
            }
            item.Objects.Add(obj);

            if (string.IsNullOrWhiteSpace(item.Xaml))
            {
                OnLoaded(obj);
                return;
            }

            ReloadElement(obj, item.Xaml);
        }

        private void DestroyElement(object obj)
        {
            var className = RetriveClassName(obj.GetType());
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

        private void ReloadElements(string classFullName, ReloadItem item)
        {
            if (!item.Objects.Any())
            {
                return;
            }

            //var nameParts = classFullName.Split('.');
            //var nameSpace = string.Join(".", nameParts.Take(nameParts.Length - 1));
            //var className = nameParts[nameParts.Length - 1];

            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    foreach (var element in item.Objects)
                    {
                        ReloadElement(element, item.Xaml);
                    }

                    var checkElement = item.Objects.First();

                    ReloadItem[] affectedItems;
                    switch(item.Objects.First())
                    {
                        case Application app:
                            affectedItems = _fileMapping.Values.ToArray();
                            break;
                        case ResourceDictionary resDict:
                            var dictType = resDict.GetType();
                            affectedItems = _fileMapping.Values.Where(
                                e => (e.Objects.FirstOrDefault() as VisualElement)
                                    ?.Resources
                                    ?.MergedDictionaries
                                    ?.Any(d => d.GetType() == dictType)
                                    ?? false)
                                    .ToArray();
                            break;
                        default:
                            return;
                    }

                    foreach (var affectedItem in affectedItems)
                    {
                        foreach (var obj in affectedItem.Objects)
                        {
                            ReloadElement(obj, affectedItem.Xaml);
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("### HOTRELOAD ERROR: CANNOT PARSE XAML ###");
                }
            });
        }

        private void ReloadElement(object obj, string xaml)
        {
            switch (obj)
            {
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
                case ResourceDictionary resDict:
                    resDict.Clear();
                    break;
                case Application app:
                    app.Resources.Clear();
                    break;
            }
            if(obj is VisualElement visual)
            {
                visual.Resources?.Clear();
            }

            if (string.IsNullOrWhiteSpace(xaml))
            {
                obj.LoadFromXaml(obj.GetType());
            }
            else
            {
                obj.LoadFromXaml(xaml);
            }
            SetupNamedChildren(obj);
            OnLoaded(obj);
        }

        private void SetupNamedChildren(object obj)
        {
            var element = obj as Element;
            if(element == null)
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

        private void OnLoaded(object element) => (element as IReloadable)?.OnLoaded();
    }
}