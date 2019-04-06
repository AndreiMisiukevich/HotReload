using System;
using System.Diagnostics;
using Xamarin.Forms.HotReload.Extension.Abstractions;

namespace Xamarin.Forms.HotReload.Extension.Helpers
{
    internal class OutputLogger : ILogger
    {
        private const string PackageLoggingFormat = "[### Hot Reload Extension ###] {0}";

        public void Log(string message)
        {
            Debug.WriteLine(PackageLoggingFormat, message);
        }

        public void LogException(Exception exception)
        {
            Debug.WriteLine(PackageLoggingFormat, exception);
        }
    }
}