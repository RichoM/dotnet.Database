using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichoM.Data
{
    /// <summary>
    /// The library's entry point. This class represents the database on which you will execute the commands.
    /// </summary>
    /// <typeparam name="TConnection">The specific type of <c>DbConnection</c> to use.</typeparam>
    public class Database<TConnection> : DatabaseContext<TConnection> where TConnection : DbConnection, new()
    {
        private string connectionString;

        /// <summary>
        /// Creates a <c>Database</c> instance using the <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the database.</param>
        public Database(string connectionString)
        {
            this.connectionString = connectionString;
        }
        
        /// <summary>
        /// Allows to group multiple commands in a transaction. If any of the commands fails with an exception
        /// the transaction will be rolled back.
        /// </summary>
        /// <param name="action">A closure whose parameter is the <c>DatabaseTransaction</c> instance
        /// used as context for the commands.</param>
        /// <param name="isolationLevel">Optional. Specifies the isolation level for the transaction.</param>
        public void TransactionDo(Action<DatabaseTransaction<TConnection>> action, IsolationLevel? isolationLevel = null)
        {
            ConnectionDo(conn =>
            {
                DatabaseTransaction<TConnection> transaction = new DatabaseTransaction<TConnection>(conn, isolationLevel);
                transaction.Do(action);
            });
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

        internal override T CommandDo<T>(Func<DbCommand, T> function)
        {
            return ConnectionDo((conn) =>
            {
                using (DbCommand cmd = conn.CreateCommand())
                {
                    return function(cmd);
                }
            });
        }
    }
}
