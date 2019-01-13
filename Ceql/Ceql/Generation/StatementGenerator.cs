namespace Ceql.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ceql.Composition;
    using Ceql.Expressions;
    using Ceql.Statements;
    using Ceql.Model;
    using Ceql.Contracts;

    public static class StatementGenerator
    {
        /// <summary>
        /// Gets the alias list.
        /// </summary>
        /// <returns>The alias list.</returns>
        /// <param name="from">From.</param>
        /// <param name="subQueryHandler">Sub query handler.</param>
        public static List<FromAlias> GetAliasList(FromClause from, Func<SelectStatement, SelectStatementModel> subQueryHandler = null)
        {
            var joinCount = 0;
            var node = from;
            while (node.Parent != null)
            {
                node = node.Parent;
                ++joinCount;
            }
        
            var list = new List<FromAlias>();

            node = from;
            while (node != null)
            {
                var alias = "T" + (joinCount - list.Count);

                var fa = new FromAlias()
                {
                    FromClause = node,
                    TableType = node.TableType,
                    Name = alias
                };

                if (node.SubSelect != null && subQueryHandler != null)
                {
                    fa.SubGeneratedSelect = subQueryHandler(node.SubSelect);
                }
                
                list.Add(fa);
                
                node = node.Parent;
            }

            list.Reverse();
            return list;
        }


        /// <summary>
        /// Returns sql for the 'FROM' clause of the sql query
        /// </summary>
        /// <returns>The sql.</returns>
        /// <param name="fromClause">From clause.</param>
        /// <param name="list">List.</param>
        /// <param name="formatter">Formatter.</param>
        public static string FromSql(FromClause fromClause, List<FromAlias> list, IConnectorFormatter formatter)
        {
            var sql = "FROM " + BuildJoins(formatter, fromClause, list);
            return sql;
        }


        /// <summary>
        /// Returns sql for the 'WHERE' clause of the sql query 
        /// </summary>
        /// <returns>The sql.</returns>
        /// <param name="whereClause">Where clause.</param>
        /// <param name="aliasList">Alias list.</param>
        /// <param name="formatter">Formatter.</param>
        public static string WhereSql(WhereClause whereClause, List<FromAlias> aliasList, IConnectorFormatter formatter)
        {
            //var whereClause = selectClause.WhereClause;
            var analyzer = new ConditionExpressionAnalyzer(formatter);

            var whereSql = "WHERE ";

            for (var i = 0; i < whereClause.FilterExpression.Count; i++)
            {
                var filter = whereClause.FilterExpression[i];
                if (i >= 1) //dont care about operator if it is a first join expression
                    switch (filter.Operator)
                    {
                        case EBooleanOperator.And:
                            whereSql += " and ";
                            break;
                        case EBooleanOperator.Or:
                            whereSql += " or ";
                            break;
                        case null:
                            break;
                        default:
                            break;
                    }

                whereSql += analyzer.Sql(FilterExpressionAliasList(aliasList,filter.ExpressionBoundClauses), filter.Expression);    
            }

            return whereSql;
        }


        /// <summary>
        /// Filters master alias list to get an ordered alias list only for the ExpressionBoundClauses references in the select or join expression
        /// </summary>
        /// <param name="aliasList"></param>
        /// <param name="clauses">From clauses in the order of the statement, first from clause comes first</param>
        /// <returns></returns>
        public static List<FromAlias> FilterExpressionAliasList(List<FromAlias> aliasList, List<FromClause> clauses)
        {
            if (clauses == null || clauses.Count == 0) return aliasList;

            var faList = new List<FromAlias>();
            for (var i = 0; i < clauses.Count; i++)
            {
                var als = aliasList.FirstOrDefault(a => a.FromClause == clauses[i]);
                faList.Add(als);
            }
            return faList;
        }
    

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="aliasList"></param>
        /// <returns></returns>
        private static string BuildJoins(IConnectorFormatter formatter, FromClause from, List<FromAlias> aliasList )
        {
            var alias = aliasList.First(x => x.FromClause == from);

            var joinType = " join ";
            switch (from.JoinType)
            {
                case EJoinType.Inner:
                    joinType = " join ";
                    break;
                case EJoinType.Left:
                    joinType = " left join ";
                    break;
                case EJoinType.Right:
                    joinType = " right join ";
                    break;
                default:
                    joinType = " join ";
                    break;
            }
            
            //joined tables
            if (from.Parent != null)
            {
                var analyzer = new ConditionExpressionAnalyzer(formatter);
                var statement = BuildJoins(formatter, from.Parent, aliasList) + joinType + TableSql(from, aliasList, formatter) + " on ";

                var joinCondition = "";
               for(var i=0; i<from.JoinExpression.Count; i++)
                {
                    var j = from.JoinExpression[i];
                    var faList = FilterExpressionAliasList(aliasList, j.ExpressionBoundClauses);
                    
                    if(i >= 1) //dont care about operator if it is a first join expression
                    switch (j.Operator)
                    {
                        case EBooleanOperator.And:
                            joinCondition += " and ";
                            break;
                        case EBooleanOperator.Or:
                            joinCondition += " or ";
                            break;
                        case null:
                            break;
                        default:
                            break;
                    }

                    joinCondition += analyzer.Sql(faList, j.Expression);    
                }
                
                //append join condition
                statement += joinCondition;


                return statement;
            }
            
            //driver tables
            return TableSql(from, aliasList, formatter);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static string TableSql(FromClause from, List<FromAlias> aliases, IConnectorFormatter formatter)
        {
            //get alias for this clause
            var alias = aliases.First(x => x.FromClause == from);

            if (from.SubSelect != null)
            {
                return "(" + alias.SubGeneratedSelect.Sql + ") " + alias.Name;
            }

            return formatter.TableNameEscape(from.SchemaName,from.TableName) + " " + alias.Name;
        }
    }
}