using System;
using Castle.Windsor;
using NHibernate.Bytecode;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture.Bytecode
{
    public class ActivatorObjectsFactory : IObjectsFactory
    {
        private readonly IWindsorContainer _container;

        public ActivatorObjectsFactory(IWindsorContainer container)
        {
            _container = container;
        }

        public object CreateInstance(System.Type type, params object[] ctorArgs)
        {
            if (_container.Kernel.HasComponent(type) || _container.Kernel.HasComponent(type.FullName))
            {
                return _container.Resolve(type, ctorArgs);
            }
            return Activator.CreateInstance(type, ctorArgs);
        }

        public object CreateInstance(System.Type type, bool nonPublic)
        {
            if (_container.Kernel.HasComponent(type) || _container.Kernel.HasComponent(type.FullName))
            {
                return _container.Resolve(type);
            }
            return Activator.CreateInstance(type, nonPublic);
        }

        public object CreateInstance(System.Type type)
        {
            if (_container.Kernel.HasComponent(type) || _container.Kernel.HasComponent(type.FullName))
            {
                return _container.Resolve(type);
            }
            return Activator.CreateInstance(type);
        }
    }
}