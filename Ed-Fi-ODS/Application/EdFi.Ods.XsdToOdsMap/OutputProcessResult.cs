using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using EdFi.Ods.Api.Models;
using EdFi.Ods.CodeGen.XsdToWebApi.Process;

namespace EdFi.Ods.XsdToOdsMap
{

    public class OutputProcessResult
    {
        private static readonly Assembly _modelsAssembly = Assembly.GetAssembly(typeof(Marker_EdFi_Ods_Api_Models));

        public static List<string> ResultList(ProcessResult processResult)
        {
            return ResultList(processResult, processResult.Expected);
        }

        public static List<string> ResultList(ProcessResult processResult, IExpectedRest expectedRest)
        {
            var expectedRestType = expectedRest as ExpectedRestType;
            if (expectedRestType != null)
                return new List<string> { FormatExpectedRestClass(expectedRestType), string.Empty, string.Empty };

            var expectedRestProperty = expectedRest as ExpectedRestProperty;
            if (expectedRestProperty != null)
                return new List<string>{ FormatExpectedRestClass(expectedRestProperty.ContainingExpectedRestType), FormatExpectedRestPropertyName(expectedRestProperty), FormatExpectedRestPropertyType(expectedRestProperty),  };

            return new List<string> { string.Empty, string.Empty, string.Empty };
        }

        private static string FormatExpectedRestPropertyType(ExpectedRestProperty expected)
        {
            var propertyTypeName = expected.PropertyExpectedRestType.GetClassName();
            propertyTypeName = propertyTypeName.Substring(0, 1).ToLower() + propertyTypeName.Substring(1);
            return expected.IsPropertyCollection ? string.Format("{0}[]", propertyTypeName) : propertyTypeName;  
        }

        private static string FormatExpectedRestPropertyName(ExpectedRestProperty expected)
        {
            var type = _modelsAssembly.GetType(string.Format("{0}.{1}", expected.ContainingExpectedRestType.GetNamespace(), expected.ContainingExpectedRestType.GetClassName()));
            if (type != null)
            {
                PropertyInfo propInfo = type.GetProperty(expected.PropertyName);

                if (propInfo != null)
                {
                    var attr = propInfo.GetCustomAttribute(typeof(DataMemberAttribute)) as DataMemberAttribute;
                    if (attr != null)
                        expected.PropertyName = attr.Name;
                }
                else
                {
                    return "PROP NOT FOUND - " + expected.PropertyName;
                }
            }
            else
            {
                return "TYPE NOT FOUND - " + expected.ContainingExpectedRestType.GetNamespace() + "." + expected.ContainingExpectedRestType.GetClassName();
            }
            return expected.PropertyName;
        }

        private static string FormatExpectedRestClass(ExpectedRestType expected)
        {
            if (expected == null)
                return "";

            var className = expected.GetClassName();

            className = className.Substring(0, 1).ToLower() + className.Substring(1);

            return className;
        }
    }
}
