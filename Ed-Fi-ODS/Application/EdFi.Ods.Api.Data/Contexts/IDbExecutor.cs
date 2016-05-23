using System;
using System.Collections.Generic;

namespace EdFi.Ods.Api.Data.Contexts
{
    public interface IDbExecutor<out T> where T : IDbContext
    {
        void ApplyChanges(Action<T> action);
        IEnumerable<TResult> Get<TResult>(Func<T, IEnumerable<TResult>> action);
        TResult Get<TResult>(Func<T, TResult> action);
    }
}