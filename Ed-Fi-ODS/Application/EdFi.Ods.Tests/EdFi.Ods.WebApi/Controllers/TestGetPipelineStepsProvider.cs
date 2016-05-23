using EdFi.Ods.Api.Pipelines.Factories;

namespace EdFi.Ods.Tests.EdFi.Ods.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.SqlClient;
    using System.Reflection;

    using global::EdFi.Common.Security;
    using global::EdFi.Ods.Api.Common;
    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Common.Exceptions;
    using global::EdFi.Ods.Entities.Common;
    using global::EdFi.Ods.Pipelines;
    using global::EdFi.Ods.Pipelines.Common;
    using global::EdFi.Ods.Pipelines.GetByKey;
    using global::EdFi.Ods.Pipelines.GetMany;
    using global::EdFi.Ods.Pipelines.Put;

    using NHibernate.Exceptions;

    public class SimpleResourceCreationStep<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : IHasPersistentModel<TEntityModel>, IHasIdentifier, IHasETag
        where TResult : PipelineResultBase, IHasResource<TResourceModel>
        where TResourceModel : IHasETag, IHasIdentifier, new()
        where TEntityModel : class, IMappable
    {
        public void Execute(TContext context, TResult result)
        {
            var resource = new TResourceModel {Id = context.Id, ETag = context.ETag};
            result.Resource = resource;
        }
    }

    public class PersistNewModel<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : PutContext<TResourceModel, TEntityModel>
        where TResult : PutResult
        where TEntityModel : class, IHasIdentifier, new()
        where TResourceModel : IHasETag, IMappable
    {
        private readonly IETagProvider etagProvider;

        public PersistNewModel(IETagProvider etagProvider)
        {
            this.etagProvider = etagProvider;
        }

        public void Execute(TContext context, TResult result)
        {
            try
            {
                var model = new TEntityModel();
                context.Resource.Map(model);
                result.ResourceWasCreated = true;
                result.ETag = this.etagProvider.GetETag(context.Resource.ETag);
                result.ResourceId = model.Id == default(Guid) ? new Guid(context.Resource.ETag) : model.Id;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
        }
    }
    
    public class PersistExistingModel<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : PutContext<TResourceModel, TEntityModel>
        where TResult : PutResult
        where TEntityModel : class, IHasIdentifier, new()
        where TResourceModel : IHasETag, IMappable, IHasIdentifier
    {
        private readonly IETagProvider etagProvider;

        public PersistExistingModel(IETagProvider etagProvider)
        {
            this.etagProvider = etagProvider;
        }

        public void Execute(TContext context, TResult result)
        {
            try
            {
                result.ResourceWasCreated = false;
                result.ResourceWasUpdated = true;
                result.ETag = this.etagProvider.GetETag(context.Resource.ETag);
                result.ResourceId = context.Resource.Id == default(Guid)
                    ? new Guid(context.Resource.ETag)
                    : context.Resource.Id;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
        }
    }

    public class SqlExceptionStep<TContext, TResult, TResourceModel, TEntityModel> :
    ExceptionStep<TContext, TResult, TResourceModel, TEntityModel, SqlException>
    where TResult : PipelineResultBase
    {
    }

    public class DeleteReferentialExceptionStep<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TResult : PipelineResultBase
    {
        public void Execute(TContext context, TResult result)
        {
            result.Exception = new GenericADOException(string.Empty,
                ExceptionCreator.Create(typeof(SqlException), @"The DELETE statement conflicted with the REFERENCE constraint ""FK_SomeForeignKey"". The conflict occurred in database ""DB_NAME"", table ""schema.TableName"", column 'ColumnName'."));
        }
    }

    public class UpdateReferentialExceptionStep<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TResult : PipelineResultBase
    {
        public void Execute(TContext context, TResult result)
        {
            result.Exception = new GenericADOException(string.Empty,
                ExceptionCreator.Create(typeof (SqlException),
                    @"The UPDATE statement conflicted with the FOREIGN KEY constraint ""FK_SomeForeignKey"". The conflict occurred in database ""DB_NAME"", table ""schema.TableName"", column 'ColumnName'."));
        }
    }

    public class InsertReferentialExceptionStep<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
    where TResult : PipelineResultBase
    {
        public void Execute(TContext context, TResult result)
        {
            result.Exception = new GenericADOException(string.Empty,
                ExceptionCreator.Create(typeof(SqlException), @"The INSERT statement conflicted with the FOREIGN KEY constraint ""FK_SomeForeignKey"". The conflict occurred in database ""DB_NAME"", table ""schema.TableName"", column 'ColumnName'."));
        }
    }

    public class InsertUniqueIdExceptionStep<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
where TResult : PipelineResultBase
    {
        public void Execute(TContext context, TResult result)
        {
            result.Exception = new GenericADOException(string.Empty,
                ExceptionCreator.Create(typeof(SqlException), @"Violation of UNIQUE KEY constraint 'FK_SomeIndex'. Cannot insert duplicate key in object 'edfi.SomeTable'."));
        }
    }

    public class ValidationExceptionStep<TContext, TResult, TResourceModel, TEntityModel> :
        ExceptionStep<TContext, TResult, TResourceModel, TEntityModel, ValidationException>
        where TResult : PipelineResultBase
    {
    }

    public class GenericAdoExceptionStep<TContext, TResult, TResourceModel, TEntityModel> :
        ExceptionStep<TContext, TResult, TResourceModel, TEntityModel, GenericADOException>
        where TResult : PipelineResultBase
    {
    }

    public class ConcurrencyExceptionStep<TContext, TResult, TResourceModel, TEntityModel> :
        ExceptionStep<TContext, TResult, TResourceModel, TEntityModel, ConcurrencyException>
        where TResult : PipelineResultBase
    {
    }

    public class NotModifiedExceptionStep<TContext, TResult, TResourceModel, TEntityModel> :
        ExceptionStep<TContext, TResult, TResourceModel, TEntityModel, NotModifiedException>
        where TResult : PipelineResultBase
    {
    }

    public class NotFoundExceptionStep<TContext, TResult, TResourceModel, TEntityModel> :
        ExceptionStep<TContext, TResult, TResourceModel, TEntityModel, NotFoundException>
        where TResult : PipelineResultBase
    {
    }

    public class EdFiSecurityExceptionStep<TContext, TResult, TResourceModel, TEntityModel> :
        ExceptionStep<TContext, TResult, TResourceModel, TEntityModel, EdFiSecurityException>
        where TResult : PipelineResultBase
    {
    }

    public class UnhandledExceptionStep<TContext, TResult, TResourceModel, TEntityModel> :
        ExceptionStep<TContext, TResult, TResourceModel, TEntityModel, Exception>
        where TResult : PipelineResultBase
    {
    }

    public abstract class ExceptionStep<TContext, TResult, TResourceModel, TEntityModel, TException> : IStep<TContext, TResult>
        where TResult : PipelineResultBase
        where TException : Exception
    {
        public void Execute(TContext context, TResult result)
        {
            result.Exception = ExceptionCreator.Create(typeof(TException), "Exception for testing");
        }
    }

    public static class ExceptionCreator
    {
        public static Exception Create(Type exceptionType, string message)
        {
            var exception = (Exception) System.Runtime.Serialization.FormatterServices.GetUninitializedObject(exceptionType);
            var m = typeof(Exception).GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
            m.SetValue(exception, message);
            return exception;
        }
    }

    public class SimpleGetByKeyResourceCreationStep<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : GetByKeyContext<TResourceModel, TEntityModel>
        where TResult : PipelineResultBase, IHasResource<TResourceModel>
        where TResourceModel : IHasETag, new()
        where TEntityModel : class, IHasIdentifier, IDateVersionedEntity
    {
        public void Execute(TContext context, TResult result)
        {
            result.Resource = context.Resource;
        }
    }

    public class SimpleGetManyResourceCreationStep<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : GetManyContext<TResourceModel, TEntityModel>
        where TResult : PipelineResultBase, IHasResources<TResourceModel>
        where TResourceModel : IHasETag, new()
        where TEntityModel : class, IHasIdentifier, IDateVersionedEntity
    {
        public void Execute(TContext context, TResult result)
        {
            result.Resources = new List<TResourceModel>();
            for (var i = 0; i < (context.QueryParameters.Limit ?? 25); i++)
            {
                result.Resources.Add(new TResourceModel());
            }
        }
    }

    public class SetNullResourceStep<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TResult : IHasResource<TResourceModel>
        where TResourceModel : IHasETag
    {
        public void Execute(TContext context, TResult result)
        {
            result.Resource = default(TResourceModel);
        }
    }

    public class EmptyResourceStep<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
    {
        public void Execute(TContext context, TResult result)
        {
        }
    }

    public class SingleStepPipelineProviderForTest : IGetPipelineStepsProvider, IGetByKeyPipelineStepsProvider, 
        IPutPipelineStepsProvider, IDeletePipelineStepsProvider, IGetBySpecificationPipelineStepsProvider
    {
        private Type _type;
        public SingleStepPipelineProviderForTest(Type type)
        {
            this._type = type;
        }

        public Type[] GetSteps()
        {
            return new[]
            {
                this._type,
            };
        }
    }
}