using System;

namespace EdFi.TestObjects.Builders
{
    public class DateTimeBuilder : NullableValueBuilderBase<DateTime>
    {
        private DateTime nextDate;

        public DateTimeBuilder()
        {
            this.Reset();
        }

        protected override DateTime GetNextValue()
        {
            var value = this.nextDate;
            this.nextDate = this.nextDate.AddDays(1);
            return value;
        }

        public override void Reset()
        {
            // Initializes a date based on today, but with "Local" date Kind.
            var nextDateLocal = DateTime.Today.AddYears(-10);

            // Initialize the "next" date value using a date Kind of "Unspecified"
            this.nextDate = new DateTime(nextDateLocal.Year, nextDateLocal.Month, nextDateLocal.Day);
        }
    }
}