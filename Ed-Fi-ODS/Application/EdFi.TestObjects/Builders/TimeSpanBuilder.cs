using System;

namespace EdFi.TestObjects.Builders
{
    public class TimeSpanBuilder : NullableValueBuilderBase<TimeSpan>
    {
        private DateTime fromDate = DateTime.Today.AddYears(-10);
        private int offsetMinutes = 1;

        protected override TimeSpan GetNextValue()
        {
            const int minutesPerDay = 24 * 60;

            var toDate = this.fromDate.AddMinutes(this.offsetMinutes);
            this.offsetMinutes = (this.offsetMinutes + 1) % minutesPerDay;
            return toDate - this.fromDate;
        }

        public override void Reset()
        {
            this.fromDate = DateTime.Today.AddYears(-10);
        }
    }
}