using EdFi.Ods.Entities.Common.Validation;

namespace EdFi.Ods.XsdParsing.Tests.InterchangeToAggregate
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;

    using Common;
    using Entities.Common;
    using CodeGen.XsdToWebApi;
    using CodeGen.XsdToWebApi.Extensions;
    using CodeGen.XsdToWebApi.Parse;
    using CodeGen.XsdToWebApi.Process;
    using NUnit.Framework;

    using RangeAttribute = System.ComponentModel.DataAnnotations.RangeAttribute;

    public abstract class BaseInterchangeToAggregate
    {
        private string _xsdPath;
        private List<ParsedSchemaObject> _parsedCoreSchemaObjects = new List<ParsedSchemaObject>();
        private List<ParsedSchemaObject> _parsedExtensionSchemaObjects = new List<ParsedSchemaObject>();

        protected void TestFixtureSetup(string interchangeName)
        {
            _xsdPath = Path.GetFullPath(@"..\..\..\schema.codegen");

            if (!Directory.Exists(_xsdPath))
                Console.WriteLine("Directory '{0}' does not exist.", _xsdPath);

            Console.WriteLine("Loading interchanges from " + _xsdPath);

            var coreInterchangeFileName = string.Format("Interchange-{0}.xsd", interchangeName);
            var extensionInterchangeFileName = string.Format("{1}Interchange-{0}-Extension.xsd", interchangeName, StringExtensions.ProjectExtension);

            if (File.Exists(Path.Combine(_xsdPath, extensionInterchangeFileName)))
                _parsedExtensionSchemaObjects = InterchangeLoader.Load(_xsdPath, extensionInterchangeFileName);
            else
                Console.WriteLine("{0} not found in folder '{1}'.", extensionInterchangeFileName, _xsdPath);

            if (!_parsedExtensionSchemaObjects.Any() && File.Exists(Path.Combine(_xsdPath, coreInterchangeFileName)))
                _parsedCoreSchemaObjects = InterchangeLoader.Load(_xsdPath, coreInterchangeFileName);
            else
                Console.WriteLine("{0} not found in folder '{1}'.", coreInterchangeFileName, _xsdPath);


            if (_parsedCoreSchemaObjects.Count == 0 && _parsedExtensionSchemaObjects.Count == 0)
                Assert.Fail("Missing interchange file: {0} or {1}", coreInterchangeFileName, extensionInterchangeFileName);
        }

        [Test]
        public void Should_parse_elements_to_existing_resource_class()
        {
            ValidateTypesInInterchange(Validate.IsRestClassCorrect);
        }

        [Test]
        public void Should_parse_types_to_entity_interfaces()
        {
            ValidateTypesInInterchange(Validate.IsRestInterfaceCorrect);
        }

        [Test]
        public void Should_parse_elements_to_entity_properties()
        {
            ValidatePropertiesInInterchange(Validate.IsRestPropertyCorrect);
        }

        [Test]
        public void Should_parse_element_types_to_matching_entity_property_types()
        {
            ValidatePropertiesInInterchange(Validate.IsRestPropertyTypeCorrect);
        }

        [Test]
        public void Should_parse_collections_to_matching_entity_types_and_properties()
        {
            ValidatePropertiesInInterchange(ValidateExpectedInlineCollection);
        }

        private bool ValidateExpectedInlineCollection(ExpectedRestProperty expectedRestProperty)
        {
            var expectedInlineCollection = expectedRestProperty as ExpectedInlineCollection;
            if (expectedInlineCollection != null)
            {
                return Validate.IsRestPropertyCorrect(expectedInlineCollection.TerminalRestProperty) && Validate.IsRestPropertyTypeCorrect(expectedInlineCollection.TerminalRestProperty);
            }

            return true;
        }

        #region Validate XSD and ODS match - not really testing code here, just xsd and db schemas

        [Test]
        public void Should_match_all_elements()
        {
            var sb = new StringBuilder();

            if (_parsedCoreSchemaObjects.Any())
            {
                var results = ValidateAllElement(_parsedCoreSchemaObjects.SelectMany(x => x.AllElements()));

                if (results.Any())
                {
                    sb.AppendLine("------------ Core Interchange ------------");
                    foreach (var result in results)
                        sb.AppendLine(result);
                }
            }

            if (_parsedExtensionSchemaObjects.Any())
            {
                var results = ValidateAllElement(_parsedExtensionSchemaObjects.SelectMany(x => x.AllElements()));

                if (results.Any())
                {
                    sb.AppendLine("----------- Extension Interchange ----------");
                    foreach (var result in results)
                        sb.AppendLine(result);
                }
            }

            if (sb.Length > 0)
                Assert.Fail(sb.ToString());
        }

        private List<ExpectedRestType> FilteredSchemaObjectsHavingRestClassType(IEnumerable<ParsedSchemaObject> schemaObjects)
        {
            return schemaObjects.Select(x => x.ProcessResult.Expected as ExpectedRestType)
                .Where(y => y != null).ToList();
        }

        private List<ExpectedRestProperty> FilteredSchemaObjectsHavingRestProperty(IEnumerable<ParsedSchemaObject> schemaObjects)
        {
            return schemaObjects.Select(x => x.ProcessResult.Expected as ExpectedRestProperty)
                .Where(y => y != null).ToList();
        }

        private List<PropertyInfo> ExtractRelevantPropertiesForClassName(string expectedRestTypeClassName)
        {
            var interfaceType = Validate.GetInterfaceType("I" + expectedRestTypeClassName);
            if (interfaceType != null)
            {
                var hasIdentifierType = typeof(IHasIdentifier);
                var descriptorType = typeof(IDescriptor);
                var edOrgType = typeof(IEducationOrganization);
                var identifiablePersonType = typeof(IIdentifiablePerson);
                var assessmentFamilyType = typeof(IAssessmentFamily);

                IEnumerable<PropertyInfo> propertyInfos = Validate.GetTypeProperties(interfaceType).ToList();
                if (hasIdentifierType.IsAssignableFrom(interfaceType))
                    propertyInfos = propertyInfos.Where(x => x.Name != "Id");
                else
                    propertyInfos =
                        propertyInfos.Where(
                            x => !("I" + x.Name == x.PropertyType.Name && expectedRestTypeClassName.StartsWith(x.Name)));

                if (descriptorType.IsAssignableFrom(interfaceType))
                    propertyInfos = propertyInfos.Where(x => x.Name != "DescriptorId" && x.Name != "PriorDescriptorId");
                if (edOrgType.IsAssignableFrom(interfaceType))
                    propertyInfos = propertyInfos.Where(x => x.Name != "EducationOrganizationId");
                if (identifiablePersonType.IsAssignableFrom(interfaceType))
                    propertyInfos = propertyInfos.Where(x => x.Name != "UniqueId");
                if (assessmentFamilyType.IsAssignableFrom(interfaceType))
                    propertyInfos = propertyInfos.Where(x => x.Name != "Namespace");

                propertyInfos = propertyInfos.Where(x => x.GetCustomAttribute<AutoIncrementAttribute>() == null);
                return propertyInfos.ToList();
            }
            return null;
        }

        //Note: The logic of this method has been reversed to accomodate extensions. Previously it used validation criteria to be that all properties in generated code
        //should match those in schema. But in order to accomodate extensions, which will have additional properties generated in code, I have reversed logic
        //to check that all properties in schema must show up in code, but not necessarily vice versa, as it may have additional properties coming from core and extension
        //while the test method may only be using one schema and not both
        private List<string> ValidateAllElement(IEnumerable<ParsedSchemaObject> schemaObjects)
        {
            var schemaObjectsList = schemaObjects.ToList();

            var schemaObjectsHavingAnExpectedRestType = FilteredSchemaObjectsHavingRestClassType(schemaObjectsList);

            var listOfExpectedClassNames = schemaObjectsHavingAnExpectedRestType.Select(z => z.GetClassName()).ToList();

            var schemaObjectsHavingAnExpectedRestProperty = FilteredSchemaObjectsHavingRestProperty(schemaObjectsList);

            var additionalListofExpectedClassNamesContinued = schemaObjectsHavingAnExpectedRestProperty.Select(z => z.ContainingExpectedRestType.GetClassName());

            listOfExpectedClassNames.AddRange(additionalListofExpectedClassNamesContinued);

            listOfExpectedClassNames = listOfExpectedClassNames.Distinct().ToList();

            var dictionaryOfActualClassNamesAndPropertiesListAsReflectedFromCodeBase = new Dictionary<string, List<PropertyInfo>>();

            listOfExpectedClassNames.ForEach(x =>
            {
                var propertiesForClassName = ExtractRelevantPropertiesForClassName(x);
                if (propertiesForClassName != null) dictionaryOfActualClassNamesAndPropertiesListAsReflectedFromCodeBase.Add(x, propertiesForClassName);
            });

            var flattedListOfActualPropertiesInCodeBase = dictionaryOfActualClassNamesAndPropertiesListAsReflectedFromCodeBase
                .SelectMany(x => x.Value, (classEntry, property) => classEntry.Key + "." + property.Name).Distinct().ToList();

            var listofExpectedClassAndPropertyNames = schemaObjectsHavingAnExpectedRestProperty.Select(x => Tuple.Create(x.ContainingExpectedRestType.GetClassName(), x.PropertyName));

            var listofExpectedClassAndPropertyNamesWhoseClassExistsInCodeBase =
                listofExpectedClassAndPropertyNames.Where(x =>
                    dictionaryOfActualClassNamesAndPropertiesListAsReflectedFromCodeBase.ContainsKey(x.Item1)).ToList();


            var flattenedListOfExpectedProperties = new List<string>();

            listofExpectedClassAndPropertyNamesWhoseClassExistsInCodeBase.ForEach(x =>
            {
                var propertyInfo = dictionaryOfActualClassNamesAndPropertiesListAsReflectedFromCodeBase[x.Item1].FirstOrDefault(y => y.Name == x.Item2);
                if (propertyInfo != null)
                {
                    flattenedListOfExpectedProperties.Add(x.Item1 + "." + propertyInfo.Name);
                }
            });

            var missingProperties = flattenedListOfExpectedProperties.Except(flattedListOfActualPropertiesInCodeBase).ToList();

            return missingProperties;
        }

        [Test]
        public void Should_match_required_elements()
        {
            var sb = new StringBuilder();

            if (_parsedCoreSchemaObjects.Any())
            {
                var results = ValidateRequiredElements(_parsedCoreSchemaObjects.SelectMany(x => x.AllTerminalElements()));

                if (results.Any())
                {
                    sb.AppendLine("------------ Core Interchange ------------");
                    foreach (var result in results)
                        sb.AppendLine(result);
                }
            }

            if (_parsedExtensionSchemaObjects.Any())
            {
                var results = ValidateRequiredElements(_parsedExtensionSchemaObjects.SelectMany(x => x.AllTerminalElements()));

                if (results.Any())
                {
                    sb.AppendLine("----------- Extension Interchange ----------");
                    foreach (var result in results)
                        sb.AppendLine(result);
                }
            }

            if (sb.Length > 0)
                Assert.Fail(sb.ToString());
        }

        private List<string> ValidateRequiredElements(IEnumerable<ParsedSchemaObject> schemaObjects)
        {
            List<string> results = new List<string>();
            foreach (var parsedSchemaObject in schemaObjects)
            {
                var terminalProperty = parsedSchemaObject.ProcessResult.Expected as ExpectedTerminalRestProperty;
                var propertyInfo = Validate.GetEntityProperty(terminalProperty);
                if (propertyInfo == null)
                {
                    results.Add(string.Format("{0}.{1} not found", terminalProperty.ContainingExpectedRestType.GetClassName(), terminalProperty.PropertyName));
                    continue;
                }

                var isMapped = propertyInfo.GetCustomAttribute<DataMemberAttribute>() != null;
                if (isMapped)
                {
                    var isRequiredDbField =
                        propertyInfo.GetCustomAttribute<RequiredWithNonDefaultAttribute>() != null
                        // Non-nullable value types other than strings do not require the attribute to be applied
                        || (propertyInfo.PropertyType != typeof(string) && Nullable.GetUnderlyingType(propertyInfo.PropertyType) == null);

                    var isRequiredXmlElement = !terminalProperty.IsNullable;

                    if (isRequiredDbField != isRequiredXmlElement)
                        results.Add(string.Format("{0}/{1}\t{2}\t{3}.{4}\t{5}", parsedSchemaObject.ParentXmlSchemaType.Name, parsedSchemaObject.XmlSchemaObjectName, isRequiredXmlElement, propertyInfo.DeclaringType.Name, propertyInfo.Name, isRequiredDbField));
                }
            }
            return results;
        }

        [Test]
        public void Should_match_element_string_lengths()
        {
            var sb = new StringBuilder();

            if (_parsedCoreSchemaObjects.Any())
            {
                var results = ValidateStringLength(_parsedCoreSchemaObjects.SelectMany(x => x.AllTerminalElements()));

                if (results.Any())
                {
                    sb.AppendLine("------------ Core Interchange ------------");
                    foreach (var result in results)
                        sb.AppendLine(result);
                }
            }

            if (_parsedExtensionSchemaObjects.Any())
            {
                var results = ValidateStringLength(_parsedExtensionSchemaObjects.SelectMany(x => x.AllTerminalElements()));

                if (results.Any())
                {
                    sb.AppendLine("----------- Extension Interchange ----------");
                    foreach (var result in results)
                        sb.AppendLine(result);
                }
            }

            if (sb.Length > 0)
                Assert.Fail(sb.ToString());
        }

        private List<string> ValidateStringLength(IEnumerable<ParsedSchemaObject> schemaObjects)
        {
            List<string> results = new List<string>();
            foreach (var parsedSchemaObject in schemaObjects)
            {
                if (parsedSchemaObject.XmlSchemaTypeGroup == "Enumeration")
                    continue;

                var terminalProperty = parsedSchemaObject.ProcessResult.Expected as ExpectedTerminalRestProperty;
                if (terminalProperty.PropertyType != typeof(string))
                    continue;

                var propertyInfo = Validate.GetEntityProperty(terminalProperty);
                if (propertyInfo == null)
                {
                    results.Add(string.Format("{0}.{1} not found", terminalProperty.ContainingExpectedRestType.GetClassName(), terminalProperty.PropertyName));
                    continue;
                }

                var isMapped = propertyInfo.GetCustomAttribute<DataMemberAttribute>() != null;
                if (isMapped)
                {
                    var stringLengthAttribute = propertyInfo.GetCustomAttribute<StringLengthAttribute>();
                    int? schemaStringLength = null;
                    foreach (var schemaValidation in parsedSchemaObject.SchemaValidations)
                    {
                        var maxStringLength = schemaValidation as MaxStringLength;
                        if (maxStringLength != null)
                        {
                            schemaStringLength = maxStringLength.MaxLength;
                            break;
                        }
                    }

                    if (stringLengthAttribute == null)
                        results.Add(string.Format("{0}/{1}\t{2}\t{3}.{4}\t", parsedSchemaObject.ParentXmlSchemaType.Name, parsedSchemaObject.XmlSchemaObjectName, schemaStringLength, propertyInfo.DeclaringType.Name, propertyInfo.Name));
                    else if (!schemaStringLength.HasValue)
                        results.Add(string.Format("{0}/{1}\t{2}\t{3}.{4}\t{5}", parsedSchemaObject.ParentXmlSchemaType.Name, parsedSchemaObject.XmlSchemaObjectName, schemaStringLength, propertyInfo.DeclaringType.Name, propertyInfo.Name, stringLengthAttribute.MaximumLength));
                    else if (stringLengthAttribute.MaximumLength != schemaStringLength)
                        results.Add(string.Format("{0}/{1}\t{2}\t{3}.{4}\t{5}", parsedSchemaObject.ParentXmlSchemaType.Name, parsedSchemaObject.XmlSchemaObjectName, schemaStringLength, propertyInfo.DeclaringType.Name, propertyInfo.Name, stringLengthAttribute.MaximumLength));

                }
            }
            return results;
        }

        [Test]
        public void Should_match_element_decimal_format()
        {
            var sb = new StringBuilder();

            if (_parsedCoreSchemaObjects.Any())
            {
                var results = ValidateDecimalPrecision(_parsedCoreSchemaObjects.SelectMany(x => x.AllTerminalElements()));

                if (results.Any())
                {
                    sb.AppendLine("------------ Core Interchange ------------");
                    foreach (var result in results)
                        sb.AppendLine(result);
                }
            }

            if (_parsedExtensionSchemaObjects.Any())
            {
                var results = ValidateDecimalPrecision(_parsedExtensionSchemaObjects.SelectMany(x => x.AllTerminalElements()));

                if (results.Any())
                {
                    sb.AppendLine("----------- Extension Interchange ----------");
                    foreach (var result in results)
                        sb.AppendLine(result);
                }
            }

            if (sb.Length > 0)
                Assert.Fail(sb.ToString());

        }

        private List<string> ValidateDecimalPrecision(IEnumerable<ParsedSchemaObject> schemaObjects)
        {
            List<string> results = new List<string>();
            foreach (var parsedSchemaObject in schemaObjects)
            {
                var terminalProperty = parsedSchemaObject.ProcessResult.Expected as ExpectedTerminalRestProperty;
                if (terminalProperty.PropertyType != typeof(decimal) && terminalProperty.PropertyType != typeof(decimal?))
                    continue;

                var propertyInfo = Validate.GetEntityProperty(terminalProperty);
                if (propertyInfo == null)
                {
                    results.Add(string.Format("{0}.{1} not found", terminalProperty.ContainingExpectedRestType.GetClassName(), terminalProperty.PropertyName));
                    continue;
                }

                var isMapped = propertyInfo.GetCustomAttribute<DataMemberAttribute>() != null;
                if (isMapped)
                {
                    var rangeAttribute = propertyInfo.GetCustomAttribute<RangeAttribute>();
                    DecimalPrecision decimalPrecision = null;
                    foreach (var schemaValidation in parsedSchemaObject.SchemaValidations)
                    {
                        decimalPrecision = schemaValidation as DecimalPrecision;
                        if (decimalPrecision != null)
                            break;
                    }

                    if (rangeAttribute == null)
                        results.Add(string.Format("{0}/{1}\t{2}\t{3}.{4}\t", parsedSchemaObject.ParentXmlSchemaType.Name, parsedSchemaObject.XmlSchemaObjectName, decimalPrecision, propertyInfo.DeclaringType.Name, propertyInfo.Name));
                    else if (decimalPrecision == null)
                        results.Add(string.Format("{0}/{1}\t{2}\t{3}.{4}\t({5}, {6})", parsedSchemaObject.ParentXmlSchemaType.Name, parsedSchemaObject.XmlSchemaObjectName, decimalPrecision, propertyInfo.DeclaringType.Name, propertyInfo.Name, rangeAttribute.Minimum, rangeAttribute.Maximum));
                    else if (decimalPrecision.Maximum != (string)rangeAttribute.Maximum && decimalPrecision.Minimum != (string)rangeAttribute.Minimum)
                        results.Add(string.Format("{0}/{1}\t({2}, {3})\t{4}.{5}\t({6}, {7})", parsedSchemaObject.ParentXmlSchemaType.Name, parsedSchemaObject.XmlSchemaObjectName, decimalPrecision.Minimum, decimalPrecision.Maximum, propertyInfo.DeclaringType.Name, propertyInfo.Name, rangeAttribute.Minimum, rangeAttribute.Maximum));

                }
            }
            return results;
        }

        #endregion

        #region CSV Output - not really testing anything here

        [Test]
        public void Output_parsed_core_interchange()
        {
            if (_parsedCoreSchemaObjects.Any())
                Output(_parsedCoreSchemaObjects);
        }

        [Test]
        public void Output_parsed_extension_interchange()
        {
            if (_parsedExtensionSchemaObjects.Any())
                Output(_parsedExtensionSchemaObjects);
        }

        private static void Output(IEnumerable<ParsedSchemaObject> parsedInterchange)
        {
            Console.WriteLine("XSD Type Name\tXSD Element\tXSD Element Type\tProcess Rule\tResource Class\tContaining Interface\tProperty Name\tProperty Type\tMatched Interface\tMatched Property Name\tMatched Property Type\tMatched Resource Class");
            foreach (var parsedSchemaObject in parsedInterchange)
                PrintResults(parsedSchemaObject);
        }

        private static void PrintResults(ParsedSchemaObject parsedSchemaObject)
        {
            foreach (var childElement in parsedSchemaObject.ChildElements)
                Console.WriteLine(OutputParsedSchemaObject.FormattedResult(childElement));

            foreach (var childElement in parsedSchemaObject.ChildElements)
            {
                if (childElement.ProcessResult.ProcessChildren)
                    PrintResults(childElement);
            }
        }

        #endregion CSV Output - not really testing anything here

        private void ValidateTypesInInterchange(Func<ExpectedRestType, bool> validate)
        {
            var results = new List<string>();
            var sb = new StringBuilder();

            foreach (var parsedSchemaObject in _parsedCoreSchemaObjects)
                results.AddRange(ValidateParsedSchemaObjectExpectedRestType(parsedSchemaObject, validate));

            if (results.Any())
            {
                sb.AppendLine("------------ Core Interchange ------------");
                foreach (var result in results)
                    sb.AppendLine(result);
                results.Clear();
            }

            foreach (var parsedSchemaObject in _parsedExtensionSchemaObjects)
                results.AddRange(ValidateParsedSchemaObjectExpectedRestType(parsedSchemaObject, validate));

            if (results.Any())
            {
                sb.AppendLine("---------- Extension Interchange ---------");
                foreach (var result in results)
                    sb.AppendLine(result);
            }

            if (sb.Length > 0)
                Assert.Fail(sb.ToString());
        }

        private IEnumerable<string> ValidateParsedSchemaObjectExpectedRestType(ParsedSchemaObject parsedSchemaObject, Func<ExpectedRestType, bool> validate)
        {
            var results = new List<string>();
            foreach (var childElement in parsedSchemaObject.ChildElements)
            {
                var processResult = childElement.ProcessResult;
                var expectedRestType = processResult.Expected as ExpectedRestType;
                if (expectedRestType != null && !validate(expectedRestType))
                {
                    results.Add(OutputParsedSchemaObject.FormattedResult(childElement));
                    return results;
                }
            }

            foreach (var childElement in parsedSchemaObject.ChildElements)
            {
                if (childElement.ProcessResult.ProcessChildren)
                    results.AddRange(ValidateParsedSchemaObjectExpectedRestType(childElement, validate));
            }
            return results;
        }

        private void ValidatePropertiesInInterchange(Func<ExpectedRestProperty, bool> validate)
        {
            var results = new List<string>();
            var sb = new StringBuilder();

            foreach (var parsedSchemaObject in _parsedCoreSchemaObjects)
                results.AddRange(ValidateParsedSchemaObjectExpectedRestProperty(parsedSchemaObject, validate));

            if (results.Any())
            {
                sb.AppendLine("------------ Core Interchange ------------");
                foreach (var result in results)
                    sb.AppendLine(result);
                results.Clear();
            }

            foreach (var parsedSchemaObject in _parsedExtensionSchemaObjects)
                results.AddRange(ValidateParsedSchemaObjectExpectedRestProperty(parsedSchemaObject, validate));

            if (results.Any())
            {
                sb.AppendLine("---------- Extension Interchange ---------");
                foreach (var result in results)
                    sb.AppendLine(result);
            }

            if (sb.Length > 0)
                Assert.Fail(sb.ToString());
        }

        private IEnumerable<string> ValidateParsedSchemaObjectExpectedRestProperty(ParsedSchemaObject parsedSchemaObject, Func<ExpectedRestProperty, bool> validate)
        {
            var results = new List<string>();
            foreach (var childElement in parsedSchemaObject.ChildElements)
            {
                var processResult = childElement.ProcessResult;
                var expectedRestProperty = processResult.Expected as ExpectedRestProperty;
                if (expectedRestProperty != null && Validate.IsRestInterfaceCorrect(expectedRestProperty.ContainingExpectedRestType))
                {
                    var result = validate(expectedRestProperty);
                    if (!result)
                    {
                        results.Add(OutputParsedSchemaObject.FormattedResult(childElement));
                    }
                }
            }

            foreach (var childElement in parsedSchemaObject.ChildElements)
            {
                if (childElement.ProcessResult.ProcessChildren)
                    results.AddRange(ValidateParsedSchemaObjectExpectedRestProperty(childElement, validate));
            }
            return results;
        }
    }

    #region Interchange Tests

    [TestFixture]
    public class When_parsing_Assessment_Metadata_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("AssessmentMetadata");
        }
    }

    [TestFixture]
    public class When_parsing_Descriptors_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("Descriptors");
        }
    }

    [TestFixture]
    public class When_parsing_Education_Organization_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("EducationOrganization");
        }
    }

    [TestFixture]
    public class When_parsing_Education_Org_Calendar_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("EducationOrgCalendar");
        }
    }

    [TestFixture]
    public class When_parsing_Finance_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("Finance");
        }
    }

    [TestFixture]
    public class When_parsing_Master_Schedule_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("MasterSchedule");
        }
    }

    [TestFixture]
    public class When_parsing_Post_Secondary_Event_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("PostSecondaryEvent");
        }
    }

    [TestFixture]
    public class When_parsing_Staff_Association_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("StaffAssociation");
        }
    }

    [TestFixture]
    public class When_parsing_Standards_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("Standards");
        }
    }

    [TestFixture]
    public class When_parsing_Student_Assessment_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("StudentAssessment");
        }
    }

    [TestFixture]
    public class When_parsing_Student_Attendance_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("StudentAttendance");
        }
    }

    [TestFixture]
    public class When_parsing_Student_Cohort_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("StudentCohort");
        }
    }

    [TestFixture]
    public class When_parsing_Student_Discipline_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("StudentDiscipline");
        }
    }

    [TestFixture]
    public class When_parsing_Student_Enrollment_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("StudentEnrollment");
        }
    }

    [TestFixture]
    public class When_parsing_Student_Grade_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("StudentGrade");
        }
    }

    [TestFixture]
    public class When_parsing_Student_Gradebook_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("StudentGradebook");
        }
    }

    [TestFixture]
    public class When_parsing_Student_Intervention_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("StudentIntervention");
        }
    }

    [TestFixture]
    public class When_parsing_Student_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("Student");
        }
    }

    [TestFixture]
    public class When_parsing_Parent_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("Parent");
        }
    }

    [TestFixture]
    public class When_parsing_Student_Program_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("StudentProgram");
        }
    }

    [TestFixture]
    public class When_parsing_Student_Transcript_interchange : BaseInterchangeToAggregate
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            base.TestFixtureSetup("StudentTranscript");
        }
    }

    #endregion Interchange Tests
}
