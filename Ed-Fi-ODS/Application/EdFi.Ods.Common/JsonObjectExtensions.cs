using System.Collections.Generic;
using System.Reflection;
using EdFi.Common;
using EdFi.Common.Extensions;
using Newtonsoft.Json.Linq;

namespace EdFi.Ods.Common
{
    public static class JsonObjectExtensions
    {
        public static bool EqualsEntity(this JArray jsonObject, object entity,
                                        IEnumerable<PropertyInfo> scalarSignatureProperties)
        {
            foreach (PropertyInfo scalarSignatureProperty in scalarSignatureProperties)
            {
                // TODO: May need to do special handling of dates?
                string entityValue = scalarSignatureProperty.GetValue(entity).ToString();

                if (jsonObject[scalarSignatureProperty.Name.ToCamelCase()].Value<string>() != entityValue)
                    return false;
            }

            return true;
        }

        public static bool ContainsKey(this JObject jsonObject, string key)
        {
            return jsonObject.Property(key) != null;
        }

        public static IEnumerable<JArray> ArrayObjects(this JObject jsonObject)
        {
            foreach (var obj in jsonObject)
            {
                var val = obj.Value;
                if (val.Type == JTokenType.Array)
                    yield return val.Value<JArray>();
            }
        } 
    }
}
