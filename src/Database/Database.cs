using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichoM.Data
{
    public class Database
    {
        private string connectionString;
        private Func<DbConnection> connectionCreator;

        public Database(string connectionString, Func<DbConnection> connectionCreator)
        {
            this.connectionString = connectionString;
            this.connectionCreator = connectionCreator;
        }
        
        public DatabaseQuery Query(string sql)
        {
            return new DatabaseQuery(this, sql);
        }

        public DatabaseModification Modification(string sql)
        {
            return new DatabaseModification(this, sql);
        }

        public DatabaseModification Insert(string sql)
        {
            return new DatabaseModification(this, sql);
        }

        public DatabaseModification Update(string sql)
        {
            return new DatabaseModification(this, sql);
        }

        public T ConnectionDo<T>(Func<DbConnection, T> function)
        {
            using (DbConnection conn = connectionCreator())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                return function(conn);
            }
        }

        public T CommandDo<T>(Func<DbCommand, T> function)
        {
            return ConnectionDo((conn) => function(conn.CreateCommand()));
        }

        internal T ExecuteQuery<T>(DatabaseQuery query, Func<DbDataReader, T> function)
        {
            return CommandDo((cmd) =>
            {
                query.ConfigureOn(cmd);
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    return function(reader);
                }
            });
        }

        internal int ExecuteModification(DatabaseModification modification)
        {
            return CommandDo((cmd) =>
            {
                modification.ConfigureOn(cmd);
                return cmd.ExecuteNonQuery();
            });
        }
    }
}
