using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.Common;

namespace RichoM.Data
{
    public class DatabaseTransaction<TConnection> : DatabaseContext<TConnection> where TConnection : DbConnection, new()
    {
        private TConnection connection;
        private DbTransaction transaction;

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

        internal override T CommandDo<T>(Func<DbCommand, T> function)
        {
            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                return function(cmd);
            }            
        }

        internal void Do(Action<DatabaseTransaction<TConnection>> action)
        {
            try
            {
                action(this);
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                transaction.Dispose();
            }
        }
    }
}
