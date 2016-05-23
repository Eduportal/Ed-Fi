using System;

namespace EdFi.Ods.Entities.Common
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class AutoIncrementAttribute : Attribute { }
}