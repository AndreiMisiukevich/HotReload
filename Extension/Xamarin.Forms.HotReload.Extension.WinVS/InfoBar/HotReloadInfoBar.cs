using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension.WinVS.InfoBar
{
    public class HotReloadInfoBar : IVsInfoBar, IVsInfoBarActionItemCollection, IVsInfoBarTextSpanCollection
    {
        private readonly List<IVsInfoBarActionItem> _actions = new List<IVsInfoBarActionItem>();
        private readonly List<IVsInfoBarTextSpan> _spans = new List<IVsInfoBarTextSpan>();

        public HotReloadInfoBar(string message)
        {
            _spans.Add(new InfoBarTextSpan(message));
        }

        int IVsInfoBarTextSpanCollection.Count => _spans.Count;

        int IVsInfoBarActionItemCollection.Count => _actions.Count;

        public ImageMoniker Image => default(ImageMoniker);

        public bool IsCloseButtonVisible => true;

        public IVsInfoBarTextSpanCollection TextSpans => this;

        public IVsInfoBarActionItemCollection ActionItems => this;

        public IVsInfoBarActionItem GetItem(int index)
        {
            return _actions[index];
        }

        public IVsInfoBarTextSpan GetSpan(int index)
        {
            return _spans[index];
        }

        public void SetInfoBarActions(InfoBarAction[] infoBarActions)
        {
            _spans.Add(new InfoBarTextSpan("            "));
            for (int i = 0; i < infoBarActions.Length; i++)
            {
                _actions.Add(new InfoBarHyperlink(infoBarActions[i].Title));
                if (i != infoBarActions.Length - 1)
                {
                    _actions.Add(new InfoBarHyperlink("|"));
                }
            }
        }
    }
}