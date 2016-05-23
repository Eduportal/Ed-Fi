using System.Linq;
using System.Reflection;

namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using System;
    using System.Xml.Linq;

    using global::EdFi.Ods.XmlShredding;

    using NHibernate.Linq;

    public class TestMapFromXmlResourceFactory<T> : IResourceFactory<T> where T : new()
    {
        public T Build(XElement xml, INodeSearch nodeSearch)
        {
            var interfaceProperties = typeof(T).GetInterfaces().SelectMany(i => i.GetProperties());
            var resource = new T();
            xml.Elements().ForEach(x =>
            {
                var prop = interfaceProperties.FirstOrDefault(p => p.Name == x.Name.ToString());
                if (prop != null)
                    prop.SetValue(resource, Convert.ChangeType(x.Value, prop.PropertyType));
            });
            return resource;
        }
    }
}