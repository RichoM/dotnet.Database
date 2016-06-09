using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Common;

namespace RichoM.Data
{
    public abstract class DatabaseContext<TConnection> where TConnection : DbConnection, new()
    {
        internal abstract T CommandDo<T>(Func<DbCommand, T> function);

        public DatabaseQuery<TConnection> Query(string sql)
        {
            return new DatabaseQuery<TConnection>(this, sql);
        }

        public DatabaseNonQuery<TConnection> NonQuery(string sql)
        {
            return new DatabaseNonQuery<TConnection>(this, sql);
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

        internal int ExecuteModification(DatabaseNonQuery<TConnection> modification)
        {
            return CommandDo((cmd) =>
            {
                modification.ConfigureOn(cmd);
                return cmd.ExecuteNonQuery();
            });
        }
    }
}
