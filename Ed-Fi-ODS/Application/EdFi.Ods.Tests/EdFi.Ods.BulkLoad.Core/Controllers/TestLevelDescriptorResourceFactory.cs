namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using System;
    using System.Xml.Linq;

    using global::EdFi.Ods.Api.Models.Resources.LevelDescriptor;
    using global::EdFi.Ods.XmlShredding;

    public class TestLevelDescriptorResourceFactory : IResourceFactory<LevelDescriptor>
    {
        private readonly Exception _exceptionToThrow;

        public TestLevelDescriptorResourceFactory() {}
        public TestLevelDescriptorResourceFactory(Exception exceptionToThrow)
        {
            this._exceptionToThrow = exceptionToThrow;
        }

        public LevelDescriptor Build(XElement xml, INodeSearch nodeSearch)
        {
            if (this._exceptionToThrow != null) throw this._exceptionToThrow;

            return new LevelDescriptor {CodeValue = xml.Value};
        }
    }
}