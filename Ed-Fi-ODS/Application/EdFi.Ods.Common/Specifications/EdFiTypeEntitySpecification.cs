using System;
using EdFi.Common.Extensions;

namespace EdFi.Ods.Common.Specifications
{
    public class EdFiTypeEntitySpecification
    {
        public static bool IsEdFiTypeEntity(Type type)
        {
            return IsEdFiTypeEntity(type.Name);
        }

        public static bool IsEdFiTypeEntity(string typeName)
        {
            return typeName.EndsWithIgnoreCase("Type") && typeName != "SchoolYearType";
        } 
    }
}