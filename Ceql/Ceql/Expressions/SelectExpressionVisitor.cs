using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Ceql.Model;
using Ceql.Utils;

namespace Ceql.Expressions
{
    public class SelectExpressionVisitor: ExpressionVisitor
    {

        private readonly List<ParameterExpression> _parameters;
        private readonly List<FromAlias> _aliasList;

        public SelectExpressionVisitor(List<ParameterExpression> parameters, List<FromAlias> aliasList)
        {
            this._parameters = parameters;
            this._aliasList = aliasList;
        }


        protected override Expression VisitMember(MemberExpression node)
        {
            var property = node.Member as PropertyInfo;
            var fieldName = TypeHelper.GetFieldName(property);

            return base.VisitMember(node);
        }
    }
}
