namespace EdFi.Ods.Admin.UITests.Support.Coypu
{
    using System;

    using global::Coypu;

    public static class Make
    {
        public static OptionsBuilder Options
        {
            get { return new OptionsBuilder(); }
        }
    }

    public class OptionsBuilder
    {
        private readonly Options _options = new Options();

        public static implicit operator Options(OptionsBuilder builder)
        {
            return builder._options;
        }

        public OptionsBuilder Wait(TimeSpan timespan)
        {
            this._options.Timeout = timespan;
            return this;
        }

        public OptionsBuilder Consider_Invisible_Elements
        {
            get
            {
                this._options.ConsiderInvisibleElements = true;
                return this;
            }
        }

        public OptionsBuilder Do_It_Now
        {
            get
            {
                this._options.Timeout = TimeSpan.Zero;
                this._options.RetryInterval = TimeSpan.Zero;
                return this;
            }
        }
    }
}