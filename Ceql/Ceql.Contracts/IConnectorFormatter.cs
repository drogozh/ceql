using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ceql.Contracts
{
    public interface IConnectorFormatter
    {
        /// <summary>
        /// Formats value for database target
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        object Format(ISelectAlias instance, object source);

        /// <summary>
        /// Formats value for database target
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        object Format(object obj);

        /// <summary>
        /// Formats values read from selected result set
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        
        object FormatFrom(object obj);

        object FormatFrom(Type toType, object obj);

        string TableNameEscape(string schemaName, string tableName);

        string ColumnNameEscape(string columnName);
    }
}
