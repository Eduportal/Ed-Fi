namespace EdFi.Ods.Tests.TestObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    public class TestDbSet<TEntity> : DbSet<TEntity>, IQueryable, IEnumerable<TEntity>, IDbAsyncEnumerable<TEntity> where TEntity : class
    {
        private ObservableCollection<TEntity> _data;
        private IQueryable _query;

        public TestDbSet()
        {
            this._data = new ObservableCollection<TEntity>();
            this._query = this._data.AsQueryable();
        }

        public override TEntity Add(TEntity entity)
        {
            this._data.Add(entity);
            return entity;
        }

        public override TEntity Remove(TEntity entity)
        {
            this._data.Remove(entity);
            return entity;
        }

        public override TEntity Attach(TEntity entity)
        {
            if (this._data.Contains(entity)) this._data.Remove(entity);
            this._data.Add(entity);
            return entity;
        }

        public override TEntity Create()
        {
            return Activator.CreateInstance<TEntity>();
        }

        public override TDerivedEntity Create<TDerivedEntity>()
        {
            return Activator.CreateInstance<TDerivedEntity>();
        }

        public override ObservableCollection<TEntity> Local
        {
            get { return this._data; }
        }

        Type IQueryable.ElementType
        {
            get { return this._query.ElementType; }
        }

        Expression IQueryable.Expression
        {
            get { return this._query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new TestDbAsyncQueryProvider<TEntity>(this._query.Provider); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._data.GetEnumerator();
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return this._data.GetEnumerator();
        }

        IDbAsyncEnumerator<TEntity> IDbAsyncEnumerable<TEntity>.GetAsyncEnumerator()
        {
            return new TestDbAsyncEnumerator<TEntity>(this._data.GetEnumerator());
        }

        internal class TestDbAsyncEnumerator<T> : IDbAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;


            public TestDbAsyncEnumerator(IEnumerator<T> inner)
            {
                this._inner = inner;
            }

            public void Dispose()
            {
                this._inner.Dispose();
            }

            public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(this._inner.MoveNext());
            }

            public T Current { get { return this._inner.Current; } }

            object IDbAsyncEnumerator.Current
            {
                get { return this.Current; }
            }
        }

        internal class TestDbAsyncEnumerable<T> : EnumerableQuery<T>, IDbAsyncEnumerable<T>
        {
            public TestDbAsyncEnumerable(Expression expression) : base(expression) {}

            public TestDbAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable){}

            public IDbAsyncEnumerator<T> GetAsyncEnumerator()
            {
                return new TestDbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
            {
                return this.GetAsyncEnumerator();
            }

            public IQueryProvider Provider
            {
                get { return new TestDbAsyncQueryProvider<T>(this); }
            }
        }

        internal class TestDbAsyncQueryProvider<TEntity_internal> : IDbAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestDbAsyncQueryProvider(IQueryProvider provider)
            {
                this._inner = provider;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestDbAsyncEnumerable<TEntity_internal>(expression);
            }


            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestDbAsyncEnumerable<TElement>(expression);
            }

            public object Execute(Expression expression)
            {
                return this._inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return this._inner.Execute<TResult>(expression);
            }

            public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
            {
                return Task.FromResult(this.Execute(expression));
            }

            public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                return Task.FromResult(this.Execute<TResult>(expression));
            }
        }
    }
}