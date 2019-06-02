using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using System.Net;
using System.Threading;

namespace Xamarin.Forms.HotReload.Observer
{
    public static class FileObserver
    {
        private static readonly string[] _supportedFileExtensions = { ".xaml", ".css" };
        private static readonly object _locker = new object();
        private static HttpClient _client;
        private static DateTime _lastChangeTime;

        private static readonly HashSet<string> _addresses = new HashSet<string>();

        public static void Main() => Run();

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void Run()
        {
            var addresses = NetworkInterface.GetAllNetworkInterfaces()
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(x => x.Address.MapToIPv4())
                .Where(x => x.ToString() != "127.0.0.1")
                .ToArray();

            var ip = addresses.FirstOrDefault()?.ToString() ?? "127.0.0.1";

            var args = Environment.GetCommandLineArgs();
            var path = RetrieveCommandLineArgument("p=", Environment.CurrentDirectory, args);
            var url = RetrieveCommandLineArgument("u=", $"http://{ip}:8000", args);
            var autoDiscoveryPort = RetrieveCommandLineArgument("a=", "15000", args);

            try
            {
                Directory.GetDirectories(path);
            }
            catch
            {
                Console.WriteLine("MAKE SURE YOU PASSED RIGHT PATH TO PROJECT DIRECTORY AS 'P={PATH}' ARGUMENT.");
                Console.ReadKey();
                return;
            }

            foreach (var addr in url.Split(new char[] { ',', ';' }))
            {
                if (!Uri.IsWellFormedUriString(addr, UriKind.Absolute))
                {
                    Console.WriteLine("MAKE SURE YOU PASSED RIGHT DEVICE URL AS 'U={DEVICE_URL}' OR AS 'U={DEVICE_URL,DEVICE_URL2,...}' ARGUMENT.");
                    Console.ReadKey();
                    return;
                }

                _addresses.Add(addr);
            }

            UdpReceiver receiver = null;
            try
            {
                receiver = new UdpReceiver(int.Parse(autoDiscoveryPort));
                receiver.Received += (addressMsg) =>
                {
                    //TODO: pick needed address
                    var address = addressMsg.Split(';').FirstOrDefault();
                    if (address != null)
                    {
                        Console.WriteLine($"ADDRESS IS DETECTED: {address}");
                        _addresses.Add(address);
                    }
                };
                receiver.StartAsync();
            }
            catch
            {
                Console.WriteLine("MAKE SURE YOU PASSED RIGHT AUTO DISCOVERY RECEIVER PORT AS 'A={PORT}' ARGUMENT.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\n\n> HOTRELOADER STARTED AT {DateTime.Now}");
            Console.WriteLine($"\n> PATH: {path}");
            Console.WriteLine($"\n> AUTO DISCOVERY PORT: {autoDiscoveryPort}");

            foreach (var addr in _addresses)
            {
                Console.WriteLine($"\n> URL: {addr}\n");
            }


            _client = new HttpClient();

            foreach (var fileExtension in _supportedFileExtensions)
            {
                var observer = new FileSystemWatcher
                {
                    Path = path,
                    NotifyFilter = NotifyFilters.LastWrite |
                        NotifyFilters.Attributes |
                        NotifyFilters.Size |
                        NotifyFilters.CreationTime |
                        NotifyFilters.FileName,
                    Filter = $"*{fileExtension}",
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true
                };

                observer.Changed += OnFileChanged;
                observer.Created += OnFileChanged;
                observer.Renamed += OnFileChanged;
            }

            do
            {
                Console.WriteLine("\nPRESS \'ESC\' TO STOP.");
            } while (Console.ReadKey().Key != ConsoleKey.Escape);

            receiver.Stop();
        }

        private static string RetrieveCommandLineArgument(string key, string defaultValue, string[] args)
        {
            var value = args.FirstOrDefault(x => x.StartsWith(key, StringComparison.InvariantCultureIgnoreCase));
            return value != null ? value.Substring(2, value.Length - 2) : defaultValue;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            var filePath = e.FullPath.Replace("/.#", "/");
            var now = DateTime.Now;
            lock (_locker)
            {
                if (Abs((now - _lastChangeTime).TotalMilliseconds) < 900 ||
                    _supportedFileExtensions.All(fileExt => !filePath.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
                _lastChangeTime = now;
            }
            Console.WriteLine($"CHANGED {now}: {filePath}");
            SendFile(filePath);
        }

        private static async void SendFile(string filePath)
        {
            try
            {
                var escapedFilePath = Uri.EscapeDataString(filePath);
                var xaml = File.ReadAllText(filePath);
                var data = Encoding.UTF8.GetBytes(xaml);
                using (var content = new ByteArrayContent(data))
                {
                    var sendTasks = _addresses.Select(addr => _client.PostAsync($"{addr}/reload?path={escapedFilePath}", content)).ToArray();

                    await Task.WhenAll(sendTasks).ConfigureAwait(false);
                }
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("ERROR: NO CONNECTION.");
            }
        }
    }

    internal sealed class UdpReceiver
    {
        internal event Action<string> Received;

        private readonly int _port;
        private readonly object _lockObject = new object();

        private UdpClient _udpClient;
        private CancellationTokenSource _udpTokenSource;

        internal UdpReceiver(int port)
        {
            _port = port;
        }

        internal async void StartAsync()
        {
            lock (_lockObject)
            {
                _udpClient = new UdpClient(_port);
                _udpTokenSource?.Cancel();
                _udpTokenSource = new CancellationTokenSource();
            }

            var token = _udpTokenSource.Token;
            while (!token.IsCancellationRequested)
            {
                var receivedResult = _udpClient != null
                    ? await _udpClient.ReceiveAsync().ConfigureAwait(false)
                    : default(UdpReceiveResult);

                var bytes = receivedResult.Buffer;

                if (!token.IsCancellationRequested &&
                    bytes != null &&
                    bytes.Any())
                {
                    var message = Encoding.ASCII.GetString(bytes);
                    Received?.Invoke(message);
                }
            }
        }

        internal void Stop()
        {
            lock (_lockObject)
            {
                _udpTokenSource?.Cancel();
                _udpClient?.Dispose();
                _udpClient = null;
            }
        }
    }
}