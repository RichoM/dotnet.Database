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
        
        /// <summary>
        /// Executes the query and lets you iterate over its results.
        /// </summary>
        /// <param name="action">The action to be performed for each database row</param>
        public void ForEach(Action<DatabaseRowReader> action)
        {
            context.ExecuteReader(this, (reader) =>
            {
                DatabaseRowReader row = new DatabaseRowReader(reader);
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
        public IEnumerable<T> Select<T>(Func<DatabaseRowReader, T> function)
        {
            return context.ExecuteReader(this, (reader) =>
            {
                DatabaseRowReader row = new DatabaseRowReader(reader);
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
        public T First<T>(Func<DatabaseRowReader, T> function)
        {
            return context.ExecuteReader(this, reader => reader.Read() ? function(new DatabaseRowReader(reader)) : default(T));
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result
        /// set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <returns>The first column of the first row in the result set.</returns>
        public T Scalar<T>()
        {
            return context.ExecuteScalar<T>(this);
        }

        /// <summary>
        /// Executes the query and returns each row in the result set as a <c>Dictionary</c>
        /// </summary>
        /// <returns>An array with all the rows in the result set converted to instances of <c>Dictionary</c>.</returns>
        public Dictionary<string, object>[] ToArray()
        {
            return Select(row => row.ToDictionary()).ToArray();
        }
    }
}
