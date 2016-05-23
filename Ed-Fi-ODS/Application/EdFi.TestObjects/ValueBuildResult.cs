namespace EdFi.TestObjects
{
    public class ValueBuildResult
    {
        public bool Handled { get; private set; }
        public bool ShouldSetValue { get; private set; }
        public object Value { get; private set; }

        public bool ShouldSkip
        {
            get { return this.Handled && !this.ShouldSetValue && !this.ShouldDefer; }
        }

        public string LogicalPath { get; private set; }
        public bool ShouldDefer { get; private set; }

        public static ValueBuildResult WithValue(object value, string logicalPath)
        {
            return new ValueBuildResult
                       {
                           Handled = true,
                           ShouldDefer = false,
                           ShouldSetValue = true,
                           Value = value,
                           LogicalPath = logicalPath,
                       };
        }

        public static readonly ValueBuildResult NotHandled = new ValueBuildResult {Handled = false};
        
        public static readonly ValueBuildResult Deferred = new ValueBuildResult
        {
            Handled = true, 
            ShouldSetValue = false, 
            ShouldDefer = true
        };

        public static ValueBuildResult Skip(string logicalPath)
        {
            return new ValueBuildResult
            {
                Handled = true, 
                ShouldSetValue = false, 
                ShouldDefer = false, 
                LogicalPath = logicalPath
            };
        }
    }
}