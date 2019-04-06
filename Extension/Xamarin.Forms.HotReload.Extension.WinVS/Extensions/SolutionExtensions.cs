using EnvDTE;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;

namespace Xamarin.Forms.HotReload.Extension.WinVS.Extensions
{
    public static class SolutionExtensions
    {
        public static bool IsXamarinProject(this IVsSolution solution, Project project)
        {
            return IsTypedProject(solution, project, WinVSConstants.XamarinAndroidProjectKind) ||
                   IsTypedProject(solution, project, WinVSConstants.XamariniOsProjectKind) ||
                   IsTypedProject(solution, project, WinVSConstants.XamarinUWPProjectKind);
        }

        public static bool FolderContainsXamarinProjects(this IVsSolution vsSolution, Project solutionFolder)
        {
            bool hasXamarinProjects = false;
            if (solutionFolder.ProjectItems != null)
            {
                for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
                {
                    var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                    if (subProject == null)
                    {
                        continue;
                    }

                    hasXamarinProjects = subProject.Kind == WinVSConstants.DefaultFolderKind
                        ? FolderContainsXamarinProjects(vsSolution, subProject)
                        : vsSolution.IsXamarinProject(subProject);

                    if (hasXamarinProjects)
                    {
                        break;
                    }
                }
            }

            return hasXamarinProjects;
        }

        private static bool IsTypedProject(IVsSolution solution, Project project, string guid)
        {
            solution.GetProjectOfUniqueName(project.UniqueName, out var hierarchy);
            if (hierarchy is IVsAggregatableProjectCorrected aggregatableProjectCorrected)
            {
                aggregatableProjectCorrected.GetAggregateProjectTypeGuids(out var projTypeGuids);
                if (projTypeGuids.ToUpper().Contains(guid.ToUpper()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}