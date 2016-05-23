using System;
using System.Collections.Generic;

namespace EdFi.TestObjects.Builders
{
    public abstract class ExplicitLengthStringValueBuilderBase : IValueBuilder
    {
        private int counter;
        private char[] _digitsAndCharacters;

        protected ExplicitLengthStringValueBuilderBase()
        {
            var chars = new List<char>();
            for(char c = '0'; c <= '9'; c++)
                chars.Add(c);
            for(char c = 'A'; c <= 'Z'; c++)
                chars.Add(c);
            for(char c = 'a'; c <= 'z'; c++)
                chars.Add(c);
            this._digitsAndCharacters = chars.ToArray();
        }

        public ITestObjectFactory Factory { get; set; }

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType != typeof (string))
                return ValueBuildResult.NotHandled;

            if (buildContext.ContainingType == null)
                return ValueBuildResult.NotHandled;

            int desiredLength;
            
            if (!TryGetLength(buildContext, out desiredLength))
                return ValueBuildResult.NotHandled;

            var value = BuildStringValue(desiredLength);

            string finalValue = value;

            // Generated value is too short - fill it with "x" characters
            if (value.Length < desiredLength)
            {
                var fillLength = desiredLength - value.Length;
                var eightKb = (int)Math.Pow(2, 10) * 8;
                var safeFillLength = Math.Min(fillLength, eightKb);
                var fill = new string('x', safeFillLength);

                finalValue = value + fill;
            }
            else if (value.Length > desiredLength)
            {
                finalValue = value.Substring(0, desiredLength);
            }

            return ValueBuildResult.WithValue(finalValue, buildContext.LogicalPropertyPath);
        }

        protected virtual string BuildStringValue(int desiredLength)
        {
            var firstChar = this._digitsAndCharacters[this.counter % this._digitsAndCharacters.Length];
            var value = string.Format("{0}-String-{1}", firstChar, this.counter++);

            // If value is longer than desired, trim to length and return
            if (desiredLength < value.Length)
            {
                return value.Substring(0, desiredLength);
            }

            return value;
        }

        /// <summary>
        /// Attempts to get the explicit length of the string to generate.
        /// </summary>
        /// <param name="buildContext">The context of the current value builder.</param>
        /// <returns></returns>
        protected abstract bool TryGetLength(BuildContext buildContext, out int length);

        public void Reset()
        {
            this.counter = 0;
        }
    }
}