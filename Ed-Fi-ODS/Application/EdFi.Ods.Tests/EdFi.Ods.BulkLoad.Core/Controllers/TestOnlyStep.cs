namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Entities.Common;
    using global::EdFi.Ods.Pipelines.Common;
    using global::EdFi.Ods.Pipelines.Put;

    public class TestOnlyStep<T,TEntity> : IStep<PutContext<T, TEntity>, PutResult> where T : IHasETag where TEntity : class, IHasIdentifier
    {
        private readonly PutResult[] _results;
        private readonly PutResult _result;
        public List<T> PersistedResources = new List<T>();
        private readonly Object _lock = new Object();

        public TestOnlyStep(PutResult result)
        {
            this._result = result;
        }

        public TestOnlyStep(PutResult[] results)
        {
            this._results = results;
        }

        protected PutResult GetResult(PutContext<T, TEntity> context)
        {
            if (this._results == null || this._results.Length <= 0) return this._result;

            var retResult = this._result;

            var tmp = this._results.FirstOrDefault(x => x.ETag == context.Resource.ETag);
            if (tmp != null)
                retResult = tmp;

            return retResult;
        }

        public void Execute(PutContext<T, TEntity> context, PutResult result)
        {
            var setResult = this.GetResult(context);

            result.ResourceWasCreated = setResult.ResourceWasCreated;
            result.ResourceId = setResult.ResourceId;
            result.ResourceWasUpdated = setResult.ResourceWasUpdated;
            result.ResourceWasDeleted = setResult.ResourceWasDeleted;
            result.ResourceWasPersisted = setResult.ResourceWasPersisted;
            result.ETag = setResult.ETag;
            result.Exception = setResult.Exception;

            if (!setResult.ResourceWasCreated && !setResult.ResourceWasUpdated && !setResult.ResourceWasPersisted) return;

            lock (this._lock)
            {
                this.PersistedResources.Add(context.Resource);
            }
        }
    }
}