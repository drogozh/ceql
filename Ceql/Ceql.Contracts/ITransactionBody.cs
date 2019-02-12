namespace Ceql.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq.Expressions;
    using System.Runtime;

    public interface ITransactionBody 
    {
        void Update<T>(IEnumerable<T> entities);
        T InsertSingle<T>(T entity);
        IEnumerable<T> Insert<T>(IEnumerable<T> records);
        IEnumerable<T> FullInsert<T>(IEnumerable<T> records);
        void Delete<T>(IEnumerable<T> records);
        void Delete<T>(Expression<BooleanExpression<T>> records);
        IUpdateStatement<T> Set<T>(params Expression<SelectExpression<T, object>>[] expressions);
    }
}