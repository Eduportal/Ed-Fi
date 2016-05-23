using System.Text;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Schema;

    using EdFi.Common.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public interface IProcessChainOfResponsibility
    {
        ProcessResult Process(ParsedSchemaObject schemaObject);
    }

    public abstract class ProcessChainOfResponsibilityBase : IProcessChainOfResponsibility
    {
        private IProcessChainOfResponsibility Next { get; set; }
        protected abstract bool CanProcess(ParsedSchemaObject schemaObject);
        protected abstract ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject);

        protected ProcessChainOfResponsibilityBase(IProcessChainOfResponsibility next)
        {
            this.Next = next;
        }

        public ProcessResult Process(ParsedSchemaObject schemaObject)
        {
            if (this.CanProcess(schemaObject))
            {
                return this.DoProcessSchemaObject(schemaObject);
            }

            if (this.Next != null)
                return this.Next.Process(schemaObject);

            throw new ApplicationException("Cannot process schema object " + schemaObject.XmlSchemaObjectName);
        }

        protected ExpectedRestType GetContainingType(ParsedSchemaObject schemaObject)
        {
            var parentRestType = schemaObject.ParentElement.ProcessResult.Expected as ExpectedRestType;
            if (parentRestType == null)
            {
                var parentProperty = schemaObject.ParentElement.ProcessResult.Expected as ExpectedRestProperty;
                if (parentProperty != null)
                    parentRestType = parentProperty.PropertyExpectedRestType;
            }
            return parentRestType;
        }

        protected string GetNamespace(ParsedSchemaObject schemaObject)
        {
            var parentTypeName = schemaObject.GetParentTypeName();
            var parentElementTypeName = schemaObject.ParentXmlSchemaType.Name.StripExtensionNameValues();
            if (parentElementTypeName == parentTypeName)
                return this.GetContainingType(schemaObject).GetNamespace();

            return this.BuildNamespace(parentTypeName);
        }

        protected string GetAggregateNamespace(ParsedSchemaObject schemaObject)
        {
            return this.BuildNamespace(schemaObject.XmlSchemaType.Name.StripExtensionNameValues());
        }

        protected string BuildNamespace(string aggregateName)
        {
            return "EdFi.Ods.Api.Models.Resources." + aggregateName;
            
        }

        protected bool GetIsOptional(ParsedSchemaObject schemaObject)
        {
            if (schemaObject.IsOptional)
                return true;

            var parentRestType = this.GetContainingType(schemaObject);
            var parentContext = parentRestType as ExpectedContext;
            if (parentContext != null && parentContext.ContextNullable)
                return true;
            if (parentContext != null && !parentContext.ContextNullable)
                return false;

            if (schemaObject.XmlSchemaObject.Parent is XmlSchemaChoice)
                return true;

            if (schemaObject.XmlSchemaObject.Parent is XmlSchemaSequence && schemaObject.XmlSchemaObject.Parent.Parent is XmlSchemaChoice)
                return true;

            return false;
        }

        protected string GetContext(ParsedSchemaObject schemaObject)
        {
            string context;

            if (this.TryGetPredefinedContext(schemaObject, out context))
                return context;

            var schemaObjectName = schemaObject.XmlSchemaObjectName;
            var modifiedSchemaObjectName = schemaObjectName + "Type";
            var schemaObjectTypeName = schemaObject.XmlSchemaType.Name;
            if (modifiedSchemaObjectName.EndsWith(schemaObjectTypeName))
                context = modifiedSchemaObjectName.Substring(0, modifiedSchemaObjectName.Length - schemaObjectTypeName.Length);
            return context;
        }

        protected bool TryGetPredefinedContext(ParsedSchemaObject schemaObject, out string context)
        {
            context = string.Empty;

            if (schemaObject.ParentElement == null)
                return false;

            var schemaObjectName = schemaObject.XmlSchemaObjectName;
            var parentSchemaObjectName = schemaObject.ParentElement.XmlSchemaObjectName;
            var predefinedContext = ContextMetadata.PredefinedContextMetadata.FirstOrDefault(x => x.ElementName == schemaObjectName && x.ParentElementName == parentSchemaObjectName);
            if (predefinedContext != null)
            {
                context = predefinedContext.Context;
                return true;
            }

            return false;
        }

        protected bool IsUnifiedElementOfPredefinedContext(ParsedSchemaObject schemaObject)
        {
            var schemaObjectName = schemaObject.XmlSchemaObjectName;

            var ancestry = new List<Tuple<String, String>>();
            var parent = schemaObject.ParentElement;
            while (parent != null && parent.ParentElement != null)
            {
                ancestry.Add(new Tuple<string, string>(parent.XmlSchemaObjectName, parent.ParentElement.XmlSchemaObjectName));
                parent = parent.ParentElement;
            }

            var result = ContextMetadata.PredefinedContextMetadata
                .Any(x => x.UnifiedElements.Contains(schemaObjectName) 
                    && ancestry.Any(a=> a.Item1 == x.ElementName && a.Item2 == x.ParentElementName));

            return result;
        }

        protected string AddPropertyContext(ParsedSchemaObject schemaObject, string basePropertyName)
        {
            var parentRestType = this.GetContainingType(schemaObject);
            var context = string.Empty;
            var parentContext = parentRestType as ExpectedContext;
            if (parentContext != null && !IsUnifiedElementOfPredefinedContext(schemaObject.ParentElement))
                context = parentContext.Context;

            var contextWords = context.SplitCamelCase();
            for (var i = 0; i < contextWords.Length; i++)
            {
                var subContext = String.Join(string.Empty, contextWords.Skip(i));
                if (basePropertyName.StartsWith(subContext))
                {
                    context = context.Substring(0, context.Length - subContext.Length);
                    break;
                }
            }

            return context + basePropertyName;
        }

        protected string GetElementAnnotation(XmlSchemaElement schemaElement, string annotationName)
        {
            if (schemaElement == null || schemaElement.Name == null)
                return "Missing Annotation";

            return this.GetAnnotation(schemaElement, annotationName);
        }

        protected string GetTypeAnnotation(XmlSchemaType schemaType, string annotationName)
        {
            if (schemaType == null || schemaType.Name == null)
                return "Missing Annotation";

            return this.GetAnnotation(schemaType, annotationName);
        }

        private string GetAnnotation(XmlSchemaAnnotated annotatedSchema, string annotationName)
        {
            if (annotatedSchema.Annotation == null)
                return "Missing Annotation";

            foreach (var item in annotatedSchema.Annotation.Items)
            {
                var annotationElement = item as XmlSchemaAppInfo;
                if (annotationElement != null)
                {
                    foreach (var xmlNode in annotationElement.Markup)
                    {
                        if (xmlNode.NamespaceURI == "http://ed-fi.org/annotation" && xmlNode.LocalName == annotationName)
                            return xmlNode.InnerText;
                    }
                }
            }
            return "Missing Annotation";
        }

        protected string CombineTypeNames(string parentTypeName, string propertyTypeName)
        {
            propertyTypeName = propertyTypeName.StripExtensionNameValues();
            parentTypeName = parentTypeName.StripExtensionNameValues();
            var parentContainingWords = parentTypeName.SplitCamelCase().ToList();
            for (var i = 0; i < parentContainingWords.Count; i++)
            {
                var sb = new StringBuilder();
                parentContainingWords.GetRange(i, parentContainingWords.Count - i).ForEach(x => sb.Append(x));
                var subParentTypeName = sb.ToString();
                if (propertyTypeName.StartsWith(subParentTypeName))
                {
                    sb = new StringBuilder();
                    parentContainingWords.GetRange(0, i).ForEach(x => sb.Append(x));
                    parentTypeName = sb.ToString();
                    break;
                }
            }
            return parentTypeName + propertyTypeName;
        }
    }
}
