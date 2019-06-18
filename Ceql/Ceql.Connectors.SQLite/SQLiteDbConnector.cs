namespace Ceql.Connectors
{
    using System;
    using System.Reflection;
    using System.Data;
    using Ceql.Contracts;
    using Ceql.Contracts.Configuration;
    using Ceql.Utils;
    using Attributes = Ceql.Contracts.Attributes;
    using System.Linq;
    using System.Data.SQLite;

    public class SQLiteDbConnector : IDataConnector
    {
        public IDbConnection GetDbConnection(IConnectionConfig config)
        {
            var connection = new SQLiteConnection()
            {
                ConnectionString = config.ConnectionString
            };

            return connection;
        }

        public void PreInsert<T>(IDbCommand command, T entity, bool isFull)
        {
            return;
        }


        public void PostInsert<T>(IDbCommand command, T entity, bool isFull)
        {
            // do not retrieve auto id property
            if(isFull) 
            {
                return;
            }

            var type = TypeHelper.GetType<Attributes.Table>(entity.GetType());
            var autoSequence = TypeHelper
                .GetPropertiesForAttribute<Attributes.AutoSequence>(type)
                .FirstOrDefault();

            // no autosequence fields, nothing to do here
            if (autoSequence == null)
            {
                return;
            }
            
            command.CommandText = "SELECT LAST_INSERT_ROWID()";
            using (var reader = command.ExecuteReader())
            {
                if (!reader.Read()) return;

                var value = Convert.ChangeType(reader[0],autoSequence.PropertyType);
                autoSequence.SetValue(entity, value);
            }
        }
    }
}
