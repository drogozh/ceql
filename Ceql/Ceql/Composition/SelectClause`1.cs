namespace Ceql.Composition
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Ceql.Configuration;
    using Ceql.Contracts;
    using Ceql.Execution;
    using Ceql.Statements;

    public class SelectClause<T> : SelectStatement, IEnumerable<T>
    {

        public SelectClause()
        {
        }

        public SelectClause(Expression<SelectExpression<T>> select)
        {
            this.SelectExpression = select;
        }

        public SelectClause<T> Unique()
        {
            this.IsDistinct = true;
            return this;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SelectEnumerator<T>(this,CeqlConfiguration.Instance.GetConnectorFormatter());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
