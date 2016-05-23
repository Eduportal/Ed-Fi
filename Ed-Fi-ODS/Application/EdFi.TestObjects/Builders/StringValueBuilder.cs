using System;

namespace EdFi.TestObjects.Builders
{
    public class StringValueBuilder : IValueBuilder
    {
        public static bool GenerateNulls = true;
        public static bool GenerateEmptyStrings = true;

        private int counter;
        private int index;

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType != typeof (string))
                return ValueBuildResult.NotHandled;

            if (this.index == 1)
            {
                if (!GenerateEmptyStrings)
                    this.IncrementIndex();
            }

            if (this.index == 2)
            {
                if (!GenerateNulls)
                    this.IncrementIndex();
            }

            ValueBuildResult buildResult;
            switch (this.index)
            {
                case 0:
                    buildResult = ValueBuildResult.WithValue("String" + (++this.counter), buildContext.LogicalPropertyPath);
                    break;
                case 1:
                    buildResult = ValueBuildResult.WithValue(string.Empty, buildContext.LogicalPropertyPath);
                    break;
                case 2:
                    buildResult = ValueBuildResult.WithValue(null, buildContext.LogicalPropertyPath);
                    break;
                default:
                    throw new Exception(string.Format("Unhandled index: {0}", this.index));
            }

            this.IncrementIndex();

            return buildResult;
        }

        public void Reset()
        {
            this.counter = 0;
            this.index = 0;
        }

        private void IncrementIndex()
        {
            this.index = (this.index + 1)%3;
        }

        public ITestObjectFactory Factory { get; set; }
    }
}