using System;
using System.ComponentModel.DataAnnotations;

namespace EdFi.Ods.Swagger.Attributes
{
    public class ApiRequestAttribute : Attribute
    {
        public string Route { get; set; }
        public string Summary { get; set; }
        public string Notes { get; set; }
        public string Verb { get; set; }
        public Type Type { get; set; }
    }
}
