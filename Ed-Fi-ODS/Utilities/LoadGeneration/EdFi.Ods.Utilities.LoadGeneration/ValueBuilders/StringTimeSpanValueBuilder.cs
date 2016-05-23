// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using EdFi.TestObjects;
using EdFi.TestObjects.Builders;

namespace EdFi.Ods.Utilities.LoadGeneration.ValueBuilders
{
    public class StringTimeSpanValueBuilder : IValueBuilder
    {
        private readonly TimeSpanBuilder _timeSpanBuilder = new TimeSpanBuilder();

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (!buildContext.LogicalPropertyPath.EndsWith("Time", StringComparison.InvariantCultureIgnoreCase)
                || buildContext.TargetType != typeof(string))
                return ValueBuildResult.NotHandled;

            var tsBuildContext = new BuildContext(string.Empty, typeof(TimeSpan), null, null, null, BuildMode.Create);
            
            var tsBuildResult = _timeSpanBuilder.TryBuild(tsBuildContext);

            if (!tsBuildResult.ShouldSetValue)
                return ValueBuildResult.NotHandled;

            var newValue = (TimeSpan) tsBuildResult.Value;

            return ValueBuildResult.WithValue(newValue.ToString(@"hh\:mm"), buildContext.LogicalPropertyPath);
        }

        public void Reset() { _timeSpanBuilder.Reset(); }
        public ITestObjectFactory Factory { get; set; }
    }
}