namespace Ceql.Generation
{
    using Ceql.Statements;
    using Ceql.Contracts;
    using Ceql.Model;
    using Ceql.Utils;
    using Ceql.Configuration;
    using Attributes = Ceql.Contracts.Attributes;
    using System.Linq;
    using System.Reflection;
    using Ceql.Composition;

    public class DeleteStatementGenerator
    {
        public static DeleteStatementModel<T> Generate<T>(DeleteStatement<T> statement) where T : ITable
        {
            var formatter = CeqlConfiguration.Instance.GetConnectorFormatter();
            
            var table = TypeHelper.GetType<Attributes.Table>(statement.Type);

            // get primary-key fields only
            var fields = TypeHelper.GetPropertiesForAttribute<Attributes.Field>(table)
                .Where(f => f.GetCustomAttribute<Attributes.PrimaryKey>() != null);

            var tableName = TypeHelper.GetAttribute<Attributes.Table>(table).Name;
            var schemaAtr = TypeHelper.GetAttribute<Attributes.Schema>(table);

            var schemaName = schemaAtr != null ? schemaAtr.Name : null;

            var expressionClauses = HandleExpression(statement?.WhereClause,formatter);

            return new DeleteStatementModel<T>(CeqlConfiguration.Instance.GetConnectorFormatter())
            {
                Fields = fields,
                TableName = tableName,
                SchemaName = schemaName,
                WhereSql = expressionClauses?.WhereSql,
                FromSql = expressionClauses?.FromSql
            };
        }

        private static ExpressionSql HandleExpression<T>(WhereClause<T> statement, IConnectorFormatter formatter)
        {
            if(statement == null) 
            {
                return null;    
            }

            var aliasList = StatementGenerator.GetAliasList(statement.FromClause);
            var fromSql  = StatementGenerator.FromSql(statement.FromClause,aliasList,formatter);
            var whereSql = StatementGenerator.WhereSql(statement,aliasList,formatter);
            
            return new ExpressionSql 
            {
                WhereSql = whereSql,
                FromSql = fromSql
            };
        }
    }

    public class ExpressionSql 
    {
        public string WhereSql {get; set;}
        public string FromSql {get; set;}
    }
}
