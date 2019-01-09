using System.Linq.Expressions;
using Ceql.Utils;
using Ceql.Contracts;

namespace Ceql.Composition
{
    public class FromClause<T1, T2, T3, T4, T5> : FromClause
    {
        public FromClause(FromClause parent, Expression<BooleanExpression<T1, T2, T3, T4, T5>> join)
        {
            this.Parent = parent;
            JoinExpression.Add(new ExpressionAggregator()
            {
                Expression = join,
                ExpressionBoundClauses = CeqlUtils.GetStatementList(this)
            });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="select"></param>
        /// <param name="join"></param>
        public FromClause(FromClause parent, SelectClause<T5> select, Expression<BooleanExpression<T1, T2, T3, T4, T5>> join)
            : this(parent, join)
        {
            this.SubSelect = select;
        }

        public WhereClause<T1, T2, T3, T4,T5> Where(Expression<BooleanExpression<T1, T2, T3, T4,T5>> filter)
        {
            return new WhereClause<T1, T2, T3, T4,T5>(this, filter);
        }

        public SelectClause<T1, T2, T3, T4, T5, TResult> Select<TResult>(Expression<SelectExpression<T1, T2, T3, T4, T5, TResult>> select)
        {
            return new SelectClause<T1, T2, T3, T4, T5, TResult>(this, null, select);
        }

    }
}
