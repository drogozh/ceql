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

    public class Transaction : ITransaction
    {
        private Action<ITransaction> _body;
        private IDbConnection _connection;
        private IDataConnector _connector;


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Ceql.Execution.Transaction"/> class.
        /// </summary>
        /// <param name="transactionBody">Transaction body.</param>
        public Transaction(Action<ITransaction> transactionBody)
        {
            _body = transactionBody;
        }


        /// <summary>
        /// Inserts the single.
        /// </summary>
        /// <returns>The single.</returns>
        /// <param name="entity">Entity.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public virtual T InsertSingle<T>(T entity) where T : ITable 
        {
            Insert<T>(new List<T>(){entity});
            return entity;
        }


        /// <summary>
        /// Insert the specified entities.
        /// </summary>
        /// <returns>The insert.</returns>
        /// <param name="entities">Entities.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public virtual IEnumerable<T> Insert<T>(IEnumerable<T> entities) where T : ITable
        {
            return Insert(entities, new InsertClause<T>().Model);
        }


        /// <summary>
        /// Fulls the insert.
        /// </summary>
        /// <returns>The insert.</returns>
        /// <param name="entities">Entities.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public virtual IEnumerable<T> FullInsert<T>(IEnumerable<T> entities) where T : ITable
        {
            return Insert(entities,new InsertClause<T>().FullModel);
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
        /// <param name="entities"></param>
        public virtual void Update<T>(IEnumerable<T> entities) where T : ITable
        {
            Delete(entities);
            FullInsert(entities);
            return; 
        }


        /// <summary>
        /// Initiates update statement
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="expressions">Expressions.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IUpdateStatement<T> Set<T>(params Expression<SelectExpression<T, object>>[] expressions) where T : ITable
        {
            return new UpdateStatementExec<T>(this, expressions);
        }


        /// <summary>
        /// Execute this instance.
        /// </summary>
        public void Execute()
        {
            Execute(_body);
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


        #region Private Methods
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
        #endregion


        #region nested classes
        public class UpdateStatementExec<T> : UpdateStatement<T> where T : ITable
        {
            private Transaction _transaction;

            public UpdateStatementExec(Transaction transaction, params Expression<SelectExpression<T, object>>[] expressions)
                : base(expressions)
            {
                _transaction = transaction;
            }

            public override void Update<T1>(IEnumerable<T1> entities)
            {
                var formatter = CeqlConfiguration.Instance.GetConnectorFormatter();
                var sql = Sql + WhereClause != null ? " AND " : " WHERE ";

                using (var command = _transaction._connection.CreateCommand()) 
                {
                    foreach (var entity in entities)
                    {
                        var commandSql = sql;
                        var condition = "";
                        foreach (var property in typeof(T1).GetPrimaryKeyProperties())
                        {
                            var value = property.GetValue(entity);
                            var fieldName = TypeHelper.GetFieldName(property);
                            commandSql += (condition + _fromAlias[0].Name + "."+fieldName + "=" + formatter.Format(value));
                            condition = " AND ";
                        }

                        command.CommandText = sql;
                    }
                }
            }
        }
        #endregion
    }

}
