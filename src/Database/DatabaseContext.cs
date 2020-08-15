using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Common;
using System.Data;

namespace RichoM.Data
{
    public abstract class DatabaseContext<TConnection> where TConnection : DbConnection, new()
    {
        /// <summary>
        /// Allows to group multiple commands in a transaction. If any of the commands fails with an exception
        /// the transaction will be rolled back.
        /// </summary>
        /// <param name="action">A closure whose parameter is the <c>DatabaseTransaction</c> instance
        /// used as context for the commands.</param>
        public abstract void TransactionDo(Action<DatabaseTransaction<TConnection>> action);

        internal abstract T CommandDo<T>(Func<DbCommand, T> function);

        /// <summary>
        /// Creates a <c>DatabaseQuery</c>.
        /// </summary>
        /// <param name="commandText">The command text used to configure the query</param>
        /// <returns>The newly created <c>DatabaseQuery</c></returns>
        public DatabaseQuery<TConnection> Query(string commandText, bool storedProcedure = false)
        {
            return new DatabaseQuery<TConnection>(this, commandText, storedProcedure);
        }

        /// <summary>
        /// Creates a <c>DatabaseNonQuery</c>.
        /// </summary>
        /// <param name="commandText">The command text used to configure the command</param>
        /// <returns>The newly created <c>DatabaseNonQuery</c></returns>
        public DatabaseNonQuery<TConnection> NonQuery(string commandText, bool storedProcedure = false)
        {
            return new DatabaseNonQuery<TConnection>(this, commandText, storedProcedure);
        }
        
        internal T ExecuteReader<T>(DatabaseQuery<TConnection> query, Func<DbDataReader, T> function)
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

        internal T ExecuteScalar<T>(DatabaseQuery<TConnection> query)
        {
            return CommandDo((cmd) =>
            {
                query.ConfigureOn(cmd);
                return (T)cmd.ExecuteScalar();
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
