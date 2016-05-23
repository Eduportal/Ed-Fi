// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Database;
using EdFi.Ods.Common._Installers.ComponentNaming;
using EdFi.Ods.Entities.Repositories.NHibernate.Architecture.Bytecode;
using EdFi.Ods.Security.AuthorizationStrategies.NHibernateConfiguration;
using NHibernate;
using NHibernate.Cfg;
using Environment = NHibernate.Cfg.Environment;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class NHibernateConfigurator
    {
        public Configuration Configure(IWindsorContainer container)
        {
            // Start the NHibernate configuration
            var configuration = new Configuration();

            // Get all the authorization strategy configurators
            var authorizationStrategyConfigurators = container.ResolveAll<INHibernateFilterConfigurator>();

            // Get all the filter definitions from all the configurators
            var allFilterDetails =
                (from c in authorizationStrategyConfigurators
                 from f in c.GetFilters()
                 select f)
                 .ToList();

            // Group the filters by name first (there can only be 1 "default" filter, but flexibility 
            // to apply same filter name with same parameters to different entities should be supported
            // (and is in fact supported below when fitlers are applied to individual entity mappings)
            var allFilterDetailsGroupedByName =
                from f in allFilterDetails
                group f by f.FilterDefinition.FilterName
                    into g
                    select g;

            // Add all the filter definitions to the NHibernate configuration
            foreach (var filterDetails in allFilterDetailsGroupedByName)
                configuration.AddFilterDefinition(filterDetails.First().FilterDefinition);

            // Configure the mappings
            configuration.Configure();

            // Apply the previously defined filters to the mappings
            foreach (var mapping in configuration.ClassMappings)
            {
                Type entityType = mapping.MappedClass;
                var properties = entityType.GetProperties();

                var applicableFilters = allFilterDetails
                    .Where(filterDetails => filterDetails.ShouldApply(entityType, properties));

                foreach (var filter in applicableFilters)
                {
                    mapping.AddFilter(
                        filter.FilterDefinition.FilterName,
                        filter.FilterDefinition.DefaultFilterCondition);
                }
            }

            // NHibernate Dependency Injection
            Environment.BytecodeProvider = new BytecodeProvider(container);

            configuration.AddCreateDateHooks();

            // Build and register the session factory with the container
            container.Register(Component
                .For<ISessionFactory>()
                .UsingFactoryMethod(configuration.BuildSessionFactory)
                .LifeStyle.Singleton);

            container.Register(Component
                .For<EdFiOdsConnectionProvider>()
                .DependsOn(Dependency
                    .OnComponent(
                        typeof(IDatabaseConnectionStringProvider),
                        typeof(IDatabaseConnectionStringProvider).GetServiceNameWithSuffix(
                            Databases.Ods.ToString()))));

            return configuration;
        }
    }
}