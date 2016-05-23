namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using global::EdFi.Ods.BulkLoad.Core;

    public class StubAggregateLoader : ILoadAggregates
    {
        private readonly Type _aggregateType;
        private bool _iWasCalled;
        private long _processedOnUtcDateTime;
        //private bool _loadedAll;
        private DateTime? _timeIWasCalled;
        private int _sourceElementCount;

        public StubAggregateLoader(Type aggregateType)
        {
            this._aggregateType = aggregateType;
            this._iWasCalled = false;
            //_loadedAll = true;
            this._timeIWasCalled = null;
        }

        public void SetSourceElementCount(int count)
        {
            this._sourceElementCount = count;
        }

        public DateTime? TimeIWasAskedToProcess()
        {
            return this._timeIWasCalled;
        }

        public bool WasAskedToProcessAggregates()
        {
            return this._iWasCalled;
        }

        public long ProcessedOn()
        {
            return this._processedOnUtcDateTime;
        }

        public Task<LoadAggregateResult> LoadFrom(IIndexedXmlFileReader reader)
        {
            return Task<LoadAggregateResult>.Factory.StartNew(() =>
            {
                this._processedOnUtcDateTime = DateTime.UtcNow.Ticks;
                this._iWasCalled = true;
                this._timeIWasCalled = DateTime.UtcNow;
                Thread.Sleep(100);
                return new LoadAggregateResult{SourceElementCount = this._sourceElementCount};
            });
        }

        public Type GetAggregateType()
        {
            return this._aggregateType;
        }
    }
}