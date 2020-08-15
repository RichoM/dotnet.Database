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

        public TConnection Connection { get { return connection; } }

        public DbTransaction Transaction { get { return transaction; } }
        public bool Completed { get { return completed; } }

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        public void Commit()
        {
            if (completed) { throw new DatabaseTransactionException(); }

            transaction.Commit();
            completed = true;
        }

        /// <summary>
        /// Rolls back the database transaction.
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
