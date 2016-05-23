using System;

namespace EdFi.TestObjects.Builders
{
    // TODO: This behavior of building an arbitrary string should be refactored to use compositional behavior, rather than inheritance
    public abstract class GuidBasedExplicitLengthStringValueBuilderBase : ExplicitLengthStringValueBuilderBase
    {
        /// <summary>
        /// Generate strings values by concatenating newly generated GUIDs together.
        /// </summary>
        /// <param name="desiredLength"></param>
        /// <returns></returns>
        protected override string BuildStringValue(int desiredLength)
        {
            string value = string.Empty;

            int targetLength = Math.Min(desiredLength, 8192);

            while (value.Length < targetLength)
                value += Guid.NewGuid().ToString("n");

            return value.Substring(0, targetLength);
        }
    }
}