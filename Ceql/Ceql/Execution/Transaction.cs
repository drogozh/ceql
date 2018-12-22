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

    public class Transaction : ITransaction
    {
        private Action<ITransaction> _body;
        private IDbConnection _connection;
        private IDataConnector _connector;

        public Transaction(Action<ITransaction> transactionBody)
        {
            _body = transactionBody;
        }

        public virtual T InsertSingle<T>(T entity) where T : ITable 
        {
            Insert<T>(new List<T>(){entity});
            return entity;
        }

        public virtual IEnumerable<T> Insert<T>(IEnumerable<T> entities) where T : ITable
        {
            return Insert(entities, new InsertClause<T>().Model);
        }

        public virtual IEnumerable<T> FullInsert<T>(IEnumerable<T> entities) where T : ITable
        {
            return Insert(entities,new InsertClause<T>().FullModel);
        }

        private IEnumerable<T> Insert<T>(IEnumerable<T> entities, Model.InsertStatementModel<T> model) where T : ITable
        {
            var command = _connection.CreateCommand();

            foreach (var entity in entities)
            {
                command.CommandText = model.ApplyParameters(entity);
                _connector.PreInsert<T>(command, entity, model.IsFull);
                command.ExecuteScalar();
                _connector.PostInsert<T>(command, entity, model.IsFull);
            }
            return entities;
        }

        /// <summary>
        /// Generates and executes DELETE statement for each 
        /// entity in the entities collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public virtual void Delete<T>(IEnumerable<T> entities) where T : ITable
        {
            var model = new DeleteStatement<T>().Model;
            using(var command = _connection.CreateCommand())
            {
                foreach (var entity in entities)
                {
                    command.CommandText = model.ApplyParameters(entity);
                    command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Deletes records using boolean expression
        /// <summary>
        public virtual void Delete<T>(Expression<BooleanExpression<T>> expression) where T : ITable
        {
            var whereClause = new WhereClause<T>(new FromClause<T>(), expression);
            var model = new DeleteStatement<T>(whereClause).Model;
            // todo: hack, refactor
            var sql = "DELETE FROM T0 USING " + model.FromSql.Replace("FROM","") + " " + model.WhereSql;

            using(var command = _connection.CreateCommand()){
                command.CommandText = sql;
                command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Update statement is implemented as a series of DELETE and INSERT statements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        public virtual void Update<T>(IEnumerable<T> entities) where T : ITable
        {
            Delete(entities);
            FullInsert(entities);
            return; 
        }

        /// <summary>
        /// Executes transaction statements
        /// </summary>
        protected void Execute(Action<ITransaction> action)
        {
            _connection = CeqlConfiguration.Instance.GetConnection();
            _connector = CeqlConfiguration.Instance.GetConnector();

            _connection.Open();

            var dbTransaction = _connection.BeginTransaction();

            try
            {
                action(this);
                dbTransaction.Commit();
            }
            catch (Exception)
            {
                dbTransaction.Rollback();
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Execute()
        {
            Execute(_body);
        }
    }

    public class ResultTransaction<T> : Transaction 
    {
        private Func<ITransaction,T> _body;

        public ResultTransaction(Func<ITransaction,T> transactionBody): base(null)
        {
            _body = transactionBody;
        }

        public new T Execute()
        {
            T result = default(T);
            this.Execute(t=> {
                result = _body(t);
            });
            return result;
        }
    }
}
