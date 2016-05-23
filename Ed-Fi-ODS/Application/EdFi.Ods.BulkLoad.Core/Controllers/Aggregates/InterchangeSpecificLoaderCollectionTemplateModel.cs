//<#+

#if NOT_IN_T4
namespace EdFi.Ods.BulkLoad.Common
{
using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.BulkLoad.Common;
using EdFi.Ods.CodeGen.XmlShredding;
using EdFi.Ods.CodeGen.XsdToWebApi;
#endif

    /// <summary>
    /// This is the 'ViewModel' for the LoaderCollection.tt template and should
    /// never, ever, be used for anything else.
    /// 
    /// 
    /// Ever.
    /// </summary>
    public class InterchangeSpecificLoaderCollectionTemplateModel
    {
        private Dictionary<string, OrderedCollectionOfSets<string>> _loaderCollections;

        public InterchangeSpecificLoaderCollectionTemplateModel(IEnumerable<string> xsdPaths, IDictionary<string, IOrderedCollectionofAggregateSetsBuilder> dictionaryOfOrderedCollectionBuilders)
        {
            _loaderCollections = new Dictionary<string, OrderedCollectionOfSets<string>>();
            var interchangeObjects = (from xsdPath in xsdPaths select InterchangeLoader.Load(xsdPath) into results where results.Any() select results.Single()).ToList();
            foreach (var parsedSchemaObject in interchangeObjects)
            {
                //The Assumption - there's that word again - is every interchange name begins with "Interchange"
                var interchangeName = parsedSchemaObject.XmlSchemaObjectName.Substring(11).ToLowerInvariant();
                var builder = dictionaryOfOrderedCollectionBuilders.ContainsKey(interchangeName)?dictionaryOfOrderedCollectionBuilders[interchangeName]:null;
                if (builder == null)
                    throw new Exception(interchangeName + " not found in dictionary of ordered collection builders");
                //The Assumption (and yes it is a scary word - hence the apology) here is that the aggregates are the root elements of the xsd . . .
                var knownAggregates = parsedSchemaObject.GetChildElementsToBeParsed().Select(c => c.XmlSchemaObjectName).ToList();//once studenttranscript is merged this will need to use the skipless collection . . .
                if (_loaderCollections.ContainsKey(interchangeName))
                {
                    if (parsedSchemaObject.IsExtension)
                    {
                        _loaderCollections.Remove(interchangeName);
                        _loaderCollections.Add(interchangeName,
                            builder.BuildOrderedCollectionOfSetsFrom(knownAggregates));
                    }
                }
                else
                {
                    _loaderCollections.Add(interchangeName,
                        builder.BuildOrderedCollectionOfSetsFrom(knownAggregates));                    
                }
            }
        }

        public IEnumerable<string> Interchanges
        {
            get { return _loaderCollections.Keys; }
        }

        public OrderedCollectionOfSets<string> CollectionFor(string interchangeName)
        {
            if (_loaderCollections.ContainsKey(interchangeName))
                return _loaderCollections[interchangeName];
            throw new Exception(string.Format(
                "No aggregate loaders found for interchange {0}.  This should not happen.  Please check the HaveInterchangeSpecificLoaderCollections object, it's covering tests, and the InterchangeTypes object.",
                interchangeName));
        }
    }

#if NOT_IN_T4
}
#endif
//#>