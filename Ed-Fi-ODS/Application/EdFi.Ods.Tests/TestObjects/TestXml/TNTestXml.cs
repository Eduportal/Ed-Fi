using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EdFi.Ods.Common.Utils.Resources;
using EdFi.Ods.Entities.Common;
using NHibernate.Exceptions;

namespace EdFi.Ods.Tests.TestObjects.TestXml
{
    public static class TNTestXml
    {

        private static IEnumerable<string> GetDescriptorAggregatesFromEntityInterfaces()
        {
            var descriptorInterfaceType = typeof(IDescriptor);
            var aggregateInterfaces =
                Assembly.GetAssembly(descriptorInterfaceType)
                    .GetTypes()
                    .Where(t => t.GetInterfaces().Any(i => i == descriptorInterfaceType) && t.IsInterface).ToList();
            var aggregates = new List<string>();
            aggregateInterfaces.ForEach(i => aggregates.Add(i.Name.Substring(1)));
            return aggregates;
        }

        public static IEnumerable<string> DescriptorsAggregateRoots {get { return GetDescriptorAggregatesFromEntityInterfaces(); }} 

        public static Stream Descriptors
        {
            get { return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("TNDescriptors.xml"); }
        }

        public static IEnumerable<string> StudentEnrollmentAggregateRoots { get
        {
            return new[]
            {
                "SectionReference",
                "StudentSchoolAssociation",
                "StudentSectionAssociation",
                "GraduationPlan",
                "StudentEducationOrganizationAssociation",
                "StudentTransportation"
            };
        } }

        public static Stream StudentEnrollmentWithSectionReference { get
        {
            return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("TopLevelSectionReference.xml");
        } }
    }
}