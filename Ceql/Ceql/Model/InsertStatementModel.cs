using Ceql.Contracts;
using Ceql.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ceql.Model
{
    public class InsertStatementModel<T> : StatementModel<T>
    {
        public bool IsFull {get; set;}

        public IEnumerable<PropertyInfo> AutoFields { get; set; }

        public InsertStatementModel(IConnectorFormatter formatter) : base(formatter)
        { }

        private object lck = new object();
        private string _sql;

        protected override string GetSql()
        {
            if (_sql != null)
            {
                return _sql;
            }

            lock (lck)
            {
                if (_sql != null)
                {
                    return _sql;
                }

                var count = 0;
                _sql = String.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                    Formatter.TableNameEscape(SchemaName, TableName),
                    String.Join(",", Fields.Select(f => Formatter.ColumnNameEscape(f.GetCustomAttribute<Contracts.Attributes.Field>().Name))),
                    String.Join(",", Fields.Select(f => "@p" + count++)));

                return _sql;
            }
        }

        public string GetSql(T entity)
        {
            List<PropertyInfo> fields = new List<PropertyInfo>();
            
            fields.AddRange(AutoFields.Where(field => !TypeHelper.IsDefaultValue(field.GetValue(entity))));
            fields.AddRange(Fields);

            var sql = String.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                Formatter.TableNameEscape(SchemaName, TableName),
                String.Join(",", fields.Select(f => Formatter.ColumnNameEscape(f.GetCustomAttribute<Contracts.Attributes.Field>().Name))),
                String.Join(",", fields.Select(f => Formatter.Format(f.GetValue(entity)).ToString())));
            
            return sql;           
        }
    }
}
