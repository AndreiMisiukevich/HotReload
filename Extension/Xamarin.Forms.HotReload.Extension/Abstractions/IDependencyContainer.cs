namespace Xamarin.Forms.HotReload.Extension.Abstractions
{
    public interface IDependencyContainer
    {
        T Resolve<T>(object model);

        void Register<TAbst, TImpl>();
    }
}