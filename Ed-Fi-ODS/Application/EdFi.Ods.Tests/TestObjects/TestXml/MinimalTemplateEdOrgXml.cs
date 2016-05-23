using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Utils.Resources;

namespace EdFi.Ods.Tests.TestObjects.TestXml
{
    public class MinimalTemplateEdOrgXml
    {
        private static Stream EdOrgXml
        {
            get
            {
                return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("Interchange-EducationOrganization.xml");
            }
        }

        public static KeyValuePair<string, Stream> EdOrgXmlTypePair = new KeyValuePair<string, Stream>(InterchangeType.EducationOrganization.Name, EdOrgXml);


    }
}
