namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System.Collections.Generic;

    public class InterchangeMetadata : IInterchangeMetadata
    {
        private readonly string _interchangeName;
        private readonly List<IManageEntityMetadata> _aggregateMetadatas; 

        public InterchangeMetadata(string interchangeName)
        {
            this._interchangeName = interchangeName;
            this._aggregateMetadatas = new List<IManageEntityMetadata>();
        }

        public string InterchangeName { get { return this._interchangeName; } }

        public IEnumerable<IManageEntityMetadata> Aggregates { get { return this._aggregateMetadatas; } }

        public void AddAggregateMetadataMgr(IManageEntityMetadata manager)
        {
            if(!this._aggregateMetadatas.Contains(manager)) this._aggregateMetadatas.Add(manager);
        }
    }
}