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
    /// Specific type of <c>DatabaseCommand</c> that is responsible for executing inserts, deletes, updates,
    /// and any "non-query" command.
    /// </summary>
    /// <typeparam name="TConnection"></typeparam>
    public class DatabaseNonQuery<TConnection> : DatabaseCommand where TConnection : DbConnection, new()
    {
        private DatabaseContext<TConnection> context;

        internal DatabaseNonQuery(DatabaseContext<TConnection> context, string commandText, bool storedProcedure) 
            : base(commandText, storedProcedure)
        {
            this.context = context;
        }
        
        /// <summary>
        /// Use this method to specify values for your command's parameters.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        /// <param name="type">Optional. Parameter type.</param>
        /// <returns>The current instance so you can keep chaining method calls.</returns>
        public DatabaseNonQuery<TConnection> WithParameter(string name, object value, DbType? type = null)
        {
            AddParameter(name, value, type);
            return this;
        }

        /// <summary>
        /// Executes the command and returns the number of affected rows.
        /// </summary>
        /// <returns>The number of affected rows.</returns>
        public int Execute()
        {
            return context.ExecuteNonQuery(this);
        }
    }
}
