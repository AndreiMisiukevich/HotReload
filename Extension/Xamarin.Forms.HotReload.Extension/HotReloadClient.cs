using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.HotReload.Extension
{
    internal class HotReloadClient
    {
        private HttpClient _httpClient;

        internal string Address => _httpClient.BaseAddress.ToString();

        internal void SetBaseAddress(string baseAddress)
        {
            if (_httpClient == null || baseAddress != _httpClient?.BaseAddress.AbsolutePath.TrimEnd('/'))
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(baseAddress),
                    Timeout = TimeSpan.FromSeconds(2)
                };
                _httpClient = client;
            }
        }

        internal async Task PostXamlUpdateAsync(string xmlString)
        {
            var data = Encoding.UTF8.GetBytes(xmlString);
            var content = new ByteArrayContent(data);
            await _httpClient.PostAsync("reload", content);
        }

        internal async Task PostCssUpdateAsync(string pathToCssFile)
        {
            var escapedCssFilePath = Uri.EscapeUriString(pathToCssFile);
            await _httpClient.PostAsync($"/reload/css?path={escapedCssFilePath}", null);
        }
    }
}