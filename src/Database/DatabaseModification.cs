using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichoM.Data
{
    public class DatabaseModification<TConnection> : DatabaseCommand where TConnection : DbConnection, new()
    {
        private Database<TConnection> db;

        internal DatabaseModification(Database<TConnection> db, string sql) : base(sql)
        {
            this.db = db;
        }

        public DatabaseModification<TConnection> WithParameter(string name, object value)
        {
            AddParameter(name, value, null);
            return this;
        }

        public DatabaseModification<TConnection> WithParameter(string name, object value, DbType type)
        {
            AddParameter(name, value, type);
            return this;
        }

        public int Execute()
        {
            return db.ExecuteModification(this);
        }
    }
}
