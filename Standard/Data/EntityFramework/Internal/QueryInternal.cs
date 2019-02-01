﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Collections;
using System.Linq.Expressions;

namespace TraceCore.Standard.Data.EntityFramework.Internal
{
    using TraceCore.Data;

#if DEBUG
    [System.Diagnostics.DebuggerDisplay(@"{ToStringDebug(),nq}")]
#endif
    class QueryInternal : IQuery, IOrderedQueryable
#if !NET40
    , IDbAsyncEnumerable
#endif
    {
        internal protected IQueryable _query = null;
        internal protected DB.DbQueryProvider _provider = null;

        public IRepository Repository { get; set; }

        public QueryInternal(IQueryable query, IRepository repo)
        {
            if (query == null) throw new ArgumentNullException(nameof(query), "Запрос не может быть null.");
            if (repo == null) throw new ArgumentNullException(nameof(repo), "Репозиторий не может быть null.");

            _query = query;
            Repository = repo;

            var d = _query.GetType();
        }

        public Type ElementType
        {
            get { return _query.ElementType; }
        }

        public Expression Expression
        {
            get { return _query.Expression; }
        }

        public IQueryProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    if (_query.Provider is DB.DbQueryProvider) _provider = _query.Provider as DB.DbQueryProvider;
                    else _provider = new DB.DbQueryProvider(_query.Provider, Repository as RepositoryInternalBase);
                }
                return _provider;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _query.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _query.GetEnumerator();
        }

#if !NET40
        public IDbAsyncEnumerator GetAsyncEnumerator()
        {
            return ((IDbAsyncEnumerable)_query).GetAsyncEnumerator();
        }
#endif

        public override string ToString()
        {
            var queryText = _query != null ? _query.ToString() : "";
            return queryText.TrimStart('{').TrimEnd('}');
        }

#if DEBUG
        internal string ToStringDebug()
        {
            var queryText = _query != null ? _query.ToString() : "";
            return queryText.TrimStart('{').TrimEnd('}');
        }
#endif
    }

    class QueryInternal<TEntity> : QueryInternal, IQuery<TEntity>, IOrderedQueryable<TEntity>
#if !NET40
    , IDbAsyncEnumerable<TEntity> 
#endif
    {
        public QueryInternal(DbQuery query, IRepository repo) : base(query, repo)
        {
        }

        public QueryInternal(DbQuery<TEntity> query, IRepository repo) : base(query, repo)
        {
        }

        public QueryInternal(IQueryable<TEntity> query, IRepository repo) : base(query, repo)
        {
        }

        public new IEnumerator<TEntity> GetEnumerator()
        {
            return (_query as IQueryable<TEntity>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _query.GetEnumerator();
        }

        /// <summary>
        /// См. <see cref="IQuery{TEntity}.AsNoTracking"/>.
        /// </summary>
        public IQuery<TEntity> AsNoTracking()
        {
            return new QueryInternal<TEntity>(QueryableExtensions.AsNoTracking(DBQuery as IQueryable) as DbQuery, Repository);
        }

        /// <summary>
        /// См. <see cref="IQuery{TEntity}.IsNoTracking"/>.
        /// </summary>
        public bool IsNoTracking()
        {
            if (DBQuery.Expression.ToString().Contains("MergeAs(NoTracking)")) return true;
            return false;
        }

        /// <summary>
        /// См. <see cref="IQuery{TEntity}.Include(string)"/>.
        /// </summary>
        public IQuery<TEntity> Include(string path)
        {
            return new QueryInternal<TEntity>(QueryableExtensions.Include(DBQuery, path), Repository);
        }

        public IQuery<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> path)
        {
            //Check.NotNull<IQueryable<T>>(source, "source");
            //Check.NotNull<Expression<Func<T, TProperty>>>(path, "path");
            string text;
            if (!DbHelpers.TryParsePath(path.Body, out text) || text == null)
            {
                throw new ArgumentException("Strings.DbExtensions_InvalidIncludePathExpression", nameof(path));
            }
            return this.Include(text);
        }

#if !NET40
        IDbAsyncEnumerator<TEntity> IDbAsyncEnumerable<TEntity>.GetAsyncEnumerator()
        {
            return ((IDbAsyncEnumerable<TEntity>)DBQuery).GetAsyncEnumerator();
        }
#endif

        protected IQueryable<TEntity> DBQuery
        {
            get { return _query as IQueryable<TEntity>; }
        }
    }
}
