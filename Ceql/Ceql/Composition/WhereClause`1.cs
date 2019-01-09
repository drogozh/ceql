namespace Ceql.Composition
{
    using Ceql.Contracts;
    using Ceql.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;

    /// <summary>
    /// Where clause for a single table
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WhereClause<T> : WhereClause
    {
        public WhereClause(FromClause<T> fromClause, Expression<BooleanExpression<T>> filter)
        {
            this.FromClause = fromClause;
            this.FilterExpression.Add(
                new ExpressionAggregator()
                {
                    Expression = filter,
                    ExpressionBoundClauses = CeqlUtils.GetStatementList(fromClause)
                }
            );
        }

        public WhereClause And(Expression<BooleanExpression<T>> filter)
        {
            AddExpression(filter,EBooleanOperator.And);
            return this;
        }

        public WhereClause Or(Expression<BooleanExpression<T>> filter)
        {
            AddExpression(filter,EBooleanOperator.Or);
            return this;
        }

        public SelectClause<T, TResult> Select<TResult>(Expression<SelectExpression<T, TResult>> select)
        {
            return new SelectClause<T, TResult>((FromClause<T>)this.FromClause, this, select);
        }

        private void AddExpression(Expression<BooleanExpression<T>> filter, EBooleanOperator op)
        {
            this.FilterExpression.Add(
                new ExpressionAggregator()
                {
                    Expression = filter,
                    Operator = op,
                    ExpressionBoundClauses = CeqlUtils.GetStatementList(this.FromClause)
                }
            );
        }
    }
}
