using System;

namespace EdFi.Ods.Common.Utils.Extensions
{
    public static class TypeExtensions
    {
        public static bool CanBeCastTo<TOther>(this Type t)
        {
            return typeof (TOther).IsAssignableFrom(t);
        }

        public static bool CannotBeCastTo<TOther>(this Type t)
        {
            return !t.CanBeCastTo<TOther>();
        }

        public static bool IsConcrete(this Type t)
        {
            return !t.IsAbstract;
        }

        public static object DefaultValue(this Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }
    }

    public static class ObjectExtensions
    {
        public static bool IsDefault(this object value, Type type)
        {
            var defaultValue = type.DefaultValue();

            if (value == null)
                return defaultValue == null;

            return value.Equals(defaultValue);
        }
    }
}