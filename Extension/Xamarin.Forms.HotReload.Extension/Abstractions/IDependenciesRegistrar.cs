namespace Xamarin.Forms.HotReload.Extension.Abstractions
{
    public interface IDependenciesRegistrar
    {
        void Register(IDependencyContainer container);
    }
}