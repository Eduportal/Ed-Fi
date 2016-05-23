using EdFi.Ods.CodeGen.XmlShredding.CodeDeclarations;
using System;
using System.Collections.Generic;

namespace EdFi.Ods.CodeGen.XmlShredding
{
    public class CodeGenEngine : ICodeGenEngine
    {
        private readonly ICodeGenerator codeGenerator;
        private readonly IEnumerable<IInterchangeMetadata> _interchangeMetaManagers;

        public CodeGenEngine(ICodeGenerator codeGenerator, IEnumerable<IInterchangeMetadata> interchangeMetaManagers)
        {
            this.codeGenerator = codeGenerator;
            _interchangeMetaManagers = interchangeMetaManagers;
        }

        public void Execute()
        {
            //this.codeGenerator.StartNewFile("ResourceFactories.generated.cs");
            foreach (var interchangeMetadata in _interchangeMetaManagers)
            {
                foreach (var aggregate in interchangeMetadata.Aggregates)
                {
                    GenerateCodeFor(aggregate);
                }
            }
            codeGenerator.WriteToFile();
        }

        private void GenerateCodeFor(IManageEntityMetadata entityMetadataMgr)
        {
            var aggregateRoot = entityMetadataMgr.EntityName;
            // Generate namespace section 
            var namespaceDeclaration = new NamespaceDeclaration(aggregateRoot);

            // Generate class declration
            var factoryClass = new ClassDeclaration
                {
                    ClassName = aggregateRoot + "Factory",
                    Base = string.Format("IResourceFactory<{0}>", aggregateRoot)
                };

            namespaceDeclaration.Classes.Add(factoryClass);

            var context = new CodeGenContext(namespaceDeclaration, factoryClass);

            var buildMethodName = "Build" + aggregateRoot;

            context.ClassDeclaration
                .OpenMethod(new MethodDeclaration("public", aggregateRoot, "Build", "XElement element, INodeSearch nodeSearch"))
                .WriteLine("return ({0})", aggregateRoot)
                .Write("{0}(element, nodeSearch);", buildMethodName)
                .EndMethod();

            GenerateHelperBuildMethodFor(entityMetadataMgr, context);

            // write the generated code to file
            codeGenerator.Generate(namespaceDeclaration);
        }


        private void GenerateHelperBuildMethodFor(IManageEntityMetadata entityMetadataMgr, CodeGenContext context)
        {
            var @class = entityMetadataMgr.EntityName;

            var resourceNamespace = entityMetadataMgr.ResourceNamespace;

            if (!string.IsNullOrEmpty(resourceNamespace) && !context.NamespaceDeclaration.Usings.Contains(resourceNamespace))
                context.NamespaceDeclaration.Usings.Add(resourceNamespace);

            var @interface = entityMetadataMgr.EntityInterface;

            const string methodParameters = "XElement element, INodeSearch nodeSearch";

            var buildMethodName = "Build" + @class;


            context.ClassDeclaration
                   .OpenMethod(new MethodDeclaration("private", @interface, buildMethodName, methodParameters));


            var generateTheseMethodsRightAfterYourFinishThisMethod = new Queue<Action>();

            GenerateMethodBody(entityMetadataMgr, @class, context, @interface, generateTheseMethodsRightAfterYourFinishThisMethod);

            context.ClassDeclaration.EndMethod();

            foreach (var action in generateTheseMethodsRightAfterYourFinishThisMethod)
            {
                action();
            }
        }

