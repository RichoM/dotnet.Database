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
    /// <summary>
    /// This class represents a row in a result set. 
    /// For performance reasons it is implemented as a wrapper around an instance 
    /// of <c>DbDataReader</c>. This means you should never store instances of this
    /// class because they are valid only while the <c>DbDataReader</c> is alive.
    /// </summary>
    public class DatabaseRowReader
    {
        private DbDataReader reader;

        internal DatabaseRowReader(DbDataReader reader)
        {
            this.reader = reader;
        }
        
        public int Depth { get { return reader.Depth; } }
        public int FieldCount { get { return reader.FieldCount; } }

        public string GetName(int index) { return reader.GetName(index); }
        public int GetOrdinal(string name) { return reader.GetOrdinal(name); }
        public int GetValues(object[] values) { return reader.GetValues(values); }

        /// <summary>
        /// Returns a <c>Dictionary</c> that represents the current row.
        /// </summary>
        /// <returns>The <c>Dictionary</c> representing the row.</returns>
        public Dictionary<string, object> ToDictionary()
        {
            return ToDictionary(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a <c>Dictionary</c> that represents the current row allowing to specify the
        /// StringComparer used to create the <c>Dictionary</c>.
        /// </summary>
        /// <returns>The <c>Dictionary</c> representing the row.</returns>
        public Dictionary<string, object> ToDictionary(StringComparer comparer)
        {
            var dictionary = new Dictionary<string, object>(comparer);
            for (int i = 0; i < FieldCount; i++)
            {
                dictionary[GetName(i)] = GetValue(i);
            }
            return dictionary;
        }

        // Fields accessed by ordinal
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
        public Type GetProviderSpecificFieldType(int ordinal) { return reader.GetProviderSpecificFieldType(ordinal); }
        public string GetString(int ordinal) { return reader.GetString(ordinal); }
        public object GetValue(int ordinal) { return reader.GetValue(ordinal); }
        public bool IsDBNull(int ordinal) { return reader.IsDBNull(ordinal); }
        
        // Fields accessed by name
        public bool GetBoolean(string name) { return reader.GetBoolean(GetOrdinal(name)); }
        public byte GetByte(string name) { return reader.GetByte(GetOrdinal(name)); }
        public char GetChar(string name) { return reader.GetChar(GetOrdinal(name)); }
        public string GetDataTypeName(string name) { return reader.GetDataTypeName(GetOrdinal(name)); }
        public DateTime GetDateTime(string name) { return reader.GetDateTime(GetOrdinal(name)); }
        public decimal GetDecimal(string name) { return reader.GetDecimal(GetOrdinal(name)); }
        public double GetDouble(string name) { return reader.GetDouble(GetOrdinal(name)); }
        public Type GetFieldType(string name) { return reader.GetFieldType(GetOrdinal(name)); }
        public float GetFloat(string name) { return reader.GetFloat(GetOrdinal(name)); }
        public Guid GetGuid(string name) { return reader.GetGuid(GetOrdinal(name)); }
        public short GetInt16(string name) { return reader.GetInt16(GetOrdinal(name)); }
        public int GetInt32(string name) { return reader.GetInt32(GetOrdinal(name)); }
        public long GetInt64(string name) { return reader.GetInt64(GetOrdinal(name)); }
        public Type GetProviderSpecificFieldType(string name) { return reader.GetProviderSpecificFieldType(GetOrdinal(name)); }
        public string GetString(string name) { return reader.GetString(GetOrdinal(name)); }
        public object GetValue(string name) { return reader.GetValue(GetOrdinal(name)); }
        public bool IsDBNull(string name) { return reader.IsDBNull(GetOrdinal(name)); }

    }
}
