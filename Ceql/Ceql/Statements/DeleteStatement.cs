namespace Ceql.Statements
{
    using Ceql.Composition;
    using Ceql.Contracts;
    using Ceql.Generation;
    using Ceql.Model;
    using System;

    public class DeleteStatement<T> : ISqlExpression where T : ITable
    {
        public Type Type
        {
            get
            {
                return typeof(T);
            }
        }

        public string Sql
        {
            get
            {
                return Model.Sql;
            }
        }

        public WhereClause<T> WhereClause { get; }

        public DeleteStatementModel<T> Model
        {
            get
            {
                return DeleteStatementGenerator.Generate<T>(this);
            }
        }

        public DeleteStatement(){}

        public DeleteStatement(WhereClause<T> whereClause)
        {
            this.WhereClause = whereClause;
        }
    }
}
