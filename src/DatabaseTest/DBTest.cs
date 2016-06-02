﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichoM.Data;
using System.Data.SqlServerCe;
using System.Collections.Generic;

namespace DatabaseTest
{
    [TestClass]
    public class DBTest
    {
        private Database<SqlCeConnection> db;

        [TestInitialize]
        public void Setup()
        {
            db = new Database<SqlCeConnection>(@"Data Source=db.sdf");
            try { db.Modification("DROP TABLE [Test]").Execute(); }
            catch (SqlCeException) { /* The table might not exist. Do nothing */ }
            db.Modification("CREATE TABLE [Test] (" + 
                " [id] uniqueidentifier NOT NULL," + 
                " [name] nvarchar(100) NOT NULL," + 
                " [datetime] datetime NOT NULL," + 
                " [number] int NOT NULL)")
                .Execute();
            db.Modification("ALTER TABLE [Test]" +
                " ADD CONSTRAINT [PK_Test] PRIMARY KEY ([id])")
                .Execute();
        }

        [TestMethod]
        public void TestSQLInsert()
        {
            int rows = PerformInsert(Guid.NewGuid(), "RICHO!", DateTime.Now, 42);
            Assert.AreEqual(1, rows);
        }

        [TestMethod]
        public void TestSQLEmptySelect()
        {
            List<Guid> ids = db.Query("SELECT id FROM Test")
                .Select(row =>
                {
                    Assert.Fail();
                    return row.GetGuid(0);
                });
            Assert.AreEqual(0, ids.Count);
        }

        [TestMethod]
        public void TestSQLSelect()
        {
            Guid id = Guid.NewGuid();
            string name = "Richo!";
            DateTime now = DateTime.Now;
            int number = 42;
            PerformInsert(id, name, now, number);

            List<Tuple<Guid, string, DateTime, int>> rows = db
                .Query("SELECT id, name, datetime, number FROM Test")
                .Select(row => new Tuple<Guid, string, DateTime, int>(
                        row.GetGuid(0),
                        row.GetString(1),
                        row.GetDateTime(2),
                        row.GetInt32(3)));
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual(id, rows[0].Item1);
            Assert.AreEqual(name, rows[0].Item2);
            // Datetime precision is not exactly the same in C# and SQL CE
            Assert.AreEqual(now.ToString(), rows[0].Item3.ToString());
            Assert.AreEqual(number, rows[0].Item4);
        }
        
        [TestMethod]
        public void TestSQLUpdate()
        {
            Guid id = Guid.NewGuid();
            PerformInsert(id, "Richo", DateTime.Now, 42);

            int rows = db
                .Modification("UPDATE Test SET name = @name, number = @number" +
                    " WHERE id = @id")
                .WithParameter("@name", "Ocho")
                .WithParameter("@number", 22)
                .WithParameter("@id", id)
                .Execute();

            Assert.AreEqual(1, rows);
            Tuple<string, int> row = db.Query("SELECT name, number FROM Test WHERE id = @id")
                .WithParameter("@id", id)
                .First(r => new Tuple<string, int>(r.GetString(0), r.GetInt32(1)));
            Assert.AreEqual("Ocho", row.Item1);
            Assert.AreEqual(22, row.Item2);
        }

        private int PerformInsert(Guid id, string name, DateTime now, int number)
        {
            return db.Modification("INSERT INTO Test (id, name, datetime, number)" +
                " VALUES (@id, @name, @datetime, @number)")
                .WithParameter("@id", id)
                .WithParameter("@name", name)
                .WithParameter("@datetime", now)
                .WithParameter("@number", number)
                .Execute();
        }
    }
}
