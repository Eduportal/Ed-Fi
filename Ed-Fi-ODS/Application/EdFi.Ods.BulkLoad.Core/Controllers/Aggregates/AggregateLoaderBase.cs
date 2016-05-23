using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines;
using EdFi.Ods.Pipelines.Put;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.XmlShredding;

namespace EdFi.Ods.BulkLoad.Core.Controllers.Aggregates
{
    public abstract class AggregateLoaderBase<T, TEntity> : ILoadAggregates
        where TEntity : class, IHasIdentifier
        where T : IHasETag
    {
        private class LoadResult
        {
            public bool HasUnresolvedDependency { get; set; }
            public PutResult PutResult { get; set; }
            public T Resource { get; set; }
            public XElement Element { get; set; }
        }

        private readonly string _aggregateName;
        private readonly IResourceFactory<T> _factory;
        private readonly ICreateOrUpdatePipeline<T, TEntity> _putPipeline;
        private readonly List<PropertyInfo> _naturalKeyProperties;
        private readonly List<PropertyInfo> _parentKeyProperties;
        private bool _isSelfReferencedAggregate;

        protected AggregateLoaderBase(IResourceFactory<T> factory, ICreateOrUpdatePipeline<T, TEntity> putPipeline)
        {
            _factory = factory;
            _putPipeline = putPipeline;
            var entityType = typeof (T);
            _aggregateName = entityType.Name;
            var interfaceType = entityType.GetInterface("I" + entityType.Name);
            _naturalKeyProperties = interfaceType.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(NaturalKeyMemberAttribute))).ToList();
            _parentKeyProperties = new List<PropertyInfo>();
            foreach (var naturalKeyProperty in _naturalKeyProperties)
            {
                _parentKeyProperties.AddRange(interfaceType.GetProperties()
                        .Where(
                            prop => prop.Name == "Parent" + naturalKeyProperty.Name)
                        .ToList());
            }
            _isSelfReferencedAggregate = (_naturalKeyProperties.Count > 0 &&
                                          _parentKeyProperties.Count == _naturalKeyProperties.Count);
        }

        public async Task<LoadAggregateResult> LoadFrom(IIndexedXmlFileReader reader)
        {
            var loadedResources = new ConcurrentBag<string>();

            var xElements = reader.GetNodesByEntity(_aggregateName).ToList();
            if (!xElements.Any())
                return new LoadAggregateResult();

            var loadResults = await Task.WhenAll(xElements.Select(element =>
                LoadAggregate(
                    element,
                    loadedResources,
                    (xElement) => _factory.Build(xElement, reader))
                ));

            var tasks = await IterateDependencies(
                loadResults,
                loadedResources,
                0);

            var recordsLoaded = tasks.Count(x => x.PutResult.ResourceWasPersisted);
            var loadExceptions = tasks
                .Where(t => t.PutResult.Exception != null)
                .Select(t => new LoadException
                {
                    Exception = t.PutResult.Exception,
                    Element = t.Element.ToString(SaveOptions.DisableFormatting)
                }).ToArray();
            return new LoadAggregateResult
            {
                RecordsLoaded = recordsLoaded,
                LoadExceptions = loadExceptions,
                SourceElementCount = xElements.Count
            };
        }

        private async Task<LoadResult[]> IterateDependencies(LoadResult[] loadResults, ConcurrentBag<string> loadedResources, int loadedCount)
        {
            var unresolvedDependencies = loadResults.Where(x => x.HasUnresolvedDependency).ToArray();
            var aggregatesLoaded = loadResults.Where(t => !t.HasUnresolvedDependency).ToArray();
            if (!unresolvedDependencies.Any())
                return loadResults;
            // once we hit the wall, all bets are off... try loading what's left
            if (aggregatesLoaded.Length <= loadedCount)
                _isSelfReferencedAggregate = false;


            var loadedInThisRecursion = aggregatesLoaded.Length;

            var newLoadResults =
                aggregatesLoaded.Union(await Task.WhenAll(unresolvedDependencies.Select(
                    t =>
                        LoadAggregate(t.Element, loadedResources,
                            (element) => t.Resource)))).ToArray();

            return await IterateDependencies(newLoadResults, loadedResources, loadedInThisRecursion);
        }

        private async Task<LoadResult> LoadAggregate(XElement element,
            ConcurrentBag<string> loadedResources,
            Func<XElement, T> resourceBuilder)
        {
            T resource;
            try
            {
                resource = await Task.Run(() => resourceBuilder(element));
                if (_isSelfReferencedAggregate)
                {
                    var parentKey = GetKey(resource, _parentKeyProperties);
                    if (!string.IsNullOrWhiteSpace(parentKey) && !loadedResources.Contains(parentKey))
                    {
                        return new LoadResult
                        {
                            HasUnresolvedDependency = true,
                            Resource = resource,
                            Element = element
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new LoadResult
                {
                    HasUnresolvedDependency = false,
                    PutResult =
                        new PutResult { Exception = ex, ResourceWasCreated = false, ResourceWasUpdated = false },
                    Element = element
                };
            }
            try
            {
                var putResult = _putPipeline.Process(new PutContext<T, TEntity>(resource, new ValidationState()));
                if (_isSelfReferencedAggregate)
                    loadedResources.Add(GetKey(resource, _naturalKeyProperties));
                return new LoadResult
                {
                    HasUnresolvedDependency = false,
                    Resource = resource,
                    PutResult = putResult,
                    Element = element
                };
            }
            catch (Exception pipelineExceptionThatShouldNotHappen)
            {
                return new LoadResult
                {
                    HasUnresolvedDependency = false,
                    Resource = resource,
                    PutResult =
                        new PutResult
                        {
                            Exception = pipelineExceptionThatShouldNotHappen,
                            ResourceWasCreated = false,
                            ResourceWasUpdated = false
                        },
                    Element = element
                };
            }
        }

        private static string GetKey(T resource, IEnumerable<PropertyInfo> properties)
        {
            return string.Join(string.Empty, properties.Select(prop => prop.GetValue(resource)));
        }

        public Type GetAggregateType()
        {
            return typeof(T);
        }
    }
}