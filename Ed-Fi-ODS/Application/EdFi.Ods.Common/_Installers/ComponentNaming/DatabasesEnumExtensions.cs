using System;
using System.Linq;

namespace EdFi.Ods.Common._Installers.ComponentNaming
{
    public static class DatabasesEnumExtensions
    {
        public static string GetConnectionStringName<TEnum>(this TEnum database)
        {
            var fieldInfo = typeof(TEnum)
                .GetField(database.ToString());

            // This should never happen, but if caller doesn't use explicit enum member it could
            if (fieldInfo == null)
                throw new ArgumentException(string.Format("Unable to find enumeration member for value '{0}'.", database));

            var attributes = fieldInfo
                .GetCustomAttributes(typeof(ConnectionStringNameAttribute), false)
                .Cast<ConnectionStringNameAttribute>()
                .ToList();

            if (attributes.Any())
                return attributes.First().ConnectionStringName;

            return null;
        }      
    }
}