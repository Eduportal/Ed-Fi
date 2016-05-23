// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

namespace EdFi.Ods.Tests.EdFi.Ods.Common
{
    using System;

    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Tests._Bases;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using Should;

    public class UtcTimeConverterTests
    {
        public class Times
        {
            [JsonConverter(typeof(UtcTimeConverter))]
            public TimeSpan TimeWithoutSeconds { get; set; }

            [JsonConverter(typeof(UtcTimeConverter))]
            public TimeSpan TimeWithSeconds { get; set; }

            [JsonConverter(typeof(UtcTimeConverter))]
            public TimeSpan? TimeWithPositiveUtcOffset { get; set; }

            [JsonConverter(typeof(UtcTimeConverter))]
            public TimeSpan? TimeWithNegativeUtcOffset { get; set; }

            [JsonConverter(typeof(UtcTimeConverter))]
            public TimeSpan TimeWithZSuffix { get; set; }

            [JsonConverter(typeof(UtcTimeConverter))]
            public TimeSpan TimeWithZSuffixWithoutSeconds { get; set; }
        }
        
        public class NullableTimes
        {
            [JsonConverter(typeof(UtcTimeConverter))]
            public TimeSpan? TimeWithoutSeconds { get; set; }

            [JsonConverter(typeof(UtcTimeConverter))]
            public TimeSpan? TimeWithSeconds { get; set; }

            [JsonConverter(typeof(UtcTimeConverter))]
            public TimeSpan? TimeWithPositiveUtcOffset { get; set; }
            
            [JsonConverter(typeof(UtcTimeConverter))]
            public TimeSpan? TimeWithNegativeUtcOffset { get; set; }

            [JsonConverter(typeof(UtcTimeConverter))]
            public TimeSpan? TimeWithZSuffix { get; set; }

            [JsonConverter(typeof(UtcTimeConverter))]
            public TimeSpan? TimeWithZSuffixWithoutSeconds { get; set; }
        }

        [TestFixture]
        public class When_serializing_and_deserializing_time_values_from_JSON_to_nullable_or_non_nullable_times : TestFixtureBase
        {
            private Times times;
            private NullableTimes nullableTimes;
            private string serializedJson;
            private string serializedJsonForNullableTimes;
            private Times rolloverTimes;

