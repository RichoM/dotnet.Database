using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RichoM.Data;
using System.Data;
using System.Data.SqlServerCe;
using System.Collections.Generic;
using System.Linq;

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
            try { db.NonQuery("DROP TABLE [Test]").Execute(); }
            catch (SqlCeException) { /* The table might not exist. Do nothing */ }
            db.NonQuery("CREATE TABLE [Test] (" + 
                " [id] uniqueidentifier NOT NULL," + 
                " [name] nvarchar(100) NULL," + 
                " [datetime] datetime NULL," + 
                " [number] int NULL)")
                .Execute();
            db.NonQuery("ALTER TABLE [Test]" +
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
                .NonQuery("UPDATE Test SET name = @name, number = @number" +
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

        [TestMethod]
        public void TestCommandParametersWithExplicitDbTypes()
        {
            int rows = db.NonQuery("INSERT INTO Test (id, name) VALUES (@id, @name)")
                .WithParameter("@id", Guid.NewGuid(), DbType.Guid)
                .WithParameter("@name", "Juan", DbType.String)
                .Execute();

            Assert.AreEqual(1, rows);
            string name = db.Query("SELECT name FROM Test").First(row => row.GetString(0));
            Assert.AreEqual("Juan", name);
        }

        [TestMethod]
        public void TestAccessingFieldsByUsingNamesInsteadOfOrdinals()
        {
            string[] expected = new string[] { "Ricardo", "Diego", "Sofía" };
            foreach (string name in expected) { PerformInsert(Guid.NewGuid(), name); }

            IEnumerable<string> names = db.Query("SELECT name FROM Test ORDER BY name ASC")
                .Select(row => row.GetString("name"));

            Assert.IsTrue(expected.OrderBy(each => each).SequenceEqual(names));
        }

        private int PerformInsert(Guid id, string name = null, DateTime? now = null, int? number = null)
        {
            return db.NonQuery("INSERT INTO Test (id, name, datetime, number)" +
                " VALUES (@id, @name, @datetime, @number)")
                .WithParameter("@id", id)
                .WithParameter("@name", name)
                .WithParameter("@datetime", now.HasValue ? (object)now.Value : null)
                .WithParameter("@number", number.HasValue ? (object)number.Value : null)
                .Execute();
        }
    }
}
