using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.Data;
using System.Data.SqlTypes;
using System.Data.Common;

namespace RichoM.Data
{
    public class DatabaseRow
    {
        private DbDataReader reader;

        internal DatabaseRow(DbDataReader reader)
        {
            this.reader = reader;
        }
        
        public int Depth { get { return reader.Depth; } }
        public int FieldCount { get { return reader.FieldCount; } }
        
        public bool GetBoolean(int ordinal) { return reader.GetBoolean(ordinal); }
        public byte GetByte(int ordinal) { return reader.GetByte(ordinal); }
        public char GetChar(int ordinal) { return reader.GetChar(ordinal); }
        public string GetDataTypeName(int index) { return reader.GetDataTypeName(index); }
        public DateTime GetDateTime(int ordinal) { return reader.GetDateTime(ordinal); }
        public decimal GetDecimal(int ordinal) { return reader.GetDecimal(ordinal); }
        public double GetDouble(int ordinal) { return reader.GetDouble(ordinal); }
        public Type GetFieldType(int ordinal) { return reader.GetFieldType(ordinal); }
        public float GetFloat(int ordinal) { return reader.GetFloat(ordinal); }
        public Guid GetGuid(int ordinal) { return reader.GetGuid(ordinal); }
        public short GetInt16(int ordinal) { return reader.GetInt16(ordinal); }
        public int GetInt32(int ordinal) { return reader.GetInt32(ordinal); }
        public long GetInt64(int ordinal) { return reader.GetInt64(ordinal); }
        public string GetName(int index) { return reader.GetName(index); }
        public int GetOrdinal(string name) { return reader.GetOrdinal(name); }
        public Type GetProviderSpecificFieldType(int ordinal) { return reader.GetProviderSpecificFieldType(ordinal); }
        public string GetString(int ordinal) { return reader.GetString(ordinal); }
        public object GetValue(int ordinal) { return reader.GetValue(ordinal); }
        public int GetValues(object[] values) { return reader.GetValues(values); }
        public bool IsDBNull(int ordinal) { return reader.IsDBNull(ordinal); }
    }
}
