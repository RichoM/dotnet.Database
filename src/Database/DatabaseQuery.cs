using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichoM.Data
{
    public class DatabaseQuery : DatabaseCommand
    {
        private Database db;
        
        internal DatabaseQuery(Database db, string sql) : base(sql)
        {
            this.db = db;
        }        

        public DatabaseQuery WithParameter(string name, object value)
        {
            AddParameter(name, value, null);
            return this;
        }

        internal T Execute<T>(Func<DbDataReader, T> function)
        {
            return db.ExecuteQuery(this, function);
        }

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

        public T First<T>(Func<DatabaseRow, T> function)
        {
            return Execute(reader => reader.Read() ? function(new DatabaseRow(reader)) : default(T));
        }
    }
}
