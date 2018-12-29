using System;
using System.IO;
using System.Text;
using System.Net.Http;
using static System.Math;
using System.Security.Permissions;
using System.Linq;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Xamarin.Forms.HotReload.Observer
{
    public static class FileObserver
    {
        private static readonly object _locker = new object();
        private static HttpClient _client;
        private static DateTime _lastChangeTime;

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

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                Console.WriteLine("MAKE SURE YOU PASSED RIGHT DEVICE URL AS 'U={DEVICE_URL}' ARGUMENT.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\n\n> HOTRELOADER STARTED AT {DateTime.Now}");
            Console.WriteLine($"\n> PATH: {path}");
            Console.WriteLine($"\n> URL: {url}\n");

            var observer = new FileSystemWatcher
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.xaml",
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };

            _client = new HttpClient
            {
                BaseAddress = new Uri(url)
            };

            observer.Changed += OnFileChanged;
            observer.Created += OnFileChanged;
            observer.Renamed += OnFileChanged;
            do
            {
                Console.WriteLine("\nPRESS \'ESC\' TO STOP.");
            } while (Console.ReadKey().Key != ConsoleKey.Escape);

            observer.Changed -= OnFileChanged;
            observer.Created -= OnFileChanged;
            observer.Renamed -= OnFileChanged;
        }

        private static string RetrieveCommandLineArgument(string key, string defaultValue, string[] args)
        {
            var value = args.FirstOrDefault(x => x.StartsWith(key, StringComparison.InvariantCultureIgnoreCase));
            return value != null ? value.Substring(2, value.Length - 2) : defaultValue;
        }
        
        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            var now = DateTime.Now;
            lock (_locker)
            {
                if (Abs((now - _lastChangeTime).TotalMilliseconds) < 900)
                {
                    return;
                }
                _lastChangeTime = now;
            }

            var filePath = e.FullPath.Replace("/.#", "/");
            Console.WriteLine($"CHANGED {now}: {filePath}");
            SendFile(filePath);
        }

        private static async void SendFile(string filePath)
        {
            var xaml = File.ReadAllText(filePath);
            var data = Encoding.UTF8.GetBytes(xaml);
            using (var content = new ByteArrayContent(data))
            {
                await _client.PostAsync("reload", content).ConfigureAwait(false);
            }
        }
    }
}
