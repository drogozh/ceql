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

    public class TransactionBody : ITransactionBody
    {
        ITransaction _transaction;        

        public TransactionBody(ITransaction transaction)
        {
            _transaction = transaction;
        }

        /// <summary>
        /// Inserts the single.
        /// </summary>
        /// <returns>The single.</returns>
        /// <param name="entity">Entity.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public virtual T InsertSingle<T>(T entity)
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
        public virtual IEnumerable<T> Insert<T>(IEnumerable<T> entities)
        {
            return Insert(entities, new InsertClause<T>().Model);
        }

        /// <summary>
        /// Fulls the insert.
        /// </summary>
        /// <returns>The insert.</returns>
        /// <param name="entities">Entities.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public virtual IEnumerable<T> FullInsert<T>(IEnumerable<T> entities)
        {
            return Insert(entities,new InsertClause<T>().FullModel);
        }

        /// <summary>
        /// Generates and executes DELETE statement for each 
        /// entity in the entities collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public virtual void Delete<T>(IEnumerable<T> entities)
        {
            var model = new DeleteStatement<T>().Model;
            using(var command = _transaction.Connection.CreateCommand())
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
        public virtual void Delete<T>(Expression<BooleanExpression<T>> expression)
        {
            var whereClause = new WhereClause<T>(new FromClause<T>(), expression);
            var model = new DeleteStatement<T>(whereClause).Model;
            // todo: hack, refactor
            var sql = "DELETE FROM T0 USING " + model.FromSql.Replace("FROM","") + " " + model.WhereSql;

            using(var command = _transaction.Connection.CreateCommand()){
                command.CommandText = sql;
                command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Update statement is implemented as a series of DELETE and INSERT statements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public virtual void Update<T>(IEnumerable<T> entities)
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
        public IUpdateStatement<T> Set<T>(params Expression<SelectExpression<T, object>>[] expressions)
        {
            return new UpdateStatementExec<T>(_transaction, expressions);
        }

        #region Private Methods
        private IEnumerable<T> Insert<T>(IEnumerable<T> entities, Model.InsertStatementModel<T> model)
        {
            var command = _transaction.Connection.CreateCommand();

            foreach (var entity in entities)
            {
                command.CommandText = model.ApplyParameters(entity);
                _transaction.Connector.PreInsert<T>(command, entity, model.IsFull);
                command.ExecuteScalar();
                _transaction.Connector.PostInsert<T>(command, entity, model.IsFull);
            }
            return entities;
        }
        #endregion


        #region nested classes
        public class UpdateStatementExec<T> : UpdateStatement<T>
        {
            private ITransaction _transaction;

            public UpdateStatementExec(ITransaction transaction, params Expression<SelectExpression<T, object>>[] expressions)
                : base(expressions)
            {
                _transaction = transaction;
            }

            public override void Update<T1>(IEnumerable<T1> entities)
            {
                var formatter = CeqlConfiguration.Instance.GetConnectorFormatter();
                var sql = Sql + (WhereClause != null ? " AND " : " WHERE ");

                using (var command = _transaction.Connection.CreateCommand()) 
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
                        command.CommandText = commandSql;
                        command.ExecuteScalar();
                    }
                }
            }
        }
        #endregion
    }
}