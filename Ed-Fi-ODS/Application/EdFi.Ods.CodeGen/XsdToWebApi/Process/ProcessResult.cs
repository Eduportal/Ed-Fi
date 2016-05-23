namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;

    using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;

    public class ProcessResult
    {
        public bool ProcessChildren { get; set; }
        public string ProcessingRuleName { get; set; }
        public IExpectedRest Expected { get; set; }
    }

    public interface IExpectedRest {}

    public class Skip : IExpectedRest { }
    public class Continue : IExpectedRest { }

    public class ExpectedReferencedElement : IExpectedRest
    {
        public string ReferencedElementName { get; set; }
    }

    public class ExpectedRestType : IExpectedRest
    {
        public ExpectedRestType(){}
        public ExpectedRestType(ExpectedRestType expectedRestType)
        {
            this.Namespace = expectedRestType.Namespace;
            this.ClassName = expectedRestType.ClassName;
        }

        public string Namespace { private get; set; }
        public string GetNamespace()
        {
            return this.Namespace.StripProjectPrefix();
        }

        public string ClassName { private get; set; }

        public string GetClassName()
        {
            return this.ClassName.StripProjectPrefix();
        }
    }

    public class ExpectedContext : ExpectedRestType
    {
        public ExpectedContext(){}
        public ExpectedContext(ExpectedRestType expectedRestType) : base(expectedRestType) {}

        public string Context { get; set; }
        public bool ContextNullable { get; set; }
    }

    public class ExpectedRestProperty : IExpectedRest
    {
        public ExpectedRestType PropertyExpectedRestType { get; set; }
        public ExpectedRestType ContainingExpectedRestType { get; set; }
        public string PropertyName { get; set; }
        public bool IsPropertyCollection { get; set; }
    }

    public class ExpectedTerminalRestProperty : ExpectedRestProperty
    {
        public Type PropertyType { get; set; }
        public bool IsNullable { get; set; }
    }

    public class ExpectedMultipleRestProperties : ExpectedRestProperty
    {
        public virtual ExpectedRestProperty AdditionalProperty { get; set; }
    }

    public class ExpectedInlineCollection : ExpectedMultipleRestProperties
    {
        public override ExpectedRestProperty AdditionalProperty
        {
            get
            {
                return TerminalRestProperty;
            }
            set
            {
                TerminalRestProperty = value as ExpectedTerminalRestProperty;
            }
        }

        public ExpectedTerminalRestProperty TerminalRestProperty { get; set; }
    }

    public class ReferenceTypeProcessResult : ProcessResult
    {
        public string ReferenceTypeName { get; set; }
    }

    public class ReferenceTypeKey
    {
        public string PropertyNamespace { get; set; }
        public string PropertyClassName { get; set; }
        public string PropertyName { get; set; }
    }
}
