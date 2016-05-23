namespace EdFi.TestObjects.Builders
{
    public class ObjectValueBuilder : IValueBuilder
    {
        private int nextValue = 1;

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType == typeof(object))
            {
                var value = "Object" + this.nextValue++;
                return ValueBuildResult.WithValue(value, buildContext.LogicalPropertyPath);
            }

            return ValueBuildResult.NotHandled;
        }

        public void Reset()
        {
            this.nextValue = 1;
        }

        public ITestObjectFactory Factory { get; set; }
    }
}
