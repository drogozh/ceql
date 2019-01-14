namespace Ceql.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq.Expressions;
    using System.Runtime;

    public interface ITransaction
    {
        IDbConnection Connection {get;}
        IDataConnector Connector {get;}
        void Execute();
    }
}
