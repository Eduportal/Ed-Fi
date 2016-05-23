using System;
using System.Collections.Generic;
using System.Linq;

namespace EdFi.Common.Extensions
{
    public static class TypeExtensions
    {
        private static Dictionary<Type, object> defaultValuesByType = new Dictionary<Type, object>();

        public static T GetDefaultValue<T>()
        {
            return (T)GetDefaultValue(typeof(T));
        }

        public static object GetDefaultValue(this Type type)
        {
            object value;

            if (!defaultValuesByType.TryGetValue(type, out value))
            {
                lock (defaultValuesByType)
                {
                    if (type.IsValueType)
                        value = Activator.CreateInstance(type);
                    else
                        value = null;

                    defaultValuesByType[type] = value;
                }
            }

            return value;
        }

        private static Dictionary<Type, Type> itemTypesByGenericListType = new Dictionary<Type, Type>(); 

        public static bool IsGenericList(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (type.IsValueType || type == typeof(string))
                return false;

            foreach (Type @interface in type.GetInterfaces())
            {
                if (@interface.IsGenericType)
                {
                    Type genericTypeDefinition = @interface.GetGenericTypeDefinition();

                    if (genericTypeDefinition == typeof(ICollection<>))
                    {
                        Type itemType;

                        if (!itemTypesByGenericListType.TryGetValue(type, out itemType))
                        {
                            itemType = type.GetGenericArguments().FirstOrDefault();
                            itemTypesByGenericListType[type] = itemType;
                        }

                        // if needed, you can also return the type used as generic argument
                        return itemType != null;
                    }
                }
            }

            return false;
        }

        public static Type GetItemType(this Type type)
        {
            Type itemType;

            if (!itemTypesByGenericListType.TryGetValue(type, out itemType))
            {
                itemType = type.GetGenericArguments().FirstOrDefault();
                itemTypesByGenericListType[type] = itemType;
            }

            return itemTypesByGenericListType[type];
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>);
        }

        public static bool IsAssignableFromGeneric(this Type genericType, Type type)
        {
            return
                type.GetInterfaces()
                    .Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                || (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                || (type.BaseType != null && IsAssignableFromGeneric(genericType, type.BaseType));
        }
    }
}
