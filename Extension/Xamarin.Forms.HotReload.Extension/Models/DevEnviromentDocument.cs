namespace Xamarin.Forms.HotReload.Extension.Models
{
    public class DevEnviromentDocument
    {
        public DevEnviromentDocument(string path, string content)
        {
            Path = path;
            Content = content;
        }

        public string Path { get; set; }

        public string Content { get; set; }
    }
}