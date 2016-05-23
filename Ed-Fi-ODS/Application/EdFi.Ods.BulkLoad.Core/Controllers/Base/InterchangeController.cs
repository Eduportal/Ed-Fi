using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EdFi.Ods.BulkLoad.Common;

namespace EdFi.Ods.BulkLoad.Core.Controllers.Base
{
    public class InterchangeController : IInterchangeController
    {
        private readonly OrderedCollectionOfSets<ILoadAggregates> _loaders;
        private readonly Func<string, IIndexedXmlFileReader> _reader;

        public InterchangeController(OrderedCollectionOfSets<ILoadAggregates> loaders, Func<string, IIndexedXmlFileReader> reader)
        {
            _loaders = loaders;
            _reader = reader;
        }

        public async Task<LoadResult> LoadAsync(string fromFile)
        {
            try
            {
                using (var indexedFile = _reader(fromFile))
                {
                    var aggregateLoaderResults = new List<LoadAggregateResult>();
                    foreach (var setOfLoaders in _loaders)
                    {
                        aggregateLoaderResults.AddRange(await Task.WhenAll(setOfLoaders.Select(l => l.LoadFrom(indexedFile))));
                    }

                    if (!aggregateLoaderResults.Any()) return BuildEmptyLoadResult();

                    var allExceptions =
                        aggregateLoaderResults.SelectMany(x => x.LoadExceptions)
                            .ToList();
                    var loadedResourceCount =
                        aggregateLoaderResults.Sum(x => x.RecordsLoaded);
                    var sourceElementCount = aggregateLoaderResults.Sum(x => x.SourceElementCount);
                    return new LoadResult
                    {
                        LoadExceptions = allExceptions,
                        LoadedResourceCount = loadedResourceCount,
                        SourceElementCount = sourceElementCount
                    };
                }
            }
            catch (Exception exception)
            {
                return BuildFailureResult(exception);
            }
        }

        private static LoadResult BuildFailureResult(Exception exception)
        {
            return new LoadResult
            {
                LoadExceptions = new List<LoadException> {new LoadException {Exception = exception}}
            };
        }

        private static LoadResult BuildEmptyLoadResult()
        {
            return new LoadResult();
        }
    }
}