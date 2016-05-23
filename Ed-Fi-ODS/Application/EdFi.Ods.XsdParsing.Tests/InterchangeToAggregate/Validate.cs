using EdFi.Ods.Entities.NHibernate;

namespace EdFi.Ods.XsdParsing.Tests.InterchangeToAggregate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using EdFi.Ods.Entities.Common;
    using EdFi.Ods.Api.Models;
    using EdFi.Ods.CodeGen.XsdToWebApi.Process;
    using EdFi.Ods.XsdParsing.Tests;

    public class Validate
    {
        private static readonly List<Type> _modelInterfaces;
        private static readonly List<Type> _resourceClasses;
        private static readonly List<Type> _entityClasses;
        private static readonly Dictionary<Type, PropertyInfo[]> _modelInterfaceProperties = new Dictionary<Type, PropertyInfo[]>();
        private static readonly Type _listType = typeof(IList<>);

        static Validate()
        {
            // TODO: Embedded convention - namespaces in strings
            var modelsAssembly = Assembly.GetAssembly(typeof(Marker_EdFi_Ods_Entities_Common));
            _modelInterfaces = modelsAssembly.GetTypes().Where(x => x.IsInterface && x.Namespace == "EdFi.Ods.Entities.Common").ToList();

            // TODO: Embedded convention - namespaces in strings
            var resourceAssembly = Assembly.GetAssembly(typeof(Marker_EdFi_Ods_Api_Models));
            _resourceClasses = resourceAssembly.GetTypes().Where(x => x.IsClass && x.Namespace.StartsWith("EdFi.Ods.Api.Models.Resources")).ToList();

            // TODO: Embedded convention - namespaces in strings
            var entitiesAssembly = Assembly.GetAssembly(typeof(Marker_EdFi_Ods_Entities_NHibernate));
            _entityClasses = entitiesAssembly
                .GetTypes()
                .Where(x => x.IsClass
                         && x.Namespace != null
                         && x.Namespace.StartsWith("EdFi.Ods.Entities.NHibernate")
                         && x.Namespace.EndsWith("Aggregate")).ToList();
        }

        public static bool IsRestPropertyTypeCorrect(ExpectedRestProperty expectedRestProperty)
        {
            var property = GetInterfaceProperty(expectedRestProperty);
            if (property == null)
                return false;

            var terminalProperty = expectedRestProperty as ExpectedTerminalRestProperty;
            if (terminalProperty != null)
                return property.PropertyType == terminalProperty.PropertyType;

            var propertyType = GetInterfaceType(expectedRestProperty.PropertyExpectedRestType);
            if (propertyType == null)
                return property.PropertyType.Name == expectedRestProperty.PropertyExpectedRestType.GetClassName();

            if (expectedRestProperty.IsPropertyCollection)
                propertyType = _listType.MakeGenericType(propertyType);

            return property.PropertyType == propertyType;
        }

        public static bool IsRestPropertyCorrect(ExpectedRestProperty expectedRestType)
        {
            var property = GetInterfaceProperty(expectedRestType);
            return property != null;
        }

        public static bool IsRestInterfaceCorrect(ExpectedRestType expectedRestType)
        {
            var type = GetInterfaceType(expectedRestType);
            return type != null;
        }

        public static bool IsRestClassCorrect(ExpectedRestType expectedRestType)
        {
            var type = GetResourceClassType(expectedRestType);
            return type != null;
        }

        private static Type GetResourceClassType(ExpectedRestType expectedRestType)
        {
            if (expectedRestType == null)
                return null;

            var interfaceType = GetInterfaceType(expectedRestType);
            if (interfaceType == null)
                return null;

            var type = _resourceClasses.SingleOrDefault(x => x.Name == expectedRestType.GetClassName() && x.Namespace == expectedRestType.GetNamespace());
            if (type == null)
                return null;

            return type.GetInterfaces().Contains(interfaceType) ? type : null;
        }

        private static Type GetEntityClassType(ExpectedRestType expectedRestType)
        {
            if (expectedRestType == null)
                return null;

            var interfaceType = GetInterfaceType(expectedRestType);
            if (interfaceType == null)
                return null;

            var type = _entityClasses.SingleOrDefault(x => x.Name == expectedRestType.GetClassName());

            if (type == null)
                return null;

            return type.GetInterfaces().Contains(interfaceType) ? type : null;
        }

        private static Type GetInterfaceType(ExpectedRestType expectedRestType)
        {
            if (expectedRestType == null)
                return null;

            var interfaceName = "I" + expectedRestType.GetClassName();

            var type = _modelInterfaces.SingleOrDefault(x => x.Name == interfaceName);
            return type;
        }

        private static PropertyInfo GetInterfaceProperty(ExpectedRestProperty expectedRestProperty)
        {
            var type = GetInterfaceType(expectedRestProperty.ContainingExpectedRestType);
            if (type == null)
                return null;
            
            var properties = GetTypeProperties(type);
            var property = properties.SingleOrDefault(x => x.Name == expectedRestProperty.PropertyName);
            return property;
        }

        public static PropertyInfo GetEntityProperty(ExpectedRestProperty expectedRestProperty)
        {
            var interfaceType = GetInterfaceType(expectedRestProperty.ContainingExpectedRestType);
            if (interfaceType == null)
                return null;

            var interfaceProperty = GetInterfaceProperty(expectedRestProperty);
            if (interfaceProperty == null)
                return null;

            var getterProperty = interfaceProperty.GetGetMethod();

            var classType = GetEntityClassType(expectedRestProperty.ContainingExpectedRestType);
            if (classType == null)
                return null;

            var interfaceTypes = new List<Type>(){ interfaceType };
            interfaceTypes.AddRange(interfaceType.GetInterfaces());

            foreach (var type in interfaceTypes)
            {
                var interfaceMapping = classType.GetInterfaceMap(type);
                MethodInfo getterMethod = null;
                for (int i = 0; i < interfaceMapping.InterfaceMethods.Length; i++)
                {
                    if (interfaceMapping.InterfaceMethods[i] == getterProperty)
                    {
                        getterMethod = interfaceMapping.TargetMethods[i];
                        break;
                    }
                }

                foreach (PropertyInfo property in classType.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (getterMethod == property.GetGetMethod(true))
                    {
                        return property;
                    }
                }
            }
            return null;
        }

        public static PropertyInfo[] GetTypeProperties(Type interfaceType)
        {
            if (_modelInterfaceProperties.ContainsKey(interfaceType))
                return _modelInterfaceProperties[interfaceType];

            var properties = interfaceType.GetPublicProperties();
            _modelInterfaceProperties.Add(interfaceType, properties);
            return properties;
        }

        public static Type GetInterfaceType(string interfaceName)
        {
            var type = _modelInterfaces.SingleOrDefault(x => x.Name == interfaceName);
            return type;
        }
    }
}
