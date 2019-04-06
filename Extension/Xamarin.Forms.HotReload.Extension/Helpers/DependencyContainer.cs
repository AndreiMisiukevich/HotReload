using System;
using System.Collections.Generic;
using Xamarin.Forms.HotReload.Extension.Abstractions;

namespace Xamarin.Forms.HotReload.Extension.Helpers
{
    public class DependencyContainer : IDependencyContainer
    {
        private readonly Dictionary<Type, Type> _dictionary = new Dictionary<Type, Type>();

        public DependencyContainer(IDependenciesRegistrar dependenciesRegistrar)
        {
            dependenciesRegistrar.Register(this);
        }

        public T Resolve<T>(object model)
        {
            return (T) Activator.CreateInstance(_dictionary[typeof(T)], args: model);
        }

        public void Register<TAbst, TImpl>()
        {
            _dictionary.Add(typeof(TAbst), typeof(TImpl));
        }
    }
}