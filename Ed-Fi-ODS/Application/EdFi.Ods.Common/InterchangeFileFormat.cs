using System;
using System.Linq;
using EdFi.Common;
using EdFi.Common.Extensions;

namespace EdFi.Ods.Common
{
    public class InterchangeFileFormat : Enumeration<InterchangeFileFormat, string>
    {
        public override string Id { get { return Name; } }
        public string Name { get; private set; }

        public static readonly InterchangeFileFormat TextXml = new InterchangeFileFormat("text/xml");
        public static readonly InterchangeFileFormat ApplicationXml = new InterchangeFileFormat("application/xml");

        private InterchangeFileFormat(string name)
        {
            Name = name;
        }

        public static InterchangeFileFormat GetByName(string name)
        {
            return GetValues().SingleOrDefault(x => x.Name.EqualsIgnoreCase(name));
        }
    }
}