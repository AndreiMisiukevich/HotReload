using Xamarin.Forms.HotReload.Extension.Enums;

namespace Xamarin.Forms.HotReload.Extension.Models
{
    public class InfoBarAction
    {
        public InfoBarAction(InfoBarActionType actionType, string title)
        {
            Type = actionType;
            Title = title;
        }

        public InfoBarActionType Type { get; }

        public string Title { get; }
    }
}