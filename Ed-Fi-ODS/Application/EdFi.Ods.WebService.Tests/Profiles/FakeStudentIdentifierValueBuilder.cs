using System.Collections.Concurrent;
using EdFi.TestObjects;

namespace EdFi.Ods.WebService.Tests.Profiles
{
    public class FakeStudentIdentifierValueBuilder : IValueBuilder
    {
        private ConcurrentDictionary<string, int> _numberByPersonType
            = new ConcurrentDictionary<string, int>();

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            string logicalPath = buildContext.LogicalPropertyPath;
            string propertyName = buildContext.GetPropertyName();
            var containingInstance = buildContext.GetContainingInstance();

            int number;
            ValueBuildResult result;

            switch (propertyName)
            {
                case "StudentUSI":
                    var alreadyAssignedUniqueId = GetPropertyValue<string>(containingInstance, "StudentUniqueId");

                    if (alreadyAssignedUniqueId != null)
                        return ValueBuildResult.WithValue(int.Parse(alreadyAssignedUniqueId.Substring(1)), logicalPath);

                    number = _numberByPersonType.GetOrAdd("Student", x => 1);
                    result = ValueBuildResult.WithValue(number, logicalPath);
                    _numberByPersonType["Student"] = ++number;
                    return result;

                case "StudentUniqueId":
                    var alreadyAssignedUSI = GetPropertyValue<int>(containingInstance, "StudentUSI");

                    if (alreadyAssignedUSI != 0)
                        return ValueBuildResult.WithValue("S" + alreadyAssignedUSI, logicalPath);

                    number = _numberByPersonType.GetOrAdd("Student", x => 1);
                    result = ValueBuildResult.WithValue("S" + number, logicalPath);
                    _numberByPersonType["Student"] = ++number;
                    return result;
            }

            return ValueBuildResult.NotHandled;
        }

        public void Reset()
        {
            _numberByPersonType = new ConcurrentDictionary<string, int>();
        }

        public ITestObjectFactory Factory { get; set; }

        private static T GetPropertyValue<T>(object instance, string propertyName)
        {
            try
            {
                T value = (T)instance.GetType().GetProperty(propertyName).GetValue(instance);
                return value;
            }
            catch
            {
                return default(T);
            }
        }
    }
}