using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichoM.Data
{
    public class Database<TConnection> where TConnection : DbConnection, new()
    {
        private string connectionString;

        public Database(string connectionString)
        {
            this.connectionString = connectionString;
        }
        
        public DatabaseQuery<TConnection> Query(string sql)
        {
            return new DatabaseQuery<TConnection>(this, sql);
        }

        public DatabaseModification<TConnection> Modification(string sql)
        {
            return new DatabaseModification<TConnection>(this, sql);
        }

        public DatabaseModification<TConnection> Insert(string sql)
        {
            return new DatabaseModification<TConnection>(this, sql);
        }

        public DatabaseModification<TConnection> Update(string sql)
        {
            return new DatabaseModification<TConnection>(this, sql);
        }

        public T ConnectionDo<T>(Func<TConnection, T> function)
        {
            using (TConnection conn = new TConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                return function(conn);
            }
        }

        public void ConnectionDo(Action<TConnection> action)
        {
            using (TConnection conn = new TConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                action(conn);
            }
        }

        internal T CommandDo<T>(Func<DbCommand, T> function)
        {
            return ConnectionDo((conn) => function(conn.CreateCommand()));
        }

        internal T ExecuteQuery<T>(DatabaseQuery<TConnection> query, Func<DbDataReader, T> function)
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

        internal int ExecuteModification(DatabaseModification<TConnection> modification)
        {
            return CommandDo((cmd) =>
            {
                modification.ConfigureOn(cmd);
                return cmd.ExecuteNonQuery();
            });
        }
    }
}
