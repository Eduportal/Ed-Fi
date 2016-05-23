using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    public class InferredCalendarDateReference : ProcessChainOfResponsibilityBase
    {
        private const string _expectedElementName = "EducationOrganizationReference";
        private const string _referenceTypeName = "CalendarDate";

        public InferredCalendarDateReference(IProcessChainOfResponsibility next)
            : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.ParentElement != null &&
                   (schemaObject.ParentElement.XmlSchemaObjectName == "GradingPeriod" || schemaObject.ParentElement.XmlSchemaObjectName == "AcademicWeek") &&
                   schemaObject.XmlSchemaObjectName == _expectedElementName;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var expectedRestType = new ExpectedRestType
                                        {
                                            ClassName = _referenceTypeName + "Reference",
                                            Namespace = BuildNamespace(_referenceTypeName)
                                        };
            var propertyName = _referenceTypeName + "Reference";


            var expectedRestProperty = new ExpectedMultipleRestProperties
                                            {
                                                ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                PropertyExpectedRestType = expectedRestType,
                                                PropertyName = "Begin" + propertyName,
                                                IsPropertyCollection = false,
                                                AdditionalProperty = new ExpectedRestProperty
                                                                        {
                                                                            ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                                            PropertyExpectedRestType = expectedRestType,
                                                                            PropertyName = "End" + propertyName
                                                                        }
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
