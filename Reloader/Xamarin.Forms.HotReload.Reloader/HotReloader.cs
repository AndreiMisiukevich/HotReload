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

namespace Xamarin.Forms
{
    public sealed class HotReloader
    {
        private static readonly Lazy<HotReloader> _lazyHotReloader;

        static HotReloader() => _lazyHotReloader = new Lazy<HotReloader>(() => new HotReloader());

        public static HotReloader Current => _lazyHotReloader.Value;

        private Thread _daemonThread;
        private CancellationTokenSource _reloaderCancellationTokenSource;
        private ConcurrentDictionary<string, ReloadItem> _fileMapping;
        private readonly Type _xamlFilePathAttributeType;
        private readonly BindableProperty _rendererProperty;
        private readonly PropertyInfo _rendererPropertyChangedInfo;
        private readonly BindableProperty.BindingPropertyChangedDelegate _originalRendererPropertyChanged;

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
        }

        public bool IsRunning { get; private set; }

        public void Stop()
        {
            IsRunning = false;
            _reloaderCancellationTokenSource?.Cancel();
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

                var element = bindable as Element;
                if (newValue != null)
                {
                    InitializeElement(element);
                    return;
                }
                DestroyElement(element);
            });
            _rendererPropertyChangedInfo.SetValue(_rendererProperty, rendererPopertyChangedWrapper);

            _fileMapping = new ConcurrentDictionary<string, ReloadItem>();
            _reloaderCancellationTokenSource = new CancellationTokenSource();
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
                Console.WriteLine($"AVAILABLE DEVICE's IP: {address}");
            }

            Console.WriteLine($"HOTRELOAD STARTED");
        }

        private void HandleRequest(HttpListenerContext context)
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
                            var oldXaml = item.Xaml;
                            item.Xaml = xaml;

                            ReloadElements(className, item, oldXaml);
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

        private string RetriveClassName(string xaml)
        => Regex.Match(xaml, "x:Class=\"(.+)\"").Groups[1].Value;

        private string RetriveClassName(Type type)
        => type.FullName;

        private void InitializeElement(Element element)
        {
            if (element == null)
            {
                return;
            }

            var elementType = element.GetType();
            var className = RetriveClassName(elementType);
            if (!_fileMapping.TryGetValue(className, out ReloadItem item))
            {
                item = new ReloadItem();
                _fileMapping[className] = item;
            }
            item.Elements.Add(element);

            if (string.IsNullOrWhiteSpace(item.Xaml))
            {
                OnLoaded(element);
                return;
            }

            ReloadElement(element, item.Xaml);
        }

        private void DestroyElement(Element element)
        {
            var className = RetriveClassName(element.GetType());
            if (!_fileMapping.TryGetValue(className, out ReloadItem item))
            {
                return;
            }
            var entry = item.Elements.FirstOrDefault(x => x == element);
            if (entry == null)
            {
                return;
            }
            item.Elements.Remove(entry);
        }

        private void ReloadElements(string classFullName, ReloadItem item, string oldXaml)
        {
            if (!item.Elements.Any())
            {
                return;
            }

            var nameParts = classFullName.Split('.');
            var nameSpace = string.Join(".", nameParts.Take(nameParts.Length - 1));
            var className = nameParts[nameParts.Length - 1];

            var affectedItems = _fileMapping.Values.Where(e => Regex.IsMatch(e.Xaml, $"\"clr-namespace:{nameSpace}")
                                              && Regex.IsMatch(e.Xaml, $":{className}")).ToArray();

            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    foreach (var element in item.Elements)
                    {
                        ReloadElement(element, item.Xaml);
                    }

                    foreach (var affectedItem in affectedItems)
                    {
                        foreach (var element in affectedItem.Elements)
                        {
                            ReloadElement(element, affectedItem.Xaml);
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("### HOTRELOAD ERROR: CANNOT PARSE XAML ###");
                    item.Xaml = oldXaml;
                }
            });
        }

        private void ReloadElement(Element element, string xaml)
        {
            switch (element)
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
            }
            element.LoadFromXaml(xaml);
            SetupNamedChildren(element);
            OnLoaded(element);
        }

        private void SetupNamedChildren(Element element)
        {
            var fields = element.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.IsDefined(typeof(GeneratedCodeAttribute), true));

            foreach (var field in fields)
            {
                var value = element.FindByName<object>(field.Name);
                field.SetValue(element, value);
            }
        }

        private void OnLoaded(Element element) => (element as IReloadable)?.OnLoaded();
    }
}