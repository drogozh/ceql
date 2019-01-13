namespace Ceql.Execution
{
    using Ceql.Contracts;
    using System;

    public class ResultTransaction<T> : Transaction
    {
        private Func<ITransaction, T> _body;


        public ResultTransaction(Func<ITransaction, T> transactionBody) : base(null)
        {
            _body = transactionBody;
        }


        public new T Execute()
        {
            T result = default(T);
            Execute(t => {
                result = _body(t);
            });
            return result;
        }
    }
}
