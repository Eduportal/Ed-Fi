using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Entities.Common.Validation;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines.Steps
{
    public class ValidateResourceModel<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : IHasResource<TResourceModel>
        where TResourceModel : IHasETag
        where TEntityModel : class
        where TResult : PipelineResultBase
    {
        private readonly IEnumerable<IObjectValidator> _validators;

        public ValidateResourceModel(IEnumerable<IObjectValidator> validators)
        {
            _validators = validators;
        }

        public void Execute(TContext context, TResult result)
        {
            var validationResults = _validators.ValidateObject(context.Resource);

            if (!validationResults.IsValid())
            {
                result.Exception = new ValidationException(
                    string.Format("Validation of '{0}' failed.\n{1}",
                        context.Resource.GetType().Name,
                        string.Join("\n", validationResults.GetAllMessages(indentLevel: 1))));
            }
        }
    }
}