namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.CodeGen.XmlShredding.CodeDeclarations;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Process;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    [TestFixture]
    [Ignore("These are brittle.")]
    public class CodeGenEngineTests : SchemaMetadataTestBase
    {
        private NamespaceDeclaration generatedCode;
        private ClassDeclaration factoryClass;
        private const string AggregateRootNamespace = "AggregateRootNamespace";
        private const string SubClassNamespace = "EdFi.Ods.SubClassNamespace";
        private const string InterchangeName = "TestInterchange";
        private const string AggregateRootClass = "AggregateRootClass";
        private const string AggregateRootInterface = "EdFi.Ods.IAggregateRootClass";
        private ParsedSchemaObject simpleXsd;

        public class EntityAggregatePropertyStub : IEntityTypeProperty
        {
            public string PropertyName { get; set; }
            public string ElementName { get; set; }
            public IManageEntityMetadata MetaDataMgr { get; set; }

            IManageEntityMetadata IEntityTypeProperty.GetMetaDataMgr()
            {
                return this.MetaDataMgr;
            }
        }

        [SetUp]
        public void SetUp()
        {
            var xsd = this.GetParsed(x => x.XmlSchemaObjectName == "Student");
            this.simpleXsd = xsd.ChildElements.First(c => c.XmlSchemaObjectName == "Sex");
            var t4 = new FakeT4();
            var codeGenerator = new CodeGeneratorStub(t4, new FileManagerStub(), new CodeGenerationConfig());

            var singleSimpleTypedProperties = new List<ISimpleTypeProperty>
                {
                    new SimpleTypeProperty(this.simpleXsd)
                };

            var singleEntityTypedProperties = new List<IEntityTypeProperty>
                {
                    new EntityAggregatePropertyStub
                        {
                            ElementName = "ComplexElement",
                            PropertyName = "ClassTypeProperty",

                            MetaDataMgr =
                                BuildManageAggregateMetadata("SubClass", SubClassNamespace, "ISubClass",
                                                             new List<ISimpleTypeProperty>(),
                                                             new List<IEntityTypeProperty>(),
                                                             new List<IEntityTypeProperty>(),
                                                             new List<IInlineEntityProperty>(),
                                                             new List<IForeignKeyProperty>()
                                    )
                        }
                };

            var entityTypedCollectionProperties = new List<IEntityTypeProperty>
                {
                    new EntityAggregatePropertyStub
                        {
                            ElementName = "AnotherComplexElement",
                            PropertyName = "AnotherClassTypeProperty",
                            
                            MetaDataMgr =
                                BuildManageAggregateMetadata("AnotherSubClass", SubClassNamespace, "IAnotherSubClass",
                                                             new List<ISimpleTypeProperty>(),
                                                             new List<IEntityTypeProperty>(),
                                                             new List<IEntityTypeProperty>(),
                                                             new List<IInlineEntityProperty>(),
                                                             new List<IForeignKeyProperty>()
                                    ) 
                        }
                };

            var inlineEntityCollectionProperties = new List<IInlineEntityProperty>
                {
                    new InlineEntityPropertyStub
                        {
                            ClassName = "InlineTypeA",
                            InterfaceName = "IInlineTypeA",
                            ElementName = "InlineElementA",
                            PropertyName = "InlinePropertyAList",
                            InlinePropertyName = "InlinePropertyA",
                            InlinePropertyType = typeof(int)
                        },
                    new InlineEntityPropertyStub
                        {
                            ClassName = "InlineTypeB",
                            InterfaceName = "IInlineTypeB",
                            ElementName = "InlineElementB",
                            PropertyName = "InlinePropertyBList",
                            InlinePropertyName = "InlinePropertyB",
                            InlinePropertyType = typeof(string)
                        },

                    new InlineEntityPropertyStub
                    {
                        ClassName = "InlineTypeC",
                        InterfaceName = "IInlineTypeC",
                        ElementName = "InlineElementC",
                        PropertyName = "InlinePropertyCList",
                        InlinePropertyName = "InlinePropertyC",
                        InlinePropertyType = typeof(bool?)
                    }
                };

            var foreignKeyProperties = new List<IForeignKeyProperty>
                {
                    new ForeignKeyPropertyStub
                        {
                            PropertyName = "ForeignKeyProperty",
                            ElementName = "ForeignKeyElement",
                            PropertyType = typeof(short),
                            SerializedReferenceMap = @"a_serialized""/\_map"
                        },
                        new ForeignKeyPropertyStub
                        {
                            PropertyName = "FKPInt32",
                            ElementName = "FKPInt32",
                            PropertyType = typeof(Int32),
                            SerializedReferenceMap = @"map"
                        },
                        new ForeignKeyPropertyStub
                        {
                            PropertyName = "FKPInt32Nullable",
                            ElementName = "FKPInt32Nullable",
                            PropertyType = typeof(Int32?),
                            SerializedReferenceMap = @"map"
                        }
                };

            var manageAggregateMetadata = BuildManageAggregateMetadata(AggregateRootClass, AggregateRootNamespace,
                                                                       AggregateRootInterface,
                                                                       singleSimpleTypedProperties,
                                                                       singleEntityTypedProperties,
                                                                       entityTypedCollectionProperties,
                                                                       inlineEntityCollectionProperties,
                                                                       foreignKeyProperties);

            var interchangeMetadata = MockRepository.GenerateMock<IInterchangeMetadata>();
            interchangeMetadata.Expect(x => x.InterchangeName).Return(InterchangeName);
            interchangeMetadata.Expect(x => x.Aggregates)
                               .Return(new List<IManageEntityMetadata> {manageAggregateMetadata});


            var sut = new CodeGenEngine(codeGenerator, new List<IInterchangeMetadata> {interchangeMetadata});

            sut.Execute();

            this.generatedCode = codeGenerator.generatedNamespaceDeclaration;

            this.factoryClass = this.generatedCode.Classes.Single();
        }
        private static IManageEntityMetadata BuildManageAggregateMetadata(string aggregateName, string resourceNamespace, string entityInterface,
                                                                          IEnumerable<ISimpleTypeProperty> singleSimpleTypedProperties,
                                                                          IEnumerable<IEntityTypeProperty> singleEntityTypedProperties,
                                                                          IEnumerable<IEntityTypeProperty> entityTypedCollectionProperties,
                                                                          IEnumerable<IInlineEntityProperty> inlineEntityCollectionProperties, 
                                                                          IEnumerable<IForeignKeyProperty> foreignKeyProperties)
        {
            var manageAggregateMetadata = MockRepository.GenerateMock<IManageEntityMetadata>();

            manageAggregateMetadata.Expect(x => x.EntityName).Return(aggregateName);
            manageAggregateMetadata.Expect(x => x.EntityInterface).Return(entityInterface);
            manageAggregateMetadata.Expect(x => x.ResourceNamespace).Return(resourceNamespace);

            manageAggregateMetadata
                .Expect(x => x.GetSingleSimpleTypedProperties())
                .Return(singleSimpleTypedProperties);

            manageAggregateMetadata
                .Expect(x => x.GetEntityTypedCollectionProperties())
                .Return(entityTypedCollectionProperties);

            manageAggregateMetadata
                .Expect(x => x.GetSingleEntityTypedProperties())
                .Return(singleEntityTypedProperties);

            manageAggregateMetadata
                .Expect(x => x.GetInlineEntityCollectionProperties())
                .Return(inlineEntityCollectionProperties);

            manageAggregateMetadata
                .Expect(x => x.GetForeignKeyProperties())
                .Return(foreignKeyProperties);

            return manageAggregateMetadata;
        }

        [Test]
        public void Aggregate_namespace_should_be_same_as_root_class()
        {
            this.generatedCode.NamespaceEndingPortion.ShouldEqual(AggregateRootClass);
        }
        
        [Test]
        public void Should_add_using_for_aggeragte_root_namespace()
        {
            this.generatedCode.Usings.ShouldContain(AggregateRootNamespace);
        }
        
         [Test]
        public void Should_add_using_for_sub_class_namespace()
        {
            this.generatedCode.Usings.ShouldContain(SubClassNamespace);
        }
        [Test]
        public void Aggregate_class_name_should_be_same_as_aggregate_root_suffixed_with_factory()
        {
            this.factoryClass.ClassName.ShouldEqual(AggregateRootClass + "Factory");
        }

        [Test]
        public void Should_generate_public_methed_Build()
        {
            var buildMethod = this.factoryClass.Methods.Single(x => x.MethodName == "Build");

            buildMethod.Parameters.ShouldEqual("XElement element, ParsingContext context");

            buildMethod.ReturnType.ShouldEqual(AggregateRootClass);
        }

        [Test]
        public void Should_generate_assignment_for_simple_property()
        {
            var terminal = this.simpleXsd.ProcessResult.Expected as ExpectedTerminalRestProperty;
            var expected = string.Format(@"entity.{0} = element.ValueOf(""{1}"");", terminal.PropertyName,
                this.simpleXsd.XmlSchemaObjectName);
            var foundContent = this.factoryClass
                .Methods.Single(x => x.MethodName == "BuildAggregateRootClass")
                .Lines
                .Select(x => x.Content).ToArray();
            foundContent.ShouldContain(expected);
        }

        [Test]
        public void Should_generate_assignment_for_class_type_property()
        {
            this.factoryClass
                .Methods.Single(x => x.MethodName == "BuildAggregateRootClass")
                .Lines
                .Select(x => x.Content)
                .ShouldContain(
                    @"entity.ClassTypeProperty = BuildSubClass(element.ElementOrEmpty(""ComplexElement""), context);");
        }

        [Test]
        public void Should_generate_a_build_method_for_class_type_property()
        {
            this.factoryClass
                .Methods.Single(x => x.MethodName == "BuildSubClass");

        }

        [Test]
        public void Should_generate_assignment_for_property_of_type_list_of_class()
        {
            this.factoryClass
                .Methods.Single(x => x.MethodName == "BuildAggregateRootClass")
                .Lines
                .Select(x => x.Content)
                .ShouldContain(
                    @"entity.AnotherClassTypeProperty = element.ElementsOrEmpty(""AnotherComplexElement"").Select(x => BuildAnotherSubClass(x, context)).ToList();");
        }

        [Test]
        public void Should_generate_a_build_method_for_property_of_type_list_of_class()
        {
            this.factoryClass
                .Methods.Single(x => x.MethodName == "BuildAnotherSubClass");

        }

        [Test]
        public void Should_generate_assignment_for_inline_type_property_of_value_type()
        {
            var expected = new[]
                {
                    @"entity.InlinePropertyAList = element.ElementsOrEmpty(""InlineElementA"")",
                    ".Select(x => new InlineTypeA",
                    "{",
                    @"InlinePropertyA = !string.IsNullOrEmpty(x.Value) ? (Int32)x : default(Int32)",
                    "})",
                    ".Cast<IInlineTypeA>()",
                    ".ToList();"
                };

            expected = expected.Select(x => x.Trim()).ToArray();

            var actual = this.factoryClass
                .Methods.Single(x => x.MethodName == "BuildAggregateRootClass")
                .Lines
                .SkipWhile(x => x.Content != expected.First())
                .Take(expected.Length)
                .Select(x => x.Content)
                .ToArray();

            actual.ShouldEqual(expected);
        }

        [Test]
        public void Should_generate_assignment_for_inline_type_property_of_type_string()
        {
            var expected = new[]
                {
                    @"entity.InlinePropertyBList = element.ElementsOrEmpty(""InlineElementB"")",
                    ".Select(x => new InlineTypeB",
                    "{",
                    @"InlinePropertyB = (string)x",
                    "})",
                    ".Cast<IInlineTypeB>()",
                    ".ToList();"
                };

            expected = expected.Select(x => x.Trim()).ToArray();

            var actual = this.factoryClass
                .Methods.Single(x => x.MethodName == "BuildAggregateRootClass")
                .Lines
                .SkipWhile(x => x.Content != expected.First())
                .Take(expected.Length)
                .Select(x => x.Content)
                .ToArray();

            actual.ShouldEqual(expected);
        }

        [Test]
        public void Should_generate_assignment_for_inline_type_property_of_nullable_value_type()
        {
            var expected = new[]
                {
                    @"entity.InlinePropertyCList = element.ElementsOrEmpty(""InlineElementC"")",
                    ".Select(x => new InlineTypeC",
                    "{",
                    @"InlinePropertyC = (Boolean?)x",
                    "})",
                    ".Cast<IInlineTypeC>()",
                    ".ToList();"
                };

            expected = expected.Select(x => x.Trim()).ToArray();

            var actual = this.factoryClass
                .Methods.Single(x => x.MethodName == "BuildAggregateRootClass")
                .Lines
                .SkipWhile(x => x.Content != expected.First())
                .Take(expected.Length)
                .Select(x => x.Content)
                .ToArray();

            actual.ShouldEqual(expected);
        }

        [Test]
        public void Should_generate_assignment_for_foreign_key_property()
        {
            var expected = new[]
                {
                    "{",
                    @"var foundElement = context.NodeSearch.FindForeignKeyElement(element, @""a_serialized""""/\_map"");",
                    "entity.ForeignKeyProperty = !string.IsNullOrEmpty(foundElement.Value) ? (Int16)foundElement : default(Int16);",
                    "}"
                };

            expected = expected.Select(x => x.Trim()).ToArray();

            var methodLines = this.factoryClass
                .Methods.Single(x => x.MethodName == "BuildAggregateRootClass")
                .Lines
                .Select(x => x.Content)
                .ToList();

            var index = methodLines.IndexOf(expected[1]);

            var actual =methodLines
                .Skip(index - 1)
                .Take(expected.Length)
                .ToArray();

            actual.ShouldEqual(expected);
        }

        [Test]
        public void Should_Set_Int32_Foreign_Key_Property_to_correct_Type()
        {
            var expected = new[]
            {
                    "{",
                    @"var foundElement = context.NodeSearch.FindForeignKeyElement(element, @""map"");",
                    "entity.FKPInt32 = !string.IsNullOrEmpty(foundElement.Value) ? (Int32)foundElement : default(Int32);",
                    "}"
            };

            expected = expected.Select(x => x.Trim()).ToArray();

            var methodLines =
                this.factoryClass.Methods.Single(x => x.MethodName == "BuildAggregateRootClass")
                    .Lines.Select(x => x.Content)
                    .ToList();
            var index = methodLines.IndexOf(expected[1]);

            var actual = methodLines.Skip(index - 1).Take(expected.Length).ToArray();

            actual.ShouldEqual(expected);
        }
    }

    public class FileManagerStub
    {
        public void StartNewFile(string fileName)
        {
        }

        public void Process()
        {
        }
    }

    public class SimpleAggregatePropertyStub:ISimpleTypeProperty
    {
        public Type PropertyType { get; set; }
        public string ElementName { get; set; }
        public string PropertyName { get; set; }
        public bool IsSchoolYear { get; private set; }
    }

    public class InlineEntityPropertyStub : IInlineEntityProperty
    {
        public string ClassName { get; set; }
        public string InterfaceName { get; set; }
        public string PropertyName { get; set; }
        public string ElementName { get; set; }
        public string InlinePropertyName { get; set; }
        public Type InlinePropertyType { get;  set; }
        public string Namespace { get; set; }
        public bool IsSchoolYear { get; private set; }
        public bool IsNested { get; set; }
        public string[] ElementNames { get; private set; }
    }

    public class ForeignKeyPropertyStub : IForeignKeyProperty
    {
        public Type PropertyType { get; set; }
        public string PropertyName { get; set; }
        public string ElementName { get; set; }
        public string SerializedReferenceMap { get; set; }
        public bool IsSchoolYear { get; private set; }
    }
}
