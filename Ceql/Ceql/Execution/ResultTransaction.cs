namespace Ceql.Execution
{
    using Ceql.Contracts;
    using System;

    public class ResultTransaction<T> : BaseTransaction
    {
        private Func<TransactionBody, T> _body;
        private T _result = default(T);
        public ResultTransaction(Func<TransactionBody, T> transactionBody)
        {
            _body = transactionBody;
        }

        public new T Execute()
        {
            base.Execute();
            return _result;
        }

        protected override void DoTransaction()
        {
            _result = _body(new TransactionBody(this));
        }
    }
}
