using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;

namespace EdFi.Ods.Admin.Infrastructure
{
    /// <summary>
    /// A webApi dependency Resolver for Castle Windsor
    /// Ideas taken from: http://www.asp.net/web-api/overview/extensibility/using-the-web-api-dependency-resolver
    /// </summary>
    public class CastleDependencyResolver : IDependencyResolver
    {
        private readonly IWindsorContainer _container;

        public CastleDependencyResolver(IWindsorContainer container)
        {
            _container = container;
        }

        IDependencyScope IDependencyResolver.BeginScope()
        {
            return new CastleDependencyScope(_container);
        }

        object IDependencyScope.GetService(Type serviceType)
        {
            try
            {
                return _container.Resolve(serviceType);
            }
            catch
            {
                return null;
            }
        }

        IEnumerable<object> IDependencyScope.GetServices(Type serviceType)
        {
            try
            {
                return _container.ResolveAll(serviceType).Cast<object>();
            }
            catch
            {
                return new object[] { };
            }
        }

        void IDisposable.Dispose()
        {
            _container.Dispose();
        }
    }

    public class CastleDependencyScope : IDependencyScope
    {
        private readonly IDisposable _scopeDisposable;
        private readonly IWindsorContainer _container;

        public CastleDependencyScope(IWindsorContainer container)
        {
            _scopeDisposable = container.BeginScope();
            _container = container;
        }

        object IDependencyScope.GetService(Type serviceType)
        {
            try
            {
                return _container.Resolve(serviceType);
            }
            catch
            {
                return null;
            }
        }

        IEnumerable<object> IDependencyScope.GetServices(Type serviceType)
        {
            try
            {
                return _container.ResolveAll(serviceType).Cast<object>();
            }
            catch
            {
                return new object[] { };
            }
        }

        void IDisposable.Dispose()
        {
            _scopeDisposable.Dispose();
        }
    }

    public class WindsorIoCContainer
    {

    }
}