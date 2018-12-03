namespace Ceql.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ceql.Composition;
    using Ceql.Expressions;
    using Ceql.Statements;
    using Ceql.Model;
    using Ceql.Formatters;
    using Ceql.Contracts;
    using Ceql.Configuration;
    using static Ceql.Generation.StatementGenerator;

    public class SelectStatementGenerator
    {
        public static SelectStatementGenerator Instance
        {
            get { return new SelectStatementGenerator(CeqlConfiguration.Instance.GetConnectorFormatter()); }
        }

        IConnectorFormatter _formatter;

        /// <summary>
        /// Private constructor
        /// </summary>
        protected SelectStatementGenerator(IConnectorFormatter formatter)
        {
            _formatter = formatter;
        }

        /// <summary>
        /// Returns SQL statement for the provided SelectClause
        /// </summary>
        /// <param name="selectClause"></param>
        /// <returns></returns>
        public SelectStatementModel GetModel(SelectStatement selectClause)
        {
            var select = selectClause.IsDistinct ? "select distinct " : "select ";
            var from = "";
            var where = "";
            var limit = "";

            //this is a pseudo select
            if (selectClause.FromClause == null)
            {

                var _list = BuildSelectList(selectClause, null).ToList();
                
                return new SelectStatementModel()
                {
                    Sql = select + SelectSql(_list),
                    SelectList = _list
                };
            }

            //generate alias list
            var aliasList = StatementGenerator.GetAliasList(selectClause.FromClause, GetModel);

            //process from clause
            if (selectClause.FromClause != null) from = FromSql(selectClause.FromClause, aliasList, _formatter);
            if (selectClause.WhereClause != null) where = WhereSql(selectClause.WhereClause, aliasList, _formatter);

            var selectList =  BuildSelectList(selectClause, aliasList).ToList();
            
            select = select + SelectSql(selectList);

            var groupby = "";
            if (selectList.Any(a => a.IsGroupRequired == true)) {
                groupby = GroupBySql(selectList.Where(s=>s.IsGroupRequired == false));
            }

            //set limit on result set
            if (selectClause.ResultLimit > 0) limit = "limit " + selectClause.ResultLimit;

            return new SelectStatementModel()
            {
                Sql = select + " " + from + " " + where + " " + groupby + " " +limit,
                AliasList = aliasList,
                SelectList = selectList.ToList(),
                IsAllSelect = selectList.Aggregate(true,(a,b)=>b.IsAllSelect & a)
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        private string SelectSql(IEnumerable<SelectAlias> list )
        {
            return String.Join(",", list.Select(x => x.ToString() + " " + x.Alias));
        }

        private string GroupBySql(IEnumerable<SelectAlias> list) {
            if (!list.Any()) return "";
            return " group by " + String.Join(",", list.Select(x => x.ToString()));
        }

        /// <summary>
        /// Returns collection of the select statement alias
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        private IEnumerable<SelectAlias> BuildSelectList(SelectStatement select, List<FromAlias> aliasList )
        {
            var analyzer = new SelectExpressionAnalyzer(_formatter, select, FilterExpressionAliasList(aliasList, select.ExpressionBoundClauses));
            return (List<SelectAlias>)analyzer.Sql();
        }
    }
}
