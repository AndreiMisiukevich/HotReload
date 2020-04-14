using System;

namespace Xamarin.Forms
{
    [Obsolete("Obsolete/Deprecated. Just define a method with name OnHotReloaded. Keep in mind that OnHotReloaded will not be called on initial load!")]
    public interface IReloadable
    {
        void OnLoaded();
    }
}
