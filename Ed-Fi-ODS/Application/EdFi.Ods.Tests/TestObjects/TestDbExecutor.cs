namespace EdFi.Ods.Tests.TestObjects
{
    using System;
    using System.Collections.Generic;

    using global::EdFi.Ods.Api.Data.Contexts;

    public class TestDbExecutor : IDbExecutor<IBulkOperationDbContext>
    {
        private readonly IBulkOperationDbContext _context;

        public TestDbExecutor(IBulkOperationDbContext context)
        {
            this._context = context;
        }

        public void ApplyChanges(Action<IBulkOperationDbContext> action)
        {
            action(this._context);
        }

        public IEnumerable<TResult> Get<TResult>(Func<IBulkOperationDbContext, IEnumerable<TResult>> action)
        {
            return action(this._context);
        }

        public TResult Get<TResult>(Func<IBulkOperationDbContext, TResult> action)
        {
            return action(this._context);
        }
    }
}