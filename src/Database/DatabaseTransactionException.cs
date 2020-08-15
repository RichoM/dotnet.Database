using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    /// <summary>
    /// An exception that is thrown when trying to execute a command inside a 
    /// completed transaction.
    /// </summary>
    public class DatabaseTransactionException : Exception
    {
        public DatabaseTransactionException() : base("The transaction has been completed already")
        {}
    }
}
