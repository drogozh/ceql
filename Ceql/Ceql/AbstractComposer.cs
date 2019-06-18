namespace Ceql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Ceql.Execution;
    using Ceql.Contracts;
    using Ceql.Contracts.Attributes;
    using Ceql.Composition;
    using Ceql.Statements;
    using Ceql.Configuration;

    public abstract class AbstractComposer
    {
        public static ITransaction Transaction(Action<TransactionBody> transactionBody)
        {
            return new Transaction(transactionBody);
        }

        /// <summary>
        /// Transaction the specified transactionBody.
        /// </summary>
        /// <returns>The transaction.</returns>
        /// <param name="transactionBody">Transaction body.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static ResultTransaction<T> Transaction<T>(Func<TransactionBody,T> transactionBody)
        {
            return new ResultTransaction<T>(transactionBody);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string TableName(Type tableType)
        {
            var attr = tableType.GetTypeInfo().CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof (Table));

            if (attr == null) return null;
            return attr.ConstructorArguments[0].Value.ToString();
        }

        /// <summary>
        /// Returns a select clause that will result in select SQl however does not use any tables 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static SelectClause<T> PseudoSelect<T>(Expression<SelectExpression<T>> expression)
        {
            return new SelectClause<T>(expression);
        }

        /// <summary>
        /// Top level table select
        /// Use type parameter to specify table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static FromClause<T> From<T>() where T : ITable
        {
            return new FromClause<T>();
        }

        /// <summary>
        /// Top level sub-query select
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selectClause"></param>
        /// <returns></returns>
        public static FromClause<TResult> From<TResult>(SelectClause<TResult> selectClause)
        {
            return new FromClause<TResult>(selectClause);
        }

        /// <summary>
        /// Executes plain SQL statement immediately
        /// For testing purposes only, not to be used over untrusted input
        /// Query text is not checked for validity or injection
        /// </summary>
        /// <param name="sql"></param>
        public static void Sql(string sql)
        {
            using(var connection = CeqlConfiguration.Instance.GetConnection())
            {
                connection.Open();
                try 
                {
                    using(var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.ExecuteScalar();
                    }
                } 
                finally 
                {
                    connection.Close();
                }
            }   
        }

        /// <summary>
        /// Executes plain sql statement via select clause
        /// </summary>
        /// <param name="sql"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static SelectClause<TResult> Sql<TResult>(string sql)
        {
            return null;
        }

        /// <summary>
        /// In clause generator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool In<T>(IEnumerable<T> list, T item)
        {
            return true;
        }

        /// <summary>
        /// Like clause generator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="like"></param>
        /// <returns></returns>
        public static bool Like<T>(T item, string like)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T Max<T>(T item)
        {
            return item;
        }

        public static int Count<T>(T item)
        {
            return 0;
        }
    }
}
