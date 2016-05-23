using System;
using EdFi.Common.Extensions;

namespace EdFi.Ods.Common.Specifications
{
    public class EdFiDescriptorEntitySpecification
    {
        public static bool IsEdFiDescriptorEntity(Type type)
        {
            return IsEdFiDescriptorEntity(type.Name);
        }

        public static bool IsEdFiDescriptorEntity(string typeName)
        {
            return typeName.EndsWithIgnoreCase("Descriptor");
        }
    }
}