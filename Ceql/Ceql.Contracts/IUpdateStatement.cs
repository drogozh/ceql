namespace Ceql.Contracts
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IUpdateStatement<T>
    {
        IUpdateStatement<T> Values(params object[] values);
        IUpdateStatement<T> Where(Expression<BooleanExpression<T>> expression);
        void Update<T1>(IEnumerable<T1> entities);
        string Sql { get; }
    }
}
