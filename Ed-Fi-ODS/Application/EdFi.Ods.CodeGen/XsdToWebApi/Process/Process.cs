using System.Diagnostics;
using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    public class Process
    {
        private static IProcessChainOfResponsibility _chain;

        public static void ConfigureForInterfaces()
        {
            var noMatch = new NoMatch(null);
            var interchange = new Interchange(noMatch);
            var shortTypeCode = new ShortTypeCode(interchange);
            var doubleTypeCode = new DoubleTypeCode(shortTypeCode);
            var durationTypeCode = new DurationTypeCode(doubleTypeCode);
            var textTypeCode = new TextTypeCode(durationTypeCode);
            var timeTypeCode = new TimeTypeCode(textTypeCode);
            var gYearTypeCode = new GYearTypeCode(timeTypeCode);
            var boolTypeCode = new BooleanTypeCode(gYearTypeCode);
            var dateTypeCode = new DateTypeCode(boolTypeCode);
            var decimalTypeCode = new DecimalTypeCode(dateTypeCode);
            var integerTypeCode = new IntegerTypeCode(decimalTypeCode);
            var positiveIntegerTypeCode = new PositiveIntegerTypeCode(integerTypeCode);
            var intTypeCode = new IntTypeCode(positiveIntegerTypeCode);
            var stringTypeCodeCollection = new StringTypeCodeCollection(intTypeCode);
            var stringTypeCode = new StringTypeCode(stringTypeCodeCollection);
            var uniqueStateId = new UniqueId(stringTypeCode);
            var association = new Association(uniqueStateId);
            var descriptor = new Descriptor(association);
            var enumeration = new Enumeration(descriptor);
            var descriptorEnumeration = new DescriptorEnumeration(enumeration);
            var enumerationCollection = new EnumerationCollection(descriptorEnumeration);
            var extendedDescriptorReferenceCollectionCodeValue = new ExtendedDescriptorReferenceCollectionCodeValue(enumerationCollection);
            var extendedDescriptorReferenceCollection = new ExtendedDescriptorReferenceCollection(extendedDescriptorReferenceCollectionCodeValue);
            var extendedDescriptorReferenceCodeValue = new ExtendedDescriptorReferenceCodeValue(extendedDescriptorReferenceCollection);
            var extendedDescriptorReference = new ExtendedDescriptorReference(extendedDescriptorReferenceCodeValue);
            var priorDescriptor = new PriorDescriptorReference(extendedDescriptorReference);
            var schoolYear = new SchoolYear(priorDescriptor);
            var identity = new Identity(schoolYear);
            var extendedReferenceRefAttribute = new ExtendedReferenceRefAttribute(identity);
            var extendedReferenceCollection = new ExtendedReferenceCollection(extendedReferenceRefAttribute);
            var extendedReference = new ExtendedReference(extendedReferenceCollection);
            var commonCollection = new CommonCollection(extendedReference);
            var identificationDocumentCommonCollection = new IdentificationDocumentCommonCollection(commonCollection);
            var common = new Common(identificationDocumentCommonCollection);
            var commonExplosion = new CommonExpansion(common);
            var domainEntity = new DomainEntity(commonExplosion);
            var skipTopLevelReference = new SkipTopLevelReference(domainEntity);
            var skipReference = new SkipReference(skipTopLevelReference);
            var skipType = new SkipType(skipReference);
            var skipElement = new SkipElement(skipType);
            var skipDescriptorReferenceNamespace = new SkipDescriptorReferenceNamespace(skipElement);
            var skipId = new SkipIdAttribute(skipDescriptorReferenceNamespace);
            var skipLookup = new SkipLookup(skipId);

            _chain = skipLookup;
        }

        public static void ConfigureForResources()
        {
            var noMatch = new NoMatch(null);
            var interchange = new Interchange(noMatch);
            var shortTypeCode = new ShortTypeCode(interchange);
            var doubleTypeCode = new DoubleTypeCode(shortTypeCode);
            var durationTypeCode = new DurationTypeCode(doubleTypeCode);
            var textTypeCode = new TextTypeCode(durationTypeCode);
            var timeTypeCode = new TimeTypeCode(textTypeCode);
            var gYearTypeCode = new GYearTypeCode(timeTypeCode);
            var boolTypeCode = new BooleanTypeCode(gYearTypeCode);
            var dateTypeCode = new DateTypeCode(boolTypeCode);
            var inferredCalendarDate = new InferredDateCalendarDateResource(dateTypeCode);
            var decimalTypeCode = new DecimalTypeCode(inferredCalendarDate);
            var integerTypeCode = new IntegerTypeCode(decimalTypeCode);
            var positiveIntegerTypeCode = new PositiveIntegerTypeCode(integerTypeCode);
            var intTypeCode = new IntTypeCode(positiveIntegerTypeCode);
            var stringTypeCodeCollection = new StringTypeCodeCollection(intTypeCode);
            var stringTypeCode = new StringTypeCode(stringTypeCodeCollection);
            var uniqueStateId = new UniqueId(stringTypeCode);
            var association = new Association(uniqueStateId);
            var descriptor = new Descriptor(association);
            var enumeration = new Enumeration(descriptor);
            var descriptorEnumeration = new DescriptorEnumeration(enumeration);
            var enumerationCollection = new EnumerationCollection(descriptorEnumeration);
            var extendedDescriptorReferenceCollectionNamespace = new ExtendedDescriptorReferenceCollectionNamespace(enumerationCollection);
            var extendedDescriptorReferenceCollectionCodeValue = new ExtendedDescriptorReferenceCollectionCodeValue(extendedDescriptorReferenceCollectionNamespace);
            var extendedDescriptorReferenceCollection = new ExtendedDescriptorReferenceCollection(extendedDescriptorReferenceCollectionCodeValue);
            var extendedDescriptorReferenceNamespace= new ExtendedDescriptorReferenceNamespace(extendedDescriptorReferenceCollection);
            var extendedDescriptorReferenceCodeValue = new ExtendedDescriptorReferenceCodeValue(extendedDescriptorReferenceNamespace);
            var extendedDescriptorReference = new ExtendedDescriptorReference(extendedDescriptorReferenceCodeValue);
            var priorDescriptor = new PriorDescriptorReference(extendedDescriptorReference);
            var schoolYear = new SchoolYear(priorDescriptor);
            var schoolYearResource = new SchoolYearResource(schoolYear);
            var identityResouce = new IdentityResource(schoolYearResource);
            var extendedReferenceRefAttribute = new ExtendedReferenceRefAttribute(identityResouce);
            var extendedReferenceCollection = new ExtendedReferenceCollection(extendedReferenceRefAttribute);
            var extendedReference = new ExtendedReference(extendedReferenceCollection);
            var extendedReferenceResourceCollection = new ExtendedReferenceResourceCollection(extendedReference);
            var extendedReferenceResource = new ExtendedReferenceResource(extendedReferenceResourceCollection);
            var inferredCalendarDateReference = new InferredCalendarDateReference(extendedReferenceResource);
            var commonCollection = new CommonCollection(inferredCalendarDateReference);
            var identificationDocumentCommonCollection = new IdentificationDocumentCommonCollection(commonCollection);
            var common = new Common(identificationDocumentCommonCollection);
            var commonExplosion = new CommonExpansion(common);
            var domainEntity = new DomainEntity(commonExplosion);
            var skipTopLevelReference = new SkipTopLevelReference(domainEntity);
            //var skipReference = new SkipReference(skipTopLevelReference);
            var skipType = new SkipType(skipTopLevelReference);
            var skipElement = new SkipElement(skipType);
            var skipId = new SkipIdAttribute(skipElement);
            var skipLookup = new SkipLookup(skipId);

            _chain = skipLookup;
        }


        public static void ProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            if (_chain == null)
                ConfigureForInterfaces();

            var result = _chain.Process(schemaObject);
            schemaObject.ProcessResult = result;
            Debug.WriteLine("Process Schema Chain - name:{0,-60} type:{1,-60} rule:{2}",schemaObject.XmlSchemaObjectName, schemaObject.XmlSchemaType.Name, result.ProcessingRuleName);
            if (result.ProcessChildren)
            {
                foreach (var child in schemaObject.ChildElements)
                    ProcessSchemaObject(child);
            }
        }
    }
}
