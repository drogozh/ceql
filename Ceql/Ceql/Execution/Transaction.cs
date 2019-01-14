namespace Ceql.Execution
{
    using Ceql.Contracts;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Ceql.Composition;
    using Ceql.Configuration;
    using System.Linq.Expressions;
    using Ceql.Statements;
    using Ceql.Utils;

    public class Transaction : BaseTransaction
    {
        private Action<TransactionBody> _action;

        public Transaction(Action<TransactionBody> action)
        {
            _action = action;
        }

        protected override void DoTransaction()
        {
            _action(new TransactionBody(this));
        }
    }
}
