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

        /// <summary>
        /// Creates a <c>DatabaseQuery</c>.
        /// </summary>
        /// <param name="commandText">The command text used to configure the query</param>
        /// <returns>The newly created <c>DatabaseQuery</c></returns>
        public DatabaseQuery<TConnection> Query(string commandText)
        {
            return new DatabaseQuery<TConnection>(this, commandText);
        }

        /// <summary>
        /// Creates a <c>DatabaseNonQuery</c>.
        /// </summary>
        /// <param name="commandText">The command text used to configure the command</param>
        /// <returns>The newly created <c>DatabaseNonQuery</c></returns>
        public DatabaseNonQuery<TConnection> NonQuery(string commandText)
        {
            return new DatabaseNonQuery<TConnection>(this, commandText);
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

        internal int ExecuteNonQuery(DatabaseNonQuery<TConnection> modification)
        {
            return CommandDo((cmd) =>
            {
                modification.ConfigureOn(cmd);
                return cmd.ExecuteNonQuery();
            });
        }
    }
}
