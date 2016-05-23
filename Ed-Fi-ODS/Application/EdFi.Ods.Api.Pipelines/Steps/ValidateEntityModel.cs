using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Entities.Common.Validation;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines.Steps
{
    public class ValidateEntityModel<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : IHasPersistentModel<TEntityModel>
        where TResourceModel : IHasETag
        where TEntityModel : class
        where TResult : PipelineResultBase
    {
        private readonly IEnumerable<IObjectValidator> _validators;

        public ValidateEntityModel(IEnumerable<IObjectValidator> validators)
        {
            _validators = validators;
        }

        public void Execute(TContext context, TResult result)
        {
            var validationResults = _validators.ValidateObject(context.PersistentModel);

            if (!validationResults.IsValid())
            {
                result.Exception = new ValidationException(
                    string.Format("Validation of '{0}' failed.\n{1}", 
                        context.PersistentModel.GetType().Name,
                        string.Join("\n", validationResults.GetAllMessages(indentLevel: 1))));
            }
        }
    }
}