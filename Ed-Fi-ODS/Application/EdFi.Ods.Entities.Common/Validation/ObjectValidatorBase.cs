using EdFi.Ods.Common;

namespace EdFi.Ods.Entities.Common.Validation
{
    public abstract class ObjectValidatorBase
    {
        protected void SetValid()
        {
            SetValidationContext(true);
        }

        protected void SetInvalid()
        {
            SetValidationContext(false);
        }

        private void SetValidationContext(bool isValid)
        {
            var validationState = ValidationState.Current;

            if (validationState != null)
                validationState.IsValid = isValid;
        }
    }
}