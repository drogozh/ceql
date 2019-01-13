namespace Ceql.Statements
{
    using Ceql.Composition;
    using Ceql.Configuration;
    using Ceql.Contracts;
    using Ceql.Expressions;
    using Ceql.Generation;
    using Ceql.Model;
    using Ceql.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public class UpdateStatement<T> : ISqlExpression, IUpdateStatement<T> where T : ITable
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public Type Type
        {
            get
            {
                return typeof(T);
            }
        }


        /// <summary>
        /// Gets the sql.
        /// </summary>
        /// <value>The sql.</value>
        public string Sql { get; private set; }


        /// <summary>
        /// Excludes auto generated column values
        /// </summary>
        public UpdateStatementModel<T> Model
        {
            get
            {
                return UpdateStatementGenerator.Generate<T>(this);
            }
        }


        /// <summary>
        /// Includes all fields
        /// </summary>
        public UpdateStatementModel<T> FullModel
        {
            get
            {
                return UpdateStatementGenerator.Generate<T>(this);
            }
        }


        private List<SelectAlias> _selections = new List<SelectAlias>();
        protected WhereClause<T> WhereClause;
        private object[] _values;
        protected readonly FromClause<T> _fromClause;
        protected readonly List<FromAlias> _fromAlias;
        private readonly List<PropertyInfo> _primaryKeyProperties;
        private readonly IConnectorFormatter _formatter;


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Ceql.Statements.UpdateStatement`1"/> class.
        /// </summary>
        /// <param name="expressions">Expressions.</param>
        public UpdateStatement(params Expression<SelectExpression<T,object>>[] expressions)
        {
            _formatter = CeqlConfiguration.Instance.GetConnectorFormatter();

            _fromClause = new FromClause<T>();
            _primaryKeyProperties = typeof(T).GetPrimaryKeyProperties();
            _fromAlias = StatementGenerator.GetAliasList(_fromClause);

            foreach (var expression in expressions)
            {
                var selectList = (List<SelectAlias>)new SelectExpressionAnalyzer(_formatter, expression, _fromAlias).Sql();
                _selections.AddRange(selectList);
            }
        }


        /// <summary>
        /// Applies update statement values to be set on fields specified in constructor expressions.
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="values">Values.</param>
        public IUpdateStatement<T> Values(params object[] values)
        {
            _values = values;

            // create sql statement
            Sql = "UPDATE " + StatementGenerator.TableSql(_fromClause, _fromAlias, _formatter) + " SET ";

            var counter = 0;
            foreach(var alias in _selections) 
            {
                var value = _formatter.Format(alias,values[counter]);
                Sql += alias + " = " + value;
            }

            return this;
        }


        /// <summary>
        /// Where the specified expression.
        /// </summary>
        /// <returns>The where.</returns>
        /// <param name="expression">Expression.</param>
        public IUpdateStatement<T> Where(Expression<BooleanExpression<T>> expression)
        {
            WhereClause = new WhereClause<T>(_fromClause, expression);
            Sql += " " + StatementGenerator.WhereSql(WhereClause, _fromAlias, _formatter);
            return this;
        }

        public virtual void Update<T1>(IEnumerable<T1> entities) where T1 : ITable => throw new NotImplementedException();
    }
}
