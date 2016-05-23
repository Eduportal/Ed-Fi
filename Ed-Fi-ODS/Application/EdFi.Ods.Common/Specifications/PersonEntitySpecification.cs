using System;
using System.Linq;
using EdFi.Common.Extensions;

namespace EdFi.Ods.Common.Specifications
{
    public class PersonEntitySpecification
    {
        private static readonly string[] _validPersonTypes = { "Staff", "Student", "Parent" };

        public static string[] ValidPersonTypes
        {
            get { return _validPersonTypes; }
        }

        /// <summary>
        /// Indicates whether the specified <see cref="Type"/> represents a type of person.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to be evaluated.</param>
        /// <returns><b>true</b> if the entity type represents a type of person; otherwise <b>false</b>.</returns>
        public static bool IsPersonEntity(Type type)
        {
            return IsPersonEntity(type.Name);
        }

        /// <summary>
        /// Indicates whether the specified type name represents a type of person.
        /// </summary>
        /// <param name="typeName">The <see cref="Type.Name"/> value to be evaluated.</param>
        /// <returns><b>true</b> if the entity represents a type of person; otherwise <b>false</b>.</returns>
        public static bool IsPersonEntity(string typeName)
        {
            return _validPersonTypes.Contains(typeName, StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Indicates whether the specified property name is an identifier for a person.
        /// </summary>
        /// <param name="propertyName">The name of the property to be evaluated.</param>
        /// <returns><b>true</b> if the property is an identifier for a type of person; otherwise <b>false</b>.</returns>
        public static bool IsPersonIdentifier(string propertyName)
        {
            return IsPersonIdentifier(propertyName, null);
        }

        /// <summary>
        /// Indicates whether the specified property name is an identifier for the specified person type.
        /// </summary>
        /// <param name="propertyName">The name of the property to be evaluated.</param>
        /// <param name="personType">A specific type of person.</param>
        /// <returns><b>true</b> if the property is an identifier for the specified type of person; otherwise <b>false</b>.</returns>
        public static bool IsPersonIdentifier(string propertyName, string personType)
        {
            if (personType != null && !_validPersonTypes.Any(pt => pt.EqualsIgnoreCase(personType)))
                throw new ArgumentException("'{0}' is not a supported person type.");

            string entityName;

            // TODO: Embedded convention (Person identifiers can end with "USI" or "UniqueId")
            if (propertyName.TryTrimSuffix("UniqueId", out entityName)
                || propertyName.TryTrimSuffix("USI", out entityName))
            {
                return IsPersonEntity(entityName)
                    && (personType == null || entityName.EqualsIgnoreCase(personType));
            }

            return false;
        }
    }
}