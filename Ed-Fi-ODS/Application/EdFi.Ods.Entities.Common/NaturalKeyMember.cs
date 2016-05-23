using System;

namespace EdFi.Ods.Entities.Common
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class NaturalKeyMemberAttribute : Attribute { }
}