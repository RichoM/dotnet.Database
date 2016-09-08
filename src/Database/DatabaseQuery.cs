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
    /// Specific type of <c>DatabaseCommand</c> that is responsible for executing select queries.
    /// </summary>
    /// <typeparam name="TConnection"></typeparam>
    public class DatabaseQuery<TConnection> : DatabaseCommand where TConnection : DbConnection, new()
    {
        private DatabaseContext<TConnection> context;
        
        internal DatabaseQuery(DatabaseContext<TConnection> context, string commandText, bool storedProcedure) 
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
        public DatabaseQuery<TConnection> WithParameter(string name, object value, DbType? type = null)
        {
            AddParameter(name, value, type);
            return this;
        }

        internal T Execute<T>(Func<DbDataReader, T> function)
        {
            return context.ExecuteQuery(this, function);
        }

        /// <summary>
        /// Executes the query and lets you iterate over its results.
        /// </summary>
        /// <param name="action">The action to be performed for each database row</param>
        public void ForEach(Action<DatabaseRow> action)
        {
            Execute((reader) =>
            {
                DatabaseRow row = new DatabaseRow(reader);
                while (reader.Read())
                {
                    action(row);
                }
                return 0;
            });
        }

        /// <summary>
        /// Executes the query and then, for each row, it evaluates <paramref name="function"/> and
        /// collects its results.
        /// </summary>
        /// <typeparam name="T">The return type of <paramref name="function"/>.</typeparam>
        /// <param name="function">The function to evaluate for each row.</param>
        /// <returns>The results after evaluating the <paramref name="function"/> for each row.</returns>
        public List<T> Select<T>(Func<DatabaseRow, T> function)
        {
            return Execute((reader) =>
            {
                DatabaseRow row = new DatabaseRow(reader);
                List<T> result = new List<T>();
                while (reader.Read())
                {
                    result.Add(function(row));
                }
                return result;
            });
        }

        /// <summary>
        /// Executes the query and returns the result of evaluating <paramref name="function"/> using 
        /// the first row as parameter.
        /// </summary>
        /// <typeparam name="T">The return type of <paramref name="function"/>.</typeparam>
        /// <param name="function">The function to evaluate for the first row.</param>
        /// <returns>The result of evaluating <paramref name="function"/>.</returns>
        public T First<T>(Func<DatabaseRow, T> function)
        {
            return Execute(reader => reader.Read() ? function(new DatabaseRow(reader)) : default(T));
        }
    }
}
