using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EdFi.Ods.Entities.NHibernate.Architecture
{
    public static class TypeExtensions
    {
        public static IEnumerable<PropertyInfo> GetSignatureProperties(this Type type)
        {
            return type.GetProperties().Where(
                    p => Attribute.IsDefined(p, typeof(DomainSignatureAttribute), true));
        }

        public static bool IsScalar(this Type type)
        {
            return type.IsValueType || type == typeof(string);
        }
    }
}
