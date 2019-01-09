namespace Ceql.Statements
{
    using Ceql.Contracts;
    using Ceql.Generation;
    using Ceql.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;

    public class UpdateStatement<T> : ISqlExpression where T : ITable
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

        public UpdateStatement(Expression<SelectExpression<T>> expression)
        {
            
        }
    }
}
