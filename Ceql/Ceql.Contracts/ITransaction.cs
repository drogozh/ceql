namespace Ceql.Contracts
{
    using System.Collections.Generic;

    public interface ITransaction
    {
        void Execute();
        T InsertSingle<T>(T entity) where T : ITable; 

        IEnumerable<T> Insert<T>(IEnumerable<T> records) where T : ITable;

        IEnumerable<T> FullInsert<T>(IEnumerable<T> records) where T : ITable;
        void Delete<T>(IEnumerable<T> records) where T : ITable;
        void Update<T>(IEnumerable<T> records) where T : ITable;
    }
}
