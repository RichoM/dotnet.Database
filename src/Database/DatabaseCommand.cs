using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichoM.Data
{
    public abstract class DatabaseCommand
    {
        private string commandText;
        private Dictionary<string, Tuple<object, DbType?>> parameters;

        protected DatabaseCommand(string commandText)
        {
            this.commandText = commandText;
            parameters = new Dictionary<string, Tuple<object, DbType?>>();
        }

        internal string CommandText { get { return commandText; } }
        internal Dictionary<string, Tuple<object, DbType?>> Parameters { get { return parameters; } }

        protected void AddParameter(string name, object value, DbType? type)
        {
            parameters[name] = new Tuple<object, DbType?>(value, type);
        }

        internal void ConfigureOn(DbCommand cmd)
        {
            cmd.CommandText = commandText;

            foreach (KeyValuePair<string, Tuple<object, DbType?>> kvp in parameters)
            {
                string name = kvp.Key;
                object value = kvp.Value.Item1 ?? DBNull.Value;
                DbType? type = kvp.Value.Item2;
                DbParameter param = cmd.CreateParameter();
                if (type.HasValue) { param.DbType = type.Value; }
                param.ParameterName = name;
                param.Value = value;
                cmd.Parameters.Add(param);
            }
        }
    }
}
