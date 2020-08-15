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
        public override void TransactionDo(Action<DatabaseTransaction<TConnection>> action)
        {
            ConnectionDo(conn =>
            {
                var transaction = new DatabaseTransaction<TConnection>(conn, null);
                transaction.Do(action);
            });
        }

        /// <summary>
        /// Allows to group multiple commands in a transaction. If any of the commands fails with an exception
        /// the transaction will be rolled back.
        /// </summary>
        /// <param name="action">A closure whose parameter is the <c>DatabaseTransaction</c> instance
        /// used as context for the commands.</param>
        /// <param name="isolationLevel">Optional. Specifies the isolation level for the transaction.</param>
        public void TransactionDo(Action<DatabaseTransaction<TConnection>> action, IsolationLevel isolationLevel)
        {
            ConnectionDo(conn =>
            {
                var transaction = new DatabaseTransaction<TConnection>(conn, isolationLevel);
                transaction.Do(action);
            });
        }

        /// <summary>
        /// Creates a <c>DbConnection</c> and uses it as argument for the <paramref name="function" />.
        /// The <c>DbConnection</c> is automatically opened before executing the <paramref name="function" />
        /// and closed after it.
        /// </summary>
        /// <typeparam name="T">The return type of the <paramref name="function" /></typeparam>
        /// <param name="function">A function used to perform some actions with the connection</param>
        /// <returns>Returns whatever the <paramref name="function" /> returns</returns>
        public T ConnectionDo<T>(Func<TConnection, T> function)
        {
            using (TConnection conn = new TConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                return function(conn);
            }
        }

        /// <summary>
        /// Creates a <c>DbConnection</c> and uses it as argument for the <paramref name="action" />.
        /// The <c>DbConnection</c> is automatically opened before executing the <paramref name="action" />
        /// and closed after it.
        /// </summary>
        /// <param name="action">An action used to perform some actions with the connection</param>
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
