namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Schema;

    using EdFi.Common.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public abstract class SystemTypeCode<T> : ProcessChainOfResponsibilityBase where T : struct 
    {
        private readonly Type _expectedType = typeof (T);
        private readonly Type _nullableExpectedType = typeof (T?);
        private readonly XmlTypeCode _xmlTypeCode;

        protected SystemTypeCode(XmlTypeCode xmlTypeCode, IProcessChainOfResponsibility next) : base(next)
        {
            this._xmlTypeCode = xmlTypeCode;
            this.RuleName = xmlTypeCode.ToString() + " Type Code";
        }

        protected string RuleName { get; set; }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return !schemaObject.IsComplexType &&
                   schemaObject.XmlSchemaType.TypeCode == this._xmlTypeCode;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var parentRestType = this.GetContainingType(schemaObject);
            var isOptional = this.GetIsOptional(schemaObject);

            var expectedRestProperty = new ExpectedTerminalRestProperty
                                            {
                                                PropertyName = this.AddPropertyContext(schemaObject, schemaObject.XmlSchemaObjectName),
                                                PropertyExpectedRestType = new ExpectedRestType
                                                                    {
                                                                        ClassName = isOptional ? this._expectedType.Name + "?" : this._expectedType.Name,
                                                                        Namespace = this._expectedType.Namespace
                                                                    },
                                                PropertyType = isOptional ? this._nullableExpectedType : this._expectedType,
                                                ContainingExpectedRestType = parentRestType,
                                                IsNullable = isOptional
                                            };
            string propertyPrefix;
            if (this.TryFindPropertyNamePrefix(schemaObject, parentRestType, expectedRestProperty, out propertyPrefix))
            {
                expectedRestProperty.PropertyName = string.Concat(propertyPrefix, expectedRestProperty.PropertyName);
            }
            return new ProcessResult
                        {
                            ProcessChildren = false,
                            ProcessingRuleName = this.RuleName,
                            Expected = expectedRestProperty
                        };
        }

        /// <summary>
        /// This will try to find if there are any other properties in the class and try to determine if a prefix is needed for the property name.
        /// </summary>
        /// <param name="schemaObject"></param>
        /// <param name="parentRestType"></param>
        /// <param name="expectedRestProperty"></param>
        /// <param name="propertyPrefix"></param>
        /// <returns></returns>
        private bool TryFindPropertyNamePrefix(ParsedSchemaObject schemaObject, ExpectedRestType parentRestType, ExpectedTerminalRestProperty expectedRestProperty, out string propertyPrefix)
        {
            //initialize
            propertyPrefix = string.Empty;
            var list = new List<ParsedSchemaObject>();

            //Find the forefather element that is a child of the overarching ClassName
            var containingSchemaObject = this.FindContainingSchemaObjectFirstLevelChild(schemaObject, parentRestType);
            //the parent will be the actual progenitor of the current class name
            var parent = containingSchemaObject.ParentElement;

            //flatten the tree to searchable list
            list.AddRange(this.GetChildElements(parent));
            //need to query on the process result, so remove all that are null
            var nonNullProcessResults = list.Where(x => x.ProcessResult != null);
            //narrow list to nodes that have a ProcessResult of type ExpectedRestProperty
            var expectedRestProperties = nonNullProcessResults.Where(x => x.ProcessResult.Expected != null && x.ProcessResult.Expected.GetType() == typeof(ExpectedRestProperty));
            //determine if there are other nodes with the same property name
            var matches = expectedRestProperties.Where(item => ((ExpectedRestProperty)item.ProcessResult.Expected).PropertyName == expectedRestProperty.PropertyName).ToList();
            //if there are no matches, return o more work to do
            if (matches.Count <= 0)
                return false;

            //check that the names are both association, this infers that there may be additional foreign keys
            if (containingSchemaObject.ParentElement.XmlSchemaTypeGroup != "Association" || containingSchemaObject.ParentElement.XmlSchemaTypeGroup != "Association")
                return false;

            //take camel case types and turn into string lists
            var containingWords = containingSchemaObject.XmlSchemaObjectName.SplitCamelCase();
            var parentContatiningWords = containingSchemaObject.ParentElement.XmlSchemaObjectName.SplitCamelCase();
            //see what words are in the containing node name and containing node parent name
            var dif = containingWords.Except(parentContatiningWords).ToList();
            //remove words that are part of the property name already
            var propertyWords = System.Text.RegularExpressions.Regex.Replace(expectedRestProperty.PropertyName, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim().Split(' ');
            //remove words that are already in the property name
            dif = dif.Except(propertyWords).ToList();
            //Remove Common Words like "Reference" (others may need to be added here, but currently Reference was the only one found)
            dif.Remove("Reference");
            //if only one word is left it could mean that the property name may be an actual second instance in the class and need a prefix, so send that back
            if (dif.Count != 1)
                return false;

            //only one dif found so make it the prefix
            propertyPrefix = dif[0];
            return true;
        }

        private ParsedSchemaObject FindContainingSchemaObjectFirstLevelChild(ParsedSchemaObject schemaObject, ExpectedRestType parentRestType)
        {
            var parsedSchemaObj = schemaObject;
            var parent = parsedSchemaObj.ParentElement;
            var parentContainingType = this.GetContainingType(parent);

            while (parentContainingType != null && parentRestType.GetClassName() == parentContainingType.GetClassName())
            {
                parsedSchemaObj = parent;
                parent = parsedSchemaObj.ParentElement;
                parentContainingType = this.GetContainingType(parent);
            }
            return parsedSchemaObj;
        }

        private IEnumerable<ParsedSchemaObject> GetChildElements(ParsedSchemaObject parsedSchemaObject)
        {
            var list = new List<ParsedSchemaObject>();

            foreach (var childElement in parsedSchemaObject.ChildElements)
            {
                list.AddRange(this.GetChildElements(childElement));
                list.Add(childElement);
            }
            return list;
        }
    }


    public class BooleanTypeCode : SystemTypeCode<bool>
    {
        public BooleanTypeCode(IProcessChainOfResponsibility next) : base(XmlTypeCode.Boolean, next) { }
    }

    public class DateTypeCode : SystemTypeCode<DateTime>
    {
        public DateTypeCode(IProcessChainOfResponsibility next) : base(XmlTypeCode.Date, next) { }
    }

    public class DecimalTypeCode : SystemTypeCode<decimal>
    {
        public DecimalTypeCode(IProcessChainOfResponsibility next) : base(XmlTypeCode.Decimal, next) { }
    }

    public class DoubleTypeCode : SystemTypeCode<double>
    {
        public DoubleTypeCode(IProcessChainOfResponsibility next) : base(XmlTypeCode.Double, next) { }
    }
    
    public class GYearTypeCode : SystemTypeCode<short>
    {
        public GYearTypeCode(IProcessChainOfResponsibility next) : base(XmlTypeCode.GYear, next) { }
    }

    public class IntTypeCode : SystemTypeCode<int>
    {
        public IntTypeCode(IProcessChainOfResponsibility next) : base(XmlTypeCode.Int, next) { }
    }

    public class IntegerTypeCode : SystemTypeCode<int>
    {
        public IntegerTypeCode(IProcessChainOfResponsibility next) : base(XmlTypeCode.Integer, next) { }
    }

    public class PositiveIntegerTypeCode : SystemTypeCode<int>
    {
        public PositiveIntegerTypeCode(IProcessChainOfResponsibility next) : base(XmlTypeCode.PositiveInteger, next) { }
    }

    public class TimeTypeCode : SystemTypeCode<TimeSpan>
    {
        public TimeTypeCode(IProcessChainOfResponsibility next) : base(XmlTypeCode.Time, next) { }
    }

    public class ShortTypeCode : SystemTypeCode<short>
    {
        public ShortTypeCode(IProcessChainOfResponsibility next) : base(XmlTypeCode.Short, next) { }
    }
}