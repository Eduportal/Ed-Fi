using log4net;

namespace EdFi.Ods.Common
{
    public class ValidationState
    {
        public bool? IsValid { get; set; }

        public static ValidationState Current
        {
            get { return ThreadContext.Properties["ValidationState"] as ValidationState; }
            set { ThreadContext.Properties["ValidationState"] = value; }
        }
    }
}