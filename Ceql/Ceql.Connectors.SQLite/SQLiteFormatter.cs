namespace Ceql.Connectors
{
    using Ceql.Contracts;
    using Ceql.Formatters;
    using Ceql.Model;
    using System;
    using System.Reflection;

    public class SQLiteFormatter : BaseFormatter
    {

        public override string TableNameEscape(string schemaName, string tableName)
        {
            return "`" + tableName + "`";
        }

        public override string ColumnNameEscape(string columnName)
        {
            return "`" + columnName + '`';
        }
        
        public override object FormatMethodInfo(ISelectAlias instance, MethodInfo mi)
        {
            if (mi.Name == "ToString") 
            { 
                return new SqlSnippet(instance.ToString());
            }
            
            return base.FormatMethodInfo(instance, mi);
        }

        public override object Format(object obj)
        {

            if (obj == null) {
                return "null";
            }

            if (obj is DateTime)
            {
                var date = ((DateTime)obj).ToUniversalTime();
                return String.Format("'{0:yyyy-MM-dd HH:mm:ss}'", date);
            }

            if (obj is bool)
            {
                return obj.ToString();
            }

            // numbers are number
            if (obj is int || obj is long || obj is decimal || obj is float || obj is byte || obj is sbyte)
            {
                return obj;
            }

            if (obj is string)
            {
                return "'" + obj.ToString().Replace("'", "''").Replace("\\","\\\\") + "'";
            }

            // todo (dr): create InvalidFormatException type
            throw new Exception();
        }

        public override object FormatFrom(object obj)
        {
            if(obj is DateTime)
            {
                return DateTime.SpecifyKind((DateTime)obj, DateTimeKind.Utc);
            }
            return base.FormatFrom(obj);
        }

        public override object FormatFrom(Type toType, object obj)
        {
            if(obj is DateTime)
            {
                return DateTime.SpecifyKind((DateTime)obj, DateTimeKind.Utc);
            }
            return base.FormatFrom(toType,obj);
        }
    }
}
