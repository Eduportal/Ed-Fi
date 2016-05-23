using System.Xml.Linq;

namespace EdFi.Ods.XmlShredding
{
    public interface IResourceFactory<out TResource>
    {
        TResource Build(XElement xml, INodeSearch nodeSearch);
    }
}