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

    public abstract class BaseTransaction : ITransaction
    {
        public IDbConnection Connection {get; private set;}
        public IDataConnector Connector {get; private set;}

        protected abstract void DoTransaction();

        public void Execute()
        {
            Connector = CeqlConfiguration.Instance.GetConnector();
            Connection = CeqlConfiguration.Instance.GetConnection();
            Connection.Open();
            var dbTransaction = Connection.BeginTransaction();

            try
            {
                DoTransaction();
                dbTransaction.Commit();
            }
            catch (Exception)
            {
                dbTransaction.Rollback();
                throw;
            }
            finally
            {
                Connection.Close();
            }
        }
    }

}