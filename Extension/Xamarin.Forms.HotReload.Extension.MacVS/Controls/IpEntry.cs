using System;
using System.Text.RegularExpressions;
using Gtk;

namespace Xamarin.Forms.HotReload.Extension.MacVS.Controls
{
    public class IpEntry : Entry
    {
        private readonly Regex _ipRegex = new Regex("^(?:[0-9]{0,3}\\.){3}[0-9]{0,3}$");
        private string _previousTextValue;

        public IpEntry()
        {
            Changed += OnEntryChanged;
        }

        private void OnEntryChanged(object sender, EventArgs e)
        {
            var ipEntry = (IpEntry) sender;
            var entryText = ipEntry.Text;
            if (entryText != _previousTextValue)
            {
                if (!_ipRegex.IsMatch(entryText))
                {
                    ipEntry.Text = _previousTextValue;
                }
            }

            _previousTextValue = entryText;
        }
    }
}