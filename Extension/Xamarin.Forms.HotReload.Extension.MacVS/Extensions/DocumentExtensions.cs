using Xamarin.Forms.HotReload.Extension.Models;
using Document = MonoDevelop.Ide.Gui.Document;

namespace Xamarin.Forms.HotReload.Extension.MacVS.Extensions
{
    public static class DocumentExtensions
    {
        public static DevEnviromentDocument ToDevEnviromentDocument(this Document document)
        {
            var path = document.FileName;
            var content = document.Editor?.Text ?? string.Empty;
            return new DevEnviromentDocument(path, content);
        }
    }
}