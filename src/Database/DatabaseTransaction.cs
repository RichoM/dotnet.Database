using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.Common;
using Database;

namespace RichoM.Data
{
    public class DatabaseTransaction<TConnection> : DatabaseContext<TConnection> where TConnection : DbConnection, new()
    {
        private TConnection connection;
        private DbTransaction transaction;
        private bool completed;

        internal DatabaseTransaction(TConnection connection, IsolationLevel? isolationLevel)
        {
            this.connection = connection;

            if (isolationLevel.HasValue)
            {
                transaction = connection.BeginTransaction(isolationLevel.Value);
            }
            else
            {
                transaction = connection.BeginTransaction();
            }
        }

        /// <summary>
        /// <para>Actual <c>DbConnection</c> instance.</para>
        /// 
        /// <para>IMPORTANT: Do not mess with this object if you don't know what you're doing.</para>
        /// </summary>
        public TConnection Connection { get { return connection; } }

        /// <summary>
        /// <para>Actual <c>DbTransaction</c> instance.</para>
        /// 
        /// <para>IMPORTANT: Do not mess with this object if you don't know what you're doing.</para>
        /// </summary>
        public DbTransaction Transaction { get { return transaction; } }

        /// <summary>
        /// A flag that tells you if this transaction has already been commited/rollbacked.
        /// If this flag is set, then attempting to operate with this transaction will throw
        /// a <c>DatabaseTransactionException</c>.
        /// </summary>
        public bool Completed { get { return completed; } }

        /// <summary>
        /// <para>
        /// Commits the database transaction.
        /// </para>
        /// <para>
        /// IMPORTANT: You do not need to call this method explicitly. Once execution leaves
        /// the top-level transaction context this method will be called automatically.
        /// </para>
        /// </summary>
        public void Commit()
        {
            if (completed) { throw new DatabaseTransactionException(); }

            transaction.Commit();
            completed = true;
        }

        /// <summary>
        /// <para>
        /// Rolls back the database transaction.
        /// </para>
        /// <para>
        /// IMPORTANT: You do not need to call this method explicitly. If an exception happens
        /// inside a transaction context, this method will be called automatically.
        /// </para>
        /// </summary>
        public void Rollback()
        {
            if (completed) { throw new DatabaseTransactionException(); }

            transaction.Rollback();
            completed = true;
        }

        internal override T CommandDo<T>(Func<DbCommand, T> function)
        {
            if (completed) { throw new DatabaseTransactionException(); }

            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                return function(cmd);
            }            
        }

        /// <summary>
        /// <para>
        /// Allows to group multiple commands in a nested transaction. If any of the commands fails with an exception
        /// the transaction will be rolled back.
        /// </para>
        /// <para>
        /// IMPORTANT: Please note that, since SqlConnection does not support nested transactions, this implementation
        /// will reuse the same top-level <c>DbTransaction</c> for all nested transactions. The commit will only happen
        /// at the top-level (inner transaction commits will have no effect) and a rollback (at any level) will rollback 
        /// everything. Although we know it's not ideal, this implementation allows us to use the same interface for both
        /// <c>DatabaseTransaction</c> and <c>Database</c>.
        /// </para>
        /// </summary>
        /// <param name="action">A closure whose parameter is the <c>DatabaseTransaction</c> instance
        /// used as context for the commands.</param>
        public override void TransactionDo(Action<DatabaseTransaction<TConnection>> action)
        {
            Do(action, commit: false);
        }

        internal void Do(Action<DatabaseTransaction<TConnection>> action, bool commit = true)
        {
            try
            {
                action(this);
                if (commit && !completed)
                {
                    Commit();
                }
            }
            catch
            {
                if (!completed)
                {
                    Rollback();
                }
                throw;
            }
            finally
            {
                if (commit || completed)
                {
                    transaction.Dispose();
                }
            }
        }
    }
}