        private void GenerateMethodBody(IManageEntityMetadata entityMetadataMgr, string @class, CodeGenContext context, string @interface, Queue<Action> delayedMethodGeneratorActions)
        {
            context.ClassDeclaration
                   .WriteLine("if (element == null) return null;")
                   .WriteLine()
                   .WriteLine("{0} entity = new {1}();", @interface, @class)
                   .WriteLine();

            context.ClassDeclaration.WriteLine("// SingleSimpleTypedProperties");
            foreach (var singleSimpleTypedProperty in entityMetadataMgr.GetSingleSimpleTypedProperties())
            {
                if (entityMetadataMgr.IsDescriptorsExtRef())
                {
                    context.ClassDeclaration.WriteLine("var namespaceValue = element.ValueOf(\"Namespace\") ?? \"\";");
                    context.ClassDeclaration.WriteLine("var codeValue = element.ValueOf(\"CodeValue\") ?? \"\";");
                    context.ClassDeclaration.WriteLine("if( string.IsNullOrEmpty(namespaceValue) || namespaceValue.EndsWith(\"/\"))");
                    context.ClassDeclaration.WriteLine("{").PushIndent();
                    context.ClassDeclaration.WriteLine("entity.{0} = namespaceValue + codeValue;", singleSimpleTypedProperty.PropertyName);
                    context.ClassDeclaration.PopIndent().WriteLine("}");
                    context.ClassDeclaration.WriteLine("else");
                    context.ClassDeclaration.WriteLine("{").PushIndent();
                    context.ClassDeclaration.WriteLine("entity.{0} = namespaceValue + \"/\" + codeValue;", singleSimpleTypedProperty.PropertyName);
                    context.ClassDeclaration.PopIndent().WriteLine("}");
                }
                else
                {
                    context.ClassDeclaration
                    .WriteLine("entity.{0} = {1};", singleSimpleTypedProperty.PropertyName, singleSimpleTypedProperty.GetAssignmentExpressionAsStringFor(@"element"));
                }   
            }

            context.ClassDeclaration.WriteLine("// SingleEntityTypedProperties");
            foreach (var singleEntityTypedProperty in entityMetadataMgr.GetSingleEntityTypedProperties())
            {
                var mgr = singleEntityTypedProperty.GetMetaDataMgr();

                context.ClassDeclaration
                       .WriteLine(
                           @"entity.{0} = Build{1}(element.ElementOrEmpty(""{2}""), nodeSearch);",
                           singleEntityTypedProperty.PropertyName,
                           mgr.EntityName,
                           singleEntityTypedProperty.ElementName);


                delayedMethodGeneratorActions.Enqueue(() => GenerateHelperBuildMethodFor(mgr, context));
            }

            context.ClassDeclaration.WriteLine("// EntityTypedCollectionProperties");
            foreach (var entityTypedCollectionProperty in entityMetadataMgr.GetEntityTypedCollectionProperties())
            {
                var mgr = entityTypedCollectionProperty.GetMetaDataMgr();
                context.ClassDeclaration
                      .WriteLine(
                          @"entity.{0} = element.ElementsOrEmpty(""{1}"").Select(x => Build{2}(x, nodeSearch)).ToList();",
                          entityTypedCollectionProperty.PropertyName, entityTypedCollectionProperty.ElementName, mgr.EntityName);

                delayedMethodGeneratorActions.Enqueue(() => GenerateHelperBuildMethodFor(mgr, context));
            }

            context.ClassDeclaration.WriteLine("// InlineEntityCollectionProperties");
            foreach (var inlineEntityProperty in entityMetadataMgr.GetInlineEntityCollectionProperties())
            {
                var rightSideExpression = inlineEntityProperty.GetConvertExpression("x");

                var assignmentString = inlineEntityProperty.IsNested ? "NestedElementsOrEmpty" : "ElementsOrEmpty";
                var elementVariable = inlineEntityProperty.IsNested
                    ? string.Format(@"new []{0}""{1}"",""{2}""{3}", "{", inlineEntityProperty.ElementNames[0],
                        inlineEntityProperty.ElementNames[1], "}")
                    : string.Format(@"""{0}""", inlineEntityProperty.ElementName);

                context.ClassDeclaration
                           .WriteLine(@"entity.{0} = element.{1}({2})", inlineEntityProperty.PropertyName, assignmentString, elementVariable)
                           .PushIndent().WriteLine(".Select(x => new {0}", inlineEntityProperty.ClassName)
                           .PushIndent().WriteLine("{")
                           .PushIndent().WriteLine("{0} = {1}", inlineEntityProperty.InlinePropertyName, rightSideExpression).PopIndent()
                           .WriteLine("})")
                           .PopIndent().WriteLine(".Cast<{0}>()", inlineEntityProperty.InterfaceName)
                           .WriteLine(".ToList();").PopIndent();
            }

            context.ClassDeclaration.WriteLine("// ForeignKeyProperties");
            foreach (var foreignKeyProperty in entityMetadataMgr.GetForeignKeyProperties())
            {
                var foundElementExpression = foreignKeyProperty.GetConvertExpression("foundElement");
                var possibleNamespaceElementExpression = foreignKeyProperty.GetConvertExpression("possibleNamespaceElement");
                var valueReferenceMap = foreignKeyProperty.SerializedReferenceMap.Replace("\"", @"""""");
                string possibleNamespaceReferenceMap = null;
                if (valueReferenceMap.Contains("CodeValue"))
                {
                    possibleNamespaceReferenceMap = valueReferenceMap.Replace("CodeValue", "Namespace");
                    //foundElementExpression = foreignKeyProperty.GetConvertExpression("possibleNamespaceElement") + " + " + foundElementExpression;
                }

                context.ClassDeclaration
                    .WriteLine("{")
                    .PushIndent()
                    .WriteLine(@"var foundElement = nodeSearch.FindForeignKeyElement(element, @""{0}"");", valueReferenceMap);

                if (possibleNamespaceReferenceMap != null)
                    context.ClassDeclaration
                        .WriteLine(
                            @"var possibleNamespaceElement = nodeSearch.FindForeignKeyElement(element, @""{0}"");",
                            possibleNamespaceReferenceMap)
                        .WriteLine(
                            @"entity.{0} = string.IsNullOrEmpty({1}) || {1}.EndsWith(""/"")?{1}+{2}:{1}+""/""+{2};", 
                            foreignKeyProperty.PropertyName, possibleNamespaceElementExpression, foundElementExpression);
                else
                    context.ClassDeclaration
                        .WriteLine("entity.{0} = {1};", foreignKeyProperty.PropertyName, foundElementExpression);

                context.ClassDeclaration
                    .PopIndent()
                    .WriteLine("}");
            }

            context.ClassDeclaration.WriteLine("// MultiElementEntityCollectionProperties");
            foreach (var property in entityMetadataMgr.GetMultiElementEntityCollectionProperties())
            {
                context.ClassDeclaration
                    .WriteLine(
                        @"entity.{0} = element.AllElementsOrEmpty(new []{1}{2}{3}).Select(x => Build{4}(x, nodeSearch)).ToList();",
                        property.PropertyName, "{", property.ParticipatingElementsAsCommaDelimentedStringOfStrings, "}", property.EntityName);
                var closureSafeProperty = property;
                delayedMethodGeneratorActions.Enqueue(() => GenerateHelperBuildMethodFor(closureSafeProperty.GetEntityMetadataManager(), context));
            }

            context.ClassDeclaration
                   .WriteLine()
                   .WriteLine("return entity;");
        }
    }
}
