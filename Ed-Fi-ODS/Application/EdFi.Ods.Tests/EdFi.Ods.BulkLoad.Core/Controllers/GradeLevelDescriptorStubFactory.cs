namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using System.Xml.Linq;

    using global::EdFi.Ods.Api.Models.Resources.GradeLevelDescriptor;
    using global::EdFi.Ods.XmlShredding;

    public class GradeLevelDescriptorStubFactory : IResourceFactory<GradeLevelDescriptor>
    {
        public GradeLevelDescriptor Build(XElement xml, INodeSearch nodeSearch)
        {
            return new GradeLevelDescriptor{ CodeValue = "I am a stub"};
        }
    }
}