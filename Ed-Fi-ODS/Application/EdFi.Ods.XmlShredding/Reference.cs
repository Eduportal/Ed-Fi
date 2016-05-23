namespace EdFi.Ods.XmlShredding
{
    public class ParentReference
    {
        public string Id { get; set; }
        public string ElementName { get; set; }
        public string InterchangeId { get; set; }
    }

    public class ChildReference
    {
        public string ParentId { get; set; }
        public string ParentElementName { get; set; }
        public string ChildElementName { get; set; }
        public string ChildInterchangeId { get; set; }
    }
}
