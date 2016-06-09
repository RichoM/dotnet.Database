using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichoM.Data
{
    public class DatabaseNonQuery<TConnection> : DatabaseCommand where TConnection : DbConnection, new()
    {
        private DatabaseContext<TConnection> context;

        internal DatabaseNonQuery(DatabaseContext<TConnection> context, string sql) : base(sql)
        {
            this.context = context;
        }
        
        public DatabaseNonQuery<TConnection> WithParameter(string name, object value, DbType? type = null)
        {
            AddParameter(name, value, type);
            return this;
        }

        public int Execute()
        {
            return context.ExecuteModification(this);
        }
    }
}
