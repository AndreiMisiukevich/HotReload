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

namespace Xamarin.Forms.HotReload.Observer
{
    public static class FileObserver
    {
        private static readonly string[] _supportedFileExtensions = { ".xaml" };
        private static readonly object _locker = new object();
        private static HttpClient _client;
        private static DateTime _lastChangeTime;

        private static readonly List<string> _addresses = new List<string>();

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

            foreach (var addr in url.Split(','))
            {
                if (!Uri.IsWellFormedUriString(addr, UriKind.Absolute))
                {
                    Console.WriteLine("MAKE SURE YOU PASSED RIGHT DEVICE URL AS 'U={DEVICE_URL}' OR AS 'U={DEVICE_URL,DEVICE_URL2,...}' ARGUMENT.");
                    Console.ReadKey();
                    return;
                }

                _addresses.Add(addr);
            }

            Console.WriteLine($"\n\n> HOTRELOADER STARTED AT {DateTime.Now}");
            Console.WriteLine($"\n> PATH: {path}");

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
                var xaml = File.ReadAllText(filePath);
                var data = Encoding.UTF8.GetBytes(xaml);
                using (var content = new ByteArrayContent(data))
                {
                    var sendTasks = _addresses.Select(addr => _client.PostAsync($"{addr}/reload", content)).ToArray();

                    await Task.WhenAll(sendTasks).ConfigureAwait(false);
                }
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("ERROR: NO CONNECTION.");
            }
        }
    }
}