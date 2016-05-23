namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System;
    using System.Collections.Generic;

    using EdFi.Common.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using EdFi.Ods.CodeGen.XsdToWebApi.Process;

    public class SimpleTypeProperty : ISimpleTypeProperty
    {
        protected ParsedSchemaObject _property;
        protected IExpectedRest _expected;

        public SimpleTypeProperty(ParsedSchemaObject property)
        {
            this._property = property;
            this._expected = property.ProcessResult.Expected;
        }

        public Type PropertyType
        {
            get
            {
                //TODO: Find out how to discover the type for the descriptors enumeration
                return this._property.IsDescriptorsTypeEnumeration() ?  typeof (string) : ((ExpectedTerminalRestProperty)this._expected).PropertyType;
            }
        }

        public string ElementName { get { return this._property.XmlSchemaObjectName; } }
        
        public string PropertyName { get { return ((ExpectedRestProperty) this._expected).PropertyName; } }
        public bool IsSchoolYear { get { return this._property.ProcessResult.ProcessingRuleName == "School Year"; }}

        internal ParsedSchemaObject Parent { get { return this._property.ParentElement; } }
    }

    public static class SimpleTypePropertyExtensions
    {

        private static IDictionary<Type, string> ConvertMethods = 
            new Dictionary<Type, string>
            {
                {typeof (string), "ValueOf"},
                {typeof (DateTime?), "NullableDateValueOf"},
                {typeof (DateTime), "DateValueOf"},
                {typeof (Int16?), "NullableShortValueOf"},
                {typeof (Int16), "ShortValueOf"},
                {typeof (Int32), "IntValueOf"},
                {typeof (Int32?), "NullableIntValueOf"},
                {typeof (bool?), "NullableBoolValueOf"},
                {typeof (bool), "BoolValueOf"},
                {typeof (decimal), "DecimalValueOf"},
                {typeof (decimal?), "NullableDecimalValueOf"},
                {typeof (TimeSpan), "TimeSpanValueOf"},
                {typeof (TimeSpan?), "NullableTimeSpanValueOf"},
            };
        private static IDictionary<Type, string> ConvertNestedMethods = new Dictionary<Type, string>
        {
            {typeof(string), "NestedValueOf"},
            {typeof (DateTime?), "NestedNullableDateTimeValueOf"},
            {typeof(DateTime), "NestedDateTimeValueOf"},
            {typeof (Int16?), "NestedNullableShortValueOf"},
            {typeof (Int16), "NestedShortValueOf"},
            {typeof (Int32), "NestedIntValueOf"},
            {typeof (Int32?), "NestedNullableIntValueOf"},
            {typeof (bool?), "NestedNullableBoolValueOf"},
            {typeof (bool), "NestedBoolValueOf"},
            {typeof (decimal), "NestedDecimalValueOf"},
            {typeof (decimal?), "NestedNullableDecimalValueOf"},
            {typeof (TimeSpan), "NestedTimeSpanValueOf"},
            {typeof (TimeSpan?), "NestedNullableTimeSpanValueOf"},
        }; 

        public static bool IsNestedValue(this ISimpleTypeProperty property)
        {
            return property.HasParent() && property.MyParent().IsCommonExpansion();
        }

        public static bool HasParent(this ISimpleTypeProperty property)
        {
            return (((SimpleTypeProperty) property).Parent != null);
        }

        private static ParsedSchemaObject MyParent(this ISimpleTypeProperty property)
        {
            return ((SimpleTypeProperty) property).Parent;
        }

        public static string GetAssignmentExpressionAsStringFor(this ISimpleTypeProperty property, string entityVariableName)
        {
            if (property.IsSchoolYear)
            {
                return string.Format(property.PropertyType.IsNullable() ? 
                                @"{0}.ElementOrEmpty(""{1}"") == null || string.IsNullOrWhiteSpace({0}.ElementOrEmpty(""{1}"").Value) ? null : {0}.ElementOrEmpty(""{1}"").Last4CharactersAsNullableShort()" : 
                                @"{0}.ElementOrEmpty(""{1}"") == null || string.IsNullOrWhiteSpace({0}.ElementOrEmpty(""{1}"").Value) ? default(Int16) : {0}.ElementOrEmpty(""{1}"").Last4CharactersAsNullableShort() == null ? default(Int16) : (Int16){0}.ElementOrEmpty(""{1}"").Last4CharactersAsNullableShort()", 
                                entityVariableName, property.ElementName);
            }

            if (property.IsNestedValue())
            {
                string nestedExpression;
                if((ConvertNestedMethods.TryGetValue(property.PropertyType, out nestedExpression)))
                    return string.Format(@"{0}.{1}(new []{2}""{3}"",""{4}""{5})", entityVariableName, nestedExpression, "{", property.MyParent().XmlSchemaObjectName, property.ElementName, "}");
            }

            string expression;
            if (ConvertMethods.TryGetValue(property.PropertyType, out expression))
                return string.Format(@"{0}.{1}(""{2}"")", entityVariableName, expression, property.ElementName);
            
            throw new NotImplementedException(string.Format("Cannot find a conversion expression for type '{0}' when parsing SimpleTypeProperty {1}", property.PropertyType, property.PropertyName));
        }
    }
}