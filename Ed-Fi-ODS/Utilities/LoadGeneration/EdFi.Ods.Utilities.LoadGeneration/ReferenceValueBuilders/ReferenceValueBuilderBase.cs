using System;
using System.Collections.Generic;
using System.Threading;
using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using EdFi.TestObjects;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public abstract class ReferenceValueBuilderBase
    {
        private ILog _logger;

        public IExistingResourceReferenceProvider ExistingResourceReferenceProvider { get; set; }
        public IResourceCountManager ResourceCountManager { get; set; }
        public IResourcePersister ResourcePersister { get; set; }
        public IApiSdkReflectionProvider ApiSdkReflectionProvider { get; set; }

        protected ReferenceValueBuilderBase( )
        {
            _logger = LogManager.GetLogger(this.GetType());
        }

        public virtual ValueBuildResult TryBuild(BuildContext buildContext)
        {
            Type modelType;

            // TODO: Need unit tests for failure to locate model for the reference
            // Cannot find model type for the reference type, so this must be a case 
            // where we just have to populate the reference and continue
            if (!IsHandled(buildContext)
                || !ApiSdkReflectionProvider.TryGetModelType(buildContext.TargetType, out modelType))
                return ValueBuildResult.NotHandled;

            IDictionary<string, object> additionalConstraints;
            
            try
            {
                additionalConstraints = GetPropertyValueConstraints(buildContext) ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to create property value constraints due to an unexpected exception.", ex);
            }

            // TODO: GKM - Allow constraints to flow?
            //var propertyValueConstraints = new Dictionary<string, object>(
            //    buildContext.PropertyValueConstraints,
            //    StringComparer.InvariantCultureIgnoreCase);

            //propertyValueConstraints.MergeRange(additionalConstraints);

            // TODO: GKM - good or bad?  Context bleeding across (not just down).
            buildContext.PropertyValueConstraints.MergeRange(additionalConstraints);

            // If the generation progress of this resource is ahead of the progress on students...
            if (ResourceCountManager.GetProgressForResource(modelType.Name) >= ResourceCountManager.GetProgressForStudent())
            {
                // Attempt to reuse an existing reference
                var existingResource = ExistingResourceReferenceProvider.GetResourceReference(buildContext.TargetType, buildContext.PropertyValueConstraints);
                //var existingResource = ExistingResourceReferenceProvider.GetResourceReference(buildContext.TargetType, propertyValueConstraints);

                if (existingResource != null)
                {
                    _logger.DebugFormat("Progress in generating '{0}' is ahead (or level) with 'Student'.  Reusing an existing '{1}' reference.", modelType.Name, buildContext.TargetType.Name);
                    
                    return ValueBuildResult.WithValue(existingResource, buildContext.LogicalPropertyPath);
                }
            }

            var correlationId = Guid.NewGuid().ToString("n").GetHashCode();
            _logger.DebugFormat("Creating a new resource of type '{0}' to satisfy need for reference type '{1}' [correlationId={2}].", modelType.Name, buildContext.TargetType.Name, correlationId);

            // Create a new resource instance
            var createdResource = Factory.Create(string.Empty, modelType, buildContext.PropertyValueConstraints, null);
            //var createdResource = Factory.Create(string.Empty, modelType, propertyValueConstraints, null);

            // TODO: Need unit tests for this behavior, possible conversion to Try semantics?
            // If the underlying resoure couldn't be created... don't handle the reference specially.
            if (createdResource == null)
            {
                Type referenceType;
                
                if (ApiSdkReflectionProvider.TryGetReferenceType(modelType, out referenceType))
                {
                    if (Factory.CanCreate(referenceType))
                    {
                        _logger.DebugFormat(
                            "Resource '{0}' was not created, but the reference requested ('{1}') will be still be created.",
                            modelType.Name, buildContext.LogicalPropertyPath);

                        return ValueBuildResult.NotHandled;
                    }

                    // Cannot build the reference type
                    _logger.WarnFormat("Resource '{0}' was not created, and the reference requested ('{1}') also cannot be built (per the Factory's CanCreate delegate) and will be skipped.",
                        modelType.Name, buildContext.LogicalPropertyPath);

                    return ValueBuildResult.Skip(buildContext.LogicalPropertyPath);
                }
            }

            // Save the resource, and get its reference
            object resourceReference;
            ResourcePersister.PersistResource(createdResource, buildContext.PropertyValueConstraints, out resourceReference);

            _logger.DebugFormat("Finished creating/persisting '{0}' for reference '{1}' [correlationId={2}].", modelType.Name, buildContext.TargetType.Name, correlationId);

            return ValueBuildResult.WithValue(resourceReference, buildContext.LogicalPropertyPath);
        }

        protected abstract IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext);

        protected abstract bool IsHandled(BuildContext buildContext);

        public void Reset()
        {
            //There is nothing to reset.
        }

        public ITestObjectFactory Factory { get; set; }

        /// <summary>
        /// Uses the object factory to build a concrete education organization Id using the property name supplied as a string
        /// (e.g. SchoolId, LocalEducationAgencyId, EducationServiceCenterId, StateAgencyId, etc.).
        /// </summary>
        /// <param name="concretePropertyName">The property name that will be handled by an existing <see cref="IValueBuilder"/> 
        /// implementation.</param>
        /// <returns>The identifier returned by the factory.</returns>
        protected int BuildConcreteEducationOrganizationId(string concretePropertyName)
        {
            object value = Factory.Create(concretePropertyName, typeof(int), null, null);
            
            int concreteEducationOrganizationId;

            try
            {
                concreteEducationOrganizationId = (int) value;
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Unable to convert value '{0}' to an integer identifier.", value));
            }

            return concreteEducationOrganizationId;
        }
    }
}