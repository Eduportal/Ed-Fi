// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Context;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Utilities.LoadGeneration.Interceptors;
using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.Ods.Utilities.LoadGeneration.ValueBuilders;
using EdFi.TestObjects;
using EdFi.TestObjects.Builders;

namespace EdFi.Ods.Utilities.LoadGeneration._Installers
{
    public class ConfigurationSpecificInstaller : RegistrationMethodsInstallerBase
    {
        protected virtual void RegisterDefaultImplementations(IWindsorContainer container)
        {
            container.Register(
                Classes
                    .FromAssemblyContaining<Marker_EdFi_Ods_Utilities_LoadGeneration>()
                    .Where(ShouldRegisterAsDefault)
                    .WithService
                    .DefaultInterfaces());
        }

        /// <summary>
        /// Provides an opportunity for a derived installer to prevent registration of types matching the
        /// default semantics (Foo for IFoo).
        /// </summary>
        /// <param name="type">The type to be evaluated.</param>
        /// <returns><b>true</b> if the type should be registered; otherwise <b>false</b>.</returns>
        protected virtual bool ShouldRegisterAsDefault(Type type)
        {
            if (typeof(IResourcePersister).IsAssignableFrom(type)
                || typeof(IValueBuilder).IsAssignableFrom(type)
                || typeof(ITestObjectFactory).IsAssignableFrom(type)
                || typeof(IUniqueIdFactory).IsAssignableFrom(type)
                || typeof(IApiSdkFacade).IsAssignableFrom(type)
                || typeof(IPropertyConstraintsCollectionFilter).IsAssignableFrom(type)
                )
                return false;

            return true;
        }

        protected virtual void RegisterContextSpecificReferenceValueBuilders(IWindsorContainer container)
        {
            container.Register(
                Classes.FromAssemblyContaining<Marker_EdFi_Ods_Utilities_LoadGeneration>()
               .BasedOn<IContextSpecificReferenceValueBuilder>()
               .WithServiceSelf()
               .WithServiceFromInterface()
               );
        }

        protected virtual void RegisterIApiSdkFacade(IWindsorContainer container)
        {
            // Register decorators first, then the final target.  Castle automatically handles injection of next component.
            container.Register<IApiSdkFacade, FilePersistedApiSdkFacadeDecorator>();
            container.Register<IApiSdkFacade, ApiSdkFacade>();
        }

        protected virtual void RegisterIContextStorage(IWindsorContainer container)
        {
            container.Register<IContextStorage, CallContextStorage>();
        }

        protected virtual void RegisterIPropertyConstraintsCollectionFilter(IWindsorContainer container)
        {
            container.Register<IPropertyConstraintsCollectionFilter, StaffUniqueIdPropertyConstraintsCollectionFilterDecorator>();
            container.Register<IPropertyConstraintsCollectionFilter, StudentUniqueIdPropertyConstraintsCollectionFilterDecorator>();
            container.Register<IPropertyConstraintsCollectionFilter, PropertyConstraintsCollectionFilter>();
        }

        protected virtual void RegisterIResourcePersister(IWindsorContainer container)
        {
            container.Register<IResourcePersister, StaffResourcePersisterDecorator>();
            container.Register<IResourcePersister, StudentResourcePersisterDecorator>();
            container.Register<IResourcePersister, ResourcePersister>();
        }

        protected virtual void RegisterIServiceLocator(IWindsorContainer container)
        {
            container.Register(
                Component.For<IServiceLocator>().UsingFactoryMethod(() =>
                {
                    var c = new CastleWindsorServiceLocator();
                    c.Initialize(container);
                    return c;
                }));
        }

        protected virtual void RegisterITestObjectFactory(IWindsorContainer container)
        {
            // Factory allows the definition of a component to activate the value builders,
            // giving them the ability to use dependency injection.
            BuilderFactory.Activator = () => new CastleWindsorActivator();

            var valueBuilderFactory = new BuilderFactory();
            
            // Custom builders for REST API load generation (in logical reverse order)
            valueBuilderFactory.AddToFront<EdFiAwareStringValueBuilder>();      // This generates GUID-based strings for very high likelihood of uniqueness
            valueBuilderFactory.AddToFront<StringTimeSpanValueBuilder>();
            valueBuilderFactory.AddToFront<RandomNumericValueBuilder>();                        // We want random numbers ...
            valueBuilderFactory.AddToFront<RangeConstrainedRandomFloatingPointValueBuilder>();  // ... but we first need to make sure floating point values are constrained.
            valueBuilderFactory.AddToFront<BirthDateValueBuilder>();
            valueBuilderFactory.AddToFront<RandomEducationOrganizationIdValueBuilder>();
            valueBuilderFactory.AddToFront<RandomLocalEducationAgencyIdValueBuilder>();
            valueBuilderFactory.AddToFront<RandomSchoolIdValueBuilder>();
            valueBuilderFactory.AddToFront<PersonInstanceUniqueIdValueBuilder>();
            valueBuilderFactory.AddToFront<GeneralReferenceStudentUniqueIdValueBuilder>();
            valueBuilderFactory.AddToFront<SchoolYearValueBuilder>();
            valueBuilderFactory.AddToFront<EdFiDescriptorValueBuilder>();
            valueBuilderFactory.AddToFront<EdFiTypeValueBuilder>();
            
            // Order on these is important (Standard builder must be AFTER Context-Specific in execution order)
            valueBuilderFactory.AddToFront<StandardReferenceValueBuilder>();
            valueBuilderFactory.AddToFront<ContextSpecificReferenceValueBuilder>();

            // This is a specially handled reference type, must appear in front of the other reference builders
            valueBuilderFactory.AddToFront<SchoolYearTypeReferenceValueBuilder>();

            // Add the skippers
            valueBuilderFactory.AddToFront<EtagPropertySkipper>();
            valueBuilderFactory.AddToFront<IdPropertySkipper>();
            valueBuilderFactory.AddToFront<ReferenceLinkPropertySkipper>();
            valueBuilderFactory.AddToFront<SelfRecursiveRelationshipPropertySkipper>();

            // Add the context setters
            valueBuilderFactory.AddToFront<EstablishEducationOrganizationAuthorizationContextForPersonUniqueId>();

            container.Register(
                Component.For<ITestObjectFactory>()
                //.LifestylePerThread() // Note: This may not have any effect due to singleton instance lifestyles of component where this is injected
                .UsingFactoryMethod(() =>
                {
                    var factory = new TestObjectFactory(
                        valueBuilderFactory.GetBuilders(),
                        new SystemActivator(),
                        //new CustomAttributeProvider());
                        new EdFiAwareDecimalRangeAttributeProviderDecorator(
                            new CustomAttributeProvider()));

                    // Avoid unwanted PK constraint violations in child lists due to limited 
                    factory.CollectionCount = 1;

                    // Add resoures that we don't want to try to generate for various reasons
                    factory.CanCreate = type =>
                    {
                        // The SDK doesn't generate a "reference" back to LearningStandard here, 
                        // and this causes FK issues (since the value builders can generate an
                        // appropriate value without that pattern used), so don't build these
                        if (type.Name.Equals("LearningStandardPrerequisiteLearningStandard", StringComparison.InvariantCultureIgnoreCase)
                            // Throws 500 errors
                            || type.Name.Equals("LearningObjective", StringComparison.InvariantCultureIgnoreCase)
                            || type.Name.Equals("LearningObjectiveReference", StringComparison.InvariantCultureIgnoreCase)
                            // References LearningObject
                            || type.Name.Equals("ProgramLearningObjective", StringComparison.InvariantCultureIgnoreCase)
                            // References LearningObject
                            || type.Name.Equals("ObjectiveAssessmentLearningObjective", StringComparison.InvariantCultureIgnoreCase)
                            // References LearningObject
                            || type.Name.Equals("StudentLearningObjective", StringComparison.InvariantCultureIgnoreCase)
                            || type.Name.Equals("StudentLearningObjectiveReference", StringComparison.InvariantCultureIgnoreCase)
                            // Child item of ReportCard that is an array of StudentLearningObjectiveReferences which cannot be created
                            || type.Name.Equals("ReportCardStudentLearningObjective", StringComparison.InvariantCultureIgnoreCase)
                            // References LearningObject
                            || type.Name.Equals("CourseLearningObjective", StringComparison.InvariantCultureIgnoreCase)
                            // References LearningObject
                            || type.Name.Equals("GradebookEntryLearningObjective", StringComparison.InvariantCultureIgnoreCase)
                            // No data available for "AccountCodeDescriptor", a required field
                            || type.Name.Equals("AccountCode", StringComparison.InvariantCultureIgnoreCase)
                            // No data available for "ServiceDescriptor", a required field
                            || type.Name.Equals("ProgramService", StringComparison.InvariantCultureIgnoreCase)
                            || type.Name.Equals("StudentProgramAssociationService", StringComparison.InvariantCultureIgnoreCase)
                            // Self-recursive many-to-many doesn't properly follow conventions (back reference on child object isn't modeled as a reference, just the PK properties)
                            || type.Name.Equals("EducationContentDerivativeSourceEducationContent", StringComparison.InvariantCultureIgnoreCase)
                            // Inherited types don't properly follow conventions (references get flattened out into properties)
                            || type.Name.Equals("StudentCTEProgramAssociation", StringComparison.InvariantCultureIgnoreCase)
                            || type.Name.Equals("StudentMigrantEducationProgramAssociation", StringComparison.InvariantCultureIgnoreCase)
                            || type.Name.Equals("StudentSpecialEducationProgramAssociation", StringComparison.InvariantCultureIgnoreCase)
                            || type.Name.Equals("StudentTitleIPartAProgramAssociation", StringComparison.InvariantCultureIgnoreCase)
                            // We don't create any Education organizations
                            || type.Name.Equals("StateEducationAgency", StringComparison.InvariantCultureIgnoreCase)
                            || type.Name.Equals("EducationServiceCenter", StringComparison.InvariantCultureIgnoreCase)
                            || type.Name.Equals("LocalEducationAgency", StringComparison.InvariantCultureIgnoreCase)
                            || type.Name.Equals("School", StringComparison.InvariantCultureIgnoreCase)
                            || type.Name.Equals("EducationOrganizationNetwork", StringComparison.InvariantCultureIgnoreCase)
                            || type.Name.Equals("EducationOrganizationNetworkAssociation", StringComparison.InvariantCultureIgnoreCase)
                            // Possible authorization problem?  Returns Forbidden when staff is associated with LEA, but the accountReference ed org is a school, but in the same LEA
                            || type.Name.Equals("Payroll", StringComparison.InvariantCultureIgnoreCase)
                            || type.Name.Equals("ContractedStaff", StringComparison.InvariantCultureIgnoreCase)
                            // Unknown authorization problems
                            || type.Name.Equals("StudentInterventionAssociation", StringComparison.InvariantCultureIgnoreCase)
                            || type.Name.Equals("StaffCohortAssociation", StringComparison.InvariantCultureIgnoreCase)
                            // StudentProgramAssociation fails due to use of EdOrg on relationship, and with SchoolId and EdOrgId in context, filtering on student UniqueId happens on the School, but the EdOrg doesn't jive with the School.
                            || type.Name.Equals("StudentProgramAssociation", StringComparison.InvariantCultureIgnoreCase)
                            // Relies on StudentProgramAssociation relationship for establishing studentUniqueId, since studentSectionAssociationReference cannot be supplied due to bug in REST API
                            || type.Name.Equals("StudentCompetencyObjective", StringComparison.InvariantCultureIgnoreCase)
                            // Relies on a reference to StudentCompetencyObjective
                            || type.Name.Equals("ReportCardStudentCompetencyObjective", StringComparison.InvariantCultureIgnoreCase)
                            // Not supported by current authorization model (there's no ed org present for relationship-based authorization)
                            || type.Name.Equals("StudentAssessment", StringComparison.InvariantCultureIgnoreCase)
                            // Uninvestigated problem with ReportCard related to ReportCardGrade not being found, causing subsequent cascading failures
                            || type.Name.Equals("ReportCardGrade", StringComparison.InvariantCultureIgnoreCase)
                            // Student/Parent authorization not yet supported (time constraints)
                            || type.Name.Equals("StudentParentAssociation", StringComparison.InvariantCultureIgnoreCase)
                            
                            // Removed from Full Coverage file as well
                            // LocalEducationAgencyFederalFunds,,1
                            // StateEducationAgencyFederalFunds,,1
                            )
                            return false;

                        return true;
                    };

                    return factory;
                }));
        }

        protected virtual void RegisterIUniqueIdFactory(IWindsorContainer container)
        {
            container.Register<IUniqueIdFactory, EduIdBasedUniqueIdFactory>();
        }

        protected virtual void RegisterValueBuilders(IWindsorContainer container)
        {
            // Register all value builders using the IValueBuilder interface, and an interceptor to add logging behavior
            container.Register(
                Classes.FromAssemblyContaining<Marker_EdFi_Ods_Utilities_LoadGeneration>()
                    .BasedOn<IValueBuilder>()
                    .WithService.Self()
                    .WithServiceFirstInterface()
                    .Configure(c => c.Interceptors<ValueBuilderLoggingInterceptor>())
                    );

            container.Register(
                Classes.FromAssemblyContaining<Marker_EdFi_TestObjects>()
                    .BasedOn<IValueBuilder>()
                    .WithService.Self()
                    .WithServiceFirstInterface()
                    .Configure(c => c.Interceptors<ValueBuilderLoggingInterceptor>())
                    );
        }
    }
}