            protected override void ExecuteBehavior()
            {
                this.times = JsonConvert.DeserializeObject<Times>(@"
{
    ""TimeWithoutSeconds"":""07:57"", 
    ""TimeWithSeconds"":""07:57:32"", 
    ""TimeWithPositiveUtcOffset"":""07:57:32+05:00"", 
    ""TimeWithNegativeUtcOffset"":""07:57:32-05:00"", 
    ""TimeWithZSuffix"":""07:57:32Z"", 
    ""TimeWithZSuffixWithoutSeconds"":""07:57Z"" 
}");

                this.nullableTimes = JsonConvert.DeserializeObject<NullableTimes>(@"
{
    ""TimeWithoutSeconds"":""07:57"", 
    ""TimeWithSeconds"":""07:57:32"", 
    ""TimeWithPositiveUtcOffset"":""07:57:32+05:00"", 
    ""TimeWithNegativeUtcOffset"":""07:57:32-05:00"", 
    ""TimeWithZSuffix"":""07:57:32Z"", 
    ""TimeWithZSuffixWithoutSeconds"":""07:57Z"" 
}");

                this.rolloverTimes = JsonConvert.DeserializeObject<Times>(@"
{
    ""TimeWithoutSeconds"":""07:57"", 
    ""TimeWithSeconds"":""07:57:32"", 
    ""TimeWithPositiveUtcOffset"":""03:57:32+05:00"", 
    ""TimeWithNegativeUtcOffset"":""20:57:32-05:00"", 
    ""TimeWithZSuffix"":""07:57:32Z"", 
    ""TimeWithZSuffixWithoutSeconds"":""07:57Z"" 
}");

                this.serializedJson = JsonConvert.SerializeObject(this.times);
                this.serializedJsonForNullableTimes = JsonConvert.SerializeObject(this.nullableTimes);
            }

            [Test]
            public void Should_serialize_times_to_JSON_correctly()
            {
                // Approval-style test
                this.serializedJson.ShouldEqual(@"{""TimeWithoutSeconds"":""07:57:00"",""TimeWithSeconds"":""07:57:32"",""TimeWithPositiveUtcOffset"":""02:57:32"",""TimeWithNegativeUtcOffset"":""12:57:32"",""TimeWithZSuffix"":""07:57:32"",""TimeWithZSuffixWithoutSeconds"":""07:57:00""}");
            }

            [Test]
            public void Should_serialize_nullable_times_to_JSON_correctly()
            {
                // Approval-style test
                this.serializedJsonForNullableTimes.ShouldEqual(@"{""TimeWithoutSeconds"":""07:57:00"",""TimeWithSeconds"":""07:57:32"",""TimeWithPositiveUtcOffset"":""02:57:32"",""TimeWithNegativeUtcOffset"":""12:57:32"",""TimeWithZSuffix"":""07:57:32"",""TimeWithZSuffixWithoutSeconds"":""07:57:00""}");
            }

            [Test]
            public void Should_read_unspecified_time_without_seconds_as_GMT()
            {
                this.times.TimeWithoutSeconds.ShouldEqual(new TimeSpan(7, 57, 0));
                this.nullableTimes.TimeWithoutSeconds.ShouldEqual(new TimeSpan(7, 57, 0));
            }
 
            [Test]
            public void Should_read_unspecified_time_with_seconds_as_GMT()
            {
                this.times.TimeWithSeconds.ShouldEqual(new TimeSpan(7, 57, 32));
                this.nullableTimes.TimeWithSeconds.ShouldEqual(new TimeSpan(7, 57, 32));
            }  
 
            [Test]
            public void Should_read_and_convert_time_with_positive_UTC_offset_to_GMT()
            {
                this.times.TimeWithPositiveUtcOffset.ShouldEqual(new TimeSpan(2, 57, 32));
                this.nullableTimes.TimeWithPositiveUtcOffset.ShouldEqual(new TimeSpan(2, 57, 32));
            }  
 
            [Test]
            public void Should_read_and_convert_time_with_negative_UTC_offset_to_GMT()
            {
                this.times.TimeWithNegativeUtcOffset.ShouldEqual(new TimeSpan(12, 57, 32));
                this.nullableTimes.TimeWithNegativeUtcOffset.ShouldEqual(new TimeSpan(12, 57, 32));
            }  
 
            [Test]
            public void Should_read_time_with_Z_suffix_as_GMT()
            {
                this.times.TimeWithZSuffix.ShouldEqual(new TimeSpan(7, 57, 32));
                this.nullableTimes.TimeWithZSuffix.ShouldEqual(new TimeSpan(7, 57, 32));
            }       

            [Test]
            public void Should_read_time_with_Z_suffix_and_no_seconds_as_GMT()
            {
                this.times.TimeWithZSuffixWithoutSeconds.ShouldEqual(new TimeSpan(7, 57, 0));
                this.nullableTimes.TimeWithZSuffixWithoutSeconds.ShouldEqual(new TimeSpan(7, 57, 0));
            }

            [Test]
            public void Should_rollover_positive_UTC_offset_times_based_on_24_hour_clock()
            {
                this.rolloverTimes.TimeWithPositiveUtcOffset.ShouldEqual(new TimeSpan(22, 57, 32));
            }

            [Test]
            public void Should_rollover_negative_UTC_offset_times_based_on_24_hour_clock()
            {
                this.rolloverTimes.TimeWithNegativeUtcOffset.ShouldEqual(new TimeSpan(1, 57, 32));
            }
        }
        
        [TestFixture]
        public class When_deserializing_null_values_to_nullable_time_value_from_JSON : TestFixtureBase
        {
            private NullableTimes nullableTimes;
            private string serializedJsonForNullableTimes;

            protected override void ExecuteBehavior()
            {
                this.nullableTimes = JsonConvert.DeserializeObject<NullableTimes>(@"
{
    ""TimeWithoutSeconds"":null, 
    ""TimeWithSeconds"":null, 
    ""TimeWithPositiveUtcOffset"":null, 
    ""TimeWithNegativeUtcOffset"":null, 
    ""TimeWithZSuffix"":null, 
    ""TimeWithZSuffixWithoutSeconds"":null 
}");

                this.serializedJsonForNullableTimes = JsonConvert.SerializeObject(this.nullableTimes);
                Console.WriteLine(this.serializedJsonForNullableTimes);
            }

            [Test]
            public void Should_serialize_nullable_times_to_JSON_correctly()
            {
                // Approval-style test
                this.serializedJsonForNullableTimes.ShouldEqual(@"{""TimeWithoutSeconds"":null,""TimeWithSeconds"":null,""TimeWithPositiveUtcOffset"":null,""TimeWithNegativeUtcOffset"":null,""TimeWithZSuffix"":null,""TimeWithZSuffixWithoutSeconds"":null}");
            }

            [Test]
            public void Should_read_null_values_as_null()
            {
                this.nullableTimes.TimeWithoutSeconds.ShouldBeNull();
                this.nullableTimes.TimeWithSeconds.ShouldBeNull();
                this.nullableTimes.TimeWithPositiveUtcOffset.ShouldBeNull();
                this.nullableTimes.TimeWithNegativeUtcOffset.ShouldBeNull();
                this.nullableTimes.TimeWithZSuffix.ShouldBeNull();
                this.nullableTimes.TimeWithZSuffixWithoutSeconds.ShouldBeNull();
            }
        }
    }
}