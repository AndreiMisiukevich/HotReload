using EnvDTE;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension.WinVS.Extensions
{
    public static class DocumentExtensions
    {
        public static DevEnviromentDocument ToDevEnviromentDocument(this Document document)
        {
            var path = document.FullName;
            var textDocument = (TextDocument) document.Object("TextDocument");
            EditPoint editPoint = textDocument.StartPoint.CreateEditPoint();
            var content = editPoint.GetText(textDocument.EndPoint);
            return new DevEnviromentDocument(path, content);
        }
    }
}