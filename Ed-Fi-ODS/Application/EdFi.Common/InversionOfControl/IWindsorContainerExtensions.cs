using System;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;

namespace EdFi.Common.InversionOfControl
{
    /// <summary>
    /// Provides extensions methods to assist with Castle Windsor container configuration.
    /// </summary>
    public static class IWindsorContainerExtensions
    {
        /// <summary>
        /// Adds support to the return an array of all components satisfying an interface.
        /// </summary>
        /// <remarks>
        /// Will return an empty array if there are no configured components
        /// </remarks>
        /// <param name="container">The Castle Windows container.</param>
        public static void AddSupportForEmptyCollections(this IWindsorContainer container)
        {
            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel, true));
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, true));
        }
    }
}
