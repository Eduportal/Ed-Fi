using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdFi.Ods.Swagger.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class ApiMemberAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the parameter.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the parameter type of the parameter which identifies how it will be incorporated into the call (e.g. query string, HTTP header, path, etc.).
        /// </summary>
        public string ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the Swagger data type to be collected in the Swagger UI.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the HTTP verb to which the parameter applies.
        /// </summary>
        public string Verb { get; set; }

        /// <summary>
        /// Indicates whether multiple instances of the parameters can be provided.
        /// </summary>
        public bool AllowMultiple { get; set; }

        /// <summary>
        /// Indicates whether the parameter must be provided in the Swagger UI.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Indicates whether to expand the complex object to primitives in the json
        /// </summary>
        public bool Expand { get; set; }
    }
}
