using System;

namespace Xamarin.Forms.HotReload.Extension.Abstractions
{
    internal interface ILogger
    {
        void Log(string message);

        void LogException(Exception exception);
    }
}