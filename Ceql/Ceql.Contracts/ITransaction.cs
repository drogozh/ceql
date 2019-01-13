namespace Ceql.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime;

    public interface ITransaction
    {
        void Execute();
        T InsertSingle<T>(T entity) where T : ITable; 

        IEnumerable<T> Insert<T>(IEnumerable<T> records) where T : ITable;

        /**
            Insert record for every field in the table
            providing values for auto increment fields also
         */
        IEnumerable<T> FullInsert<T>(IEnumerable<T> records) where T : ITable;
        void Delete<T>(IEnumerable<T> records) where T : ITable;
        void Delete<T>(Expression<BooleanExpression<T>> records) where T : ITable;
        void Update<T>(IEnumerable<T> records) where T: ITable;
        IUpdateStatement<T> Set<T>(params Expression<SelectExpression<T, object>>[] expressions) where T: ITable;
    }
}
