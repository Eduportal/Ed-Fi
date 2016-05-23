using EdFi.Ods.CodeGen.XsdToWebApi.Parse;
using EdFi.Ods.CodeGen.XsdToWebApi.Process;
using System.Collections.Generic;
using System.Linq;

namespace EdFi.Ods.CodeGen.XmlShredding
{
    public class EntityMetadataManager : IManageEntityMetadata
    {
        private readonly ParsedSchemaObject _parsedResult;
        private readonly IXPathMapBuilder _mapBuilder;
        private readonly IEnumerable<ParsedSchemaObject> _childElements;
        private readonly IDictionary<string,IEnumerable<ParsedSchemaObject>> _multiElementCollections; 

        public EntityMetadataManager(ParsedSchemaObject aggregateRoot, IXPathMapBuilder mapBuilder)
        {
            _parsedResult = aggregateRoot;
            _mapBuilder = mapBuilder;
            var allChildren = _parsedResult.ChildElements.Where(c => !c.IsCommonExpansion()).ToList();
            allChildren.AddRange(_parsedResult.ChildElements.Where(c => c.IsCommonExpansion() && c.ChildElements.Any())
                    .SelectMany(nested => nested.ChildElements));
            var justEntityCollections = allChildren.Where(c => c.IsEntityCollection());
            _multiElementCollections = justEntityCollections.GroupBy(
                c => ((ExpectedRestProperty) c.ProcessResult.Expected).PropertyExpectedRestType.GetClassName())
                .Where(g => g.Count() > 1)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());
            foreach (var multiElementCollection in _multiElementCollections)
            {
                allChildren.RemoveAll(
                    c =>
                        c.IsEntityCollection() &&
                        ((ExpectedRestProperty) c.ProcessResult.Expected).PropertyExpectedRestType.GetClassName() ==
                        multiElementCollection.Key);
            }
            _childElements = allChildren;
        }

        public string EntityName
        {
            get
            {
                return _parsedResult.IsRestProperty() ? ((ExpectedRestProperty)_parsedResult.ProcessResult.Expected).PropertyExpectedRestType.GetClassName() :
                    ((ExpectedRestType)_parsedResult.ProcessResult.Expected).GetClassName();
            }
        }

        public string ResourceNamespace
        {
            get
            {
                if (_parsedResult.IsRestProperty())
                {
                    var expected = _parsedResult.ProcessResult.Expected as ExpectedRestProperty;
                    return expected != null ? expected.PropertyExpectedRestType.GetNamespace() : null;
                }
                else
                {
                    var expected = _parsedResult.ProcessResult.Expected as ExpectedRestType;
                    return expected == null? string.Empty : expected.GetNamespace();
                }
            }
        }

        public string EntityInterface
        {
            get
            {
                return _parsedResult.IsRestProperty() ? 
                    "I" + ((ExpectedRestProperty) _parsedResult.ProcessResult.Expected).PropertyExpectedRestType.GetClassName() :
                       "I" + ((ExpectedRestType)_parsedResult.ProcessResult.Expected).GetClassName();
            }
        }

        public IEnumerable<ISimpleTypeProperty> GetSingleSimpleTypedProperties()
        {
            return _childElements.Where(p => p.IsTerminalProperty() || p.IsDescriptorsTypeEnumeration()).Select(property => new SimpleTypeProperty(property));
        }

        public IEnumerable<IInlineEntityProperty> GetInlineEntityCollectionProperties()
        {
            return _childElements.Where(p => p.IsInlineProperty())
                    .Select(property => new InlineEntityProperty(property));
        } 

        public IEnumerable<IEntityTypeProperty> GetSingleEntityTypedProperties()
        {
            return _childElements.Where(p => p.IsSingleEntity())
                    .Select(property => new EntityTypeProperty(property));
        }

        public IEnumerable<IEntityTypeProperty> GetEntityTypedCollectionProperties()
        {
            return _childElements.Where(p => p.IsEntityCollection())
                    .Select(property => new EntityTypeProperty(property));
        }

        public IEnumerable<IForeignKeyProperty> GetForeignKeyProperties()
        {
            if (!_childElements.Any(c => c.ContainsForeignKey())) return new List<IForeignKeyProperty>();
            var rawPropertyInfo = _mapBuilder.BuildStartingXsdElementSerializedMapTuplesFor(_parsedResult);
            return rawPropertyInfo.Select(tuple => new ForeignKeyProperty(tuple.Item1, tuple.Item2)).Cast<IForeignKeyProperty>().ToList();
        }

        public IEnumerable<IMultiElementEntityCollectionProperty> GetMultiElementEntityCollectionProperties()
        {
            return _multiElementCollections
                .Select(multiElementCollection => new MultiElementEntityCollectionProperty(multiElementCollection.Value.ToArray()))
                .ToList();
        }

        public bool IsDescriptorsExtRef()
        {
            return _parsedResult.IsDescriptorsExtRef();
        }
    }
}