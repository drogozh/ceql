﻿namespace Ceql.Composition
{
    using System.Linq.Expressions;
    using Ceql.Contracts;

    public class SelectClause<T1, T2, T3, T4, T5, TResult> 
        : SelectClause<TResult>
    {

        public SelectClause(
            FromClause<T1, T2, T3, T4, T5> from, 
            WhereClause<T1, T2, T3, T4, T5> where, 
            Expression<SelectExpression<T1, T2, T3, T4, T5, TResult>> select)
        {
            this.FromClause = from;
            this.WhereClause = where;
            this.SelectExpression = select;
            this.ResultType = typeof(TResult);
        }
    }
}
