using EnvDTE;

namespace Xamarin.Forms.HotReload.Extension.WinVS.Extensions
{
    public static class ProjectExtensions
    {
        public static bool IsFolder(this Project project)
        {
            return project.Kind == WinVSConstants.DefaultFolderKind;
        }
    }
}