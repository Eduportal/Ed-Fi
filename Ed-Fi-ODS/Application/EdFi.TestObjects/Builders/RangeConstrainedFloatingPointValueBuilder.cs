using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EdFi.TestObjects._Extensions;

namespace EdFi.TestObjects.Builders
{
    public class RangeConstrainedFloatingPointValueBuilder : IValueBuilder
    {
        private const int DefaultScale = 3;
        private Dictionary<Tuple<decimal, decimal>, decimal> nextValueByRange = new Dictionary<Tuple<decimal, decimal>, decimal>();

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            // TODO: Confirm this as obsolete behavior due to build context's TargetType now returning the underlying type
            //if (buildContext.TargetType.IsGenericType
            //    && buildContext.TargetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            //{
            //    // Reassign the nullable type to the underlying target type
            //    buildContext.TargetType = Nullable.GetUnderlyingType(buildContext.TargetType);
            //}

            if (buildContext.TargetType != typeof(decimal)
                && buildContext.TargetType != typeof(double)) // TODO: GKM - because SDK uses doubles. Not a great choice.
                return ValueBuildResult.NotHandled;

            if (buildContext.ContainingType == null)
                return ValueBuildResult.NotHandled;

            var propertyName = buildContext.LogicalPropertyPath.Split('.').Last();
            var propertyInfo = buildContext.ContainingType.GetPublicProperties().SingleOrDefault(p => p.Name == propertyName);

            var attribute = this.Factory.CustomAttributeProvider.GetCustomAttributes(propertyInfo, typeof(RangeAttribute), false)
                                   .Cast<RangeAttribute>()
                                   .FirstOrDefault();

            if (attribute == null)
                return ValueBuildResult.NotHandled;

            var valueAsDecimal = this.GetNextValue(Convert.ToDecimal(attribute.Minimum), Convert.ToDecimal(attribute.Maximum));

            object valueAsObject = 0;
 
            if (buildContext.TargetType == typeof(double))
                valueAsObject = Convert.ToDouble(valueAsDecimal);
            else if (buildContext.TargetType == typeof(decimal))
                valueAsObject = valueAsDecimal;
            else
            {
                // This should never happen, but guard against future changes falling through.
                throw new InvalidOperationException(string.Format(
                    "Type '{0}' should never have been handled by the '{1}' value builder.",
                    buildContext.TargetType.Name, this.GetType().Name));
            }

            return ValueBuildResult.WithValue(valueAsObject, buildContext.LogicalPropertyPath);
        }

        protected virtual decimal GetNextValue(decimal min, decimal max)
        {
            var key = Tuple.Create(min, max);
            
            decimal delta = this.GetDelta(min, max);
            decimal value;

            if (!this.nextValueByRange.ContainsKey(key))
                value = min;
            else
                value = this.nextValueByRange[key];

            this.nextValueByRange[key] = value + delta;

            // Skip the default value
            if (value == 0)
                value = this.GetNextValue(min, max);

            // Rollover after the max value
            if (value > max)
            {
                this.nextValueByRange.Remove(key);
                value = this.GetNextValue(min, max);
            }

            return value;
        }

        private decimal GetDelta(decimal min, decimal max)
        {
            return 1/(decimal) Math.Pow(10, this.GetScale(min, max));
        }

        protected int GetScale(decimal min, decimal max)
        {
            string[] minParts = (min - Math.Floor(min)).ToString().Split('.');
            string[] maxParts = (max - Math.Floor(max)).ToString().Split('.');

            int scale = Math.Max(
                minParts.Length == 1 ? 0 : minParts[1].Length,
                maxParts.Length == 1 ? 0 : maxParts[1].Length);

            if (scale == 0)
                return DefaultScale;

            return scale;
        }

        public void Reset() { }

        public ITestObjectFactory Factory { get; set; }
    }

    // TODO: Need unit tests
    public class RangeConstrainedRandomFloatingPointValueBuilder : RangeConstrainedFloatingPointValueBuilder
    {
        // TODO: Refactor to use IRandom, and inject through constructor
        private Random _random = new Random();

        protected override decimal GetNextValue(decimal min, decimal max)
        {
            double randomNumber = _random.NextDouble();

            var newValue = ((double) max - (double) min) * randomNumber + (double) min;

            int scale = GetScale(min, max);

            return (decimal) Math.Round(newValue, scale);
        }
    }
}