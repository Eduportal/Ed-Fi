// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ValueBuilders
{
    public class BirthDateValueBuilder : IValueBuilder
    {
        private DateTime nextDate;

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            // Only handle DateTime values named "BirthDate"
            if (buildContext.TargetType != typeof(DateTime)
                || !buildContext.GetPropertyName().Equals("BirthDate", StringComparison.InvariantCultureIgnoreCase))
                return ValueBuildResult.NotHandled;

            return ValueBuildResult.WithValue(GetNextValue(), buildContext.LogicalPropertyPath);
        }

        public BirthDateValueBuilder()
        {
            this.Reset();
        }

        protected DateTime GetNextValue()
        {
            var value = this.nextDate;
            this.nextDate = this.nextDate.AddDays(1);

            // Don't generate a future date next time
            if (this.nextDate > DateTime.Today)
                Reset();

            return value;
        }

        public void Reset()
        {
            // Initializes a date based on today, but with "Local" date Kind.
            var minimumDateValue = DateTime.Today.AddYears(-10);

            // Initialize the "next" date value using a date Kind of "Unspecified"
            this.nextDate = new DateTime(minimumDateValue.Year, minimumDateValue.Month, minimumDateValue.Day);
        }

        public ITestObjectFactory Factory { get; set; }
    }
}