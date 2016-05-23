using System;
using System.Collections.Generic;

namespace EdFi.Ods.Api.Data.Contexts
{
    public class DbExecutor<T> : IDbExecutor<T> where T : IDbContext
    {
        private readonly Func<T> contextFactory;

        public DbExecutor(Func<T> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public void ApplyChanges(Action<T> action)
        {

            using (var context = contextFactory())
            {
                Apply(context, action);
            }
        }

        private static void Apply(T context, Action<T> action)
        {
            action(context);
            context.SaveChanges();
        }

        public IEnumerable<TResult> Get<TResult>(Func<T, IEnumerable<TResult>> action)
        {
            using (var context = contextFactory())
            {
                return action(context);
            }
        }

        public TResult Get<TResult>(Func<T, TResult> action)
        {
            using (var context = contextFactory())
            {
                return action(context);
            }
        }
    }
}