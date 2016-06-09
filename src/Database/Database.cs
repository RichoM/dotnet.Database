using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichoM.Data
{
    public class Database<TConnection> : DatabaseContext<TConnection> where TConnection : DbConnection, new()
    {
        private string connectionString;

        public Database(string connectionString)
        {
            this.connectionString = connectionString;
        }
        
        public void TransactionDo(Action<DatabaseTransaction<TConnection>> action)
        {
            ConnectionDo(conn =>
            {
                DbTransaction transaction = conn.BeginTransaction();
                try
                {
                    action(new DatabaseTransaction<TConnection>(conn, transaction));
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
