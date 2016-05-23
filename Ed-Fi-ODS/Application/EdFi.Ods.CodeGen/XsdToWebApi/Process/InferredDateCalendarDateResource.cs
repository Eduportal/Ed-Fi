using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    public class InferredDateCalendarDateResource : DateTypeCode
    {
        private const string _expectedTypeSuffix = "Date";
        private const string _referenceTypeName = "CalendarDate";
        private const string _expectedProperty = "Date";

        public InferredDateCalendarDateResource(IProcessChainOfResponsibility next)
            : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.ParentElement != null &&
                   (schemaObject.ParentElement.XmlSchemaObjectName == "GradingPeriod" || schemaObject.ParentElement.XmlSchemaObjectName == "AcademicWeek") &&
                   schemaObject.XmlSchemaObjectName.EndsWith(_expectedTypeSuffix) &&
                   base.CanProcess(schemaObject);
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var objectName = schemaObject.XmlSchemaObjectName;
            var context = objectName.Substring(0, objectName.Length - _expectedTypeSuffix.Length);

            var expectedRestType = new ExpectedRestType
                                        {
                                            ClassName = _referenceTypeName + "Reference",
                                            Namespace = BuildNamespace(_referenceTypeName)
                                        };
            var propertyName = context + _referenceTypeName + "Reference";

            var baseExpectedProperty = base.DoProcessSchemaObject(schemaObject).Expected as ExpectedTerminalRestProperty;
            baseExpectedProperty.PropertyName = _expectedProperty;
            baseExpectedProperty.ContainingExpectedRestType = expectedRestType;

            var expectedRestProperty = new ExpectedMultipleRestProperties
                                            {
                                                ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                PropertyExpectedRestType = expectedRestType,
                                                PropertyName = propertyName,
                                                IsPropertyCollection = false,
                                                AdditionalProperty = baseExpectedProperty
                                            };


            return new ProcessResult
            {
                ProcessChildren = true,
                ProcessingRuleName = "Inferred CalendarDate Resource",
                Expected = expectedRestProperty
            };
        }
    }
}
