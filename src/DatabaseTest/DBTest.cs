using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RichoM.Data;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Database;

namespace DatabaseTest
{
    [TestClass]
    public class DBTest
    {
        const string connectionString = "Data Source=RICHO-ASUS;Initial Catalog=DbTest;Integrated Security=True";

        private Database<SqlConnection> db;

        [TestInitialize]
        public void Setup()
        {
            db = new Database<SqlConnection>(connectionString);
            try { db.NonQuery("DROP TABLE [Test]").Execute(); }
            catch (SqlException) { /* The table might not exist. Do nothing */ }
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

        // Utility method for inserts
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

        [TestMethod]
        public void TestSQLInsert()
        {
            int rows = PerformInsert(Guid.NewGuid(), "RICHO!", DateTime.Now, 42);
            Assert.AreEqual(1, rows);
        }

        [TestMethod]
        public void TestSQLEmptySelect()
        {
            var ids = db.Query("SELECT id FROM Test")
                .Select(row =>
                {
                    Assert.Fail(); // This code should not be executed
                    return row.GetGuid(0);
                });
            Assert.AreEqual(0, ids.Count());
        }

        [TestMethod]
        public void TestSQLSelect()
        {
            Guid id = Guid.NewGuid();
            string name = "Richo!";
            DateTime now = DateTime.Now;
            int number = 42;
            PerformInsert(id, name, now, number);

            var rows = db
                .Query("SELECT id, name, datetime, number FROM Test")
                .Select(row => new Tuple<Guid, string, DateTime, int>(
                        row.GetGuid(0),
                        row.GetString(1),
                        row.GetDateTime(2),
                        row.GetInt32(3)))
                .ToArray();
            Assert.AreEqual(1, rows.Length);
            Assert.AreEqual(id, rows[0].Item1);
            Assert.AreEqual(name, rows[0].Item2);
            // Datetime precision is not exactly the same in C# and SQL server
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

            var names = db.Query("SELECT name FROM Test ORDER BY name ASC")
                .Select(row => row.GetString("name"));

            Assert.IsTrue(expected.OrderBy(each => each).SequenceEqual(names));
        }

        [TestMethod]
        public void TestTransactions()
        {
            bool exceptionThrown = false; // flag to test if the exception is thrown
            try
            {
                db.TransactionDo(transaction =>
                {
                    Guid id = Guid.NewGuid();
                    int rows = transaction
                        .NonQuery("INSERT INTO Test (id) VALUES (@id)")
                        .WithParameter("@id", id)
                        .Execute();

                    // The table now contains one row
                    Assert.AreEqual(1, rows);
                    Assert.AreEqual(1, transaction.Query("SELECT count(*) FROM Test").First(row => row.GetInt32(0)));
                    // But the database still thinks it's empty because the transaction has
                    // not been committed yet
                    //Assert.AreEqual(0, db.Query("SELECT count(*) FROM Test").First(row => row.GetInt32(0)));

                    // Duplicate id, should fail
                    transaction
                        .NonQuery("INSERT INTO Test (id) VALUES (@id)")
                        .WithParameter("@id", id)
                        .Execute();

                    Assert.Fail("Execution shouldn't reach here");
                });
                Assert.Fail("Execution shouldn't reach here either");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(SqlException));
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "The exception has not been thrown");
            // The table should be empty now
            Assert.AreEqual(0, db.Query("SELECT count(*) FROM Test").First(row => row.GetInt32(0)));
        }

        [TestMethod]
        public void TestScalar()
        {
            int count = 10;
            for (int i = 0; i < count; i++) { PerformInsert(Guid.NewGuid()); }
            Assert.AreEqual(count, db.Query("SELECT count(*) FROM Test").Scalar<int>());
        }

        [TestMethod]
        public void TestRowToDictionary()
        {
            Guid id = Guid.NewGuid();
            string name = "Richo!";
            DateTime now = DateTime.Now;
            int number = 42;
            PerformInsert(id, name, now, number);

            var rows = db
                .Query("SELECT id, name, datetime, number FROM Test")
                .Select(row => row.ToDictionary())
                .ToArray();
            Assert.AreEqual(1, rows.Length);
            Assert.AreEqual(id, rows[0]["id"]);
            Assert.AreEqual(name, rows[0]["name"]);
            // Datetime precision is not exactly the same in C# and SQL server
            Assert.AreEqual(now.ToString(), rows[0]["datetime"].ToString());
            Assert.AreEqual(number, rows[0]["number"]);
        }

        [TestMethod]
        public void TestQueryToArray()
        {
            Guid id = Guid.NewGuid();
            string name = "Richo!";
            DateTime now = DateTime.Now;
            int number = 42;
            PerformInsert(id, name, now, number);

            var rows = db.Query("SELECT id, name, datetime, number FROM Test").ToArray();

            Assert.AreEqual(1, rows.Length);
            Assert.AreEqual(id, rows[0]["id"]);
            Assert.AreEqual(name, rows[0]["name"]);
            // Datetime precision is not exactly the same in C# and SQL server
            Assert.AreEqual(now.ToString(), rows[0]["datetime"].ToString());
            Assert.AreEqual(number, rows[0]["number"]);
        }


        [TestMethod]
        public void TestRowToValueTuple()
        {
            Guid id = Guid.NewGuid();
            string name = "Richo!";
            DateTime now = DateTime.Now;
            int number = 42;
            PerformInsert(id, name, now, number);

            var rows = db
                .Query("SELECT id, name, datetime, number FROM Test")
                .Select(row => (id: row.GetGuid(0),
                                name: row.GetString(1),
                                datetime: row.GetDateTime(2),
                                number: row.GetInt32(3)))
                .ToArray();

            Assert.AreEqual(1, rows.Length);
            Assert.AreEqual(id, rows[0].id);
            Assert.AreEqual(name, rows[0].name);
            // Datetime precision is not exactly the same in C# and SQL server
            Assert.AreEqual(now.ToString(), rows[0].datetime.ToString());
            Assert.AreEqual(number, rows[0].number);
        }

        [TestMethod]
        public void TestRowToAnonObject()
        {
            Guid id = Guid.NewGuid();
            string name = "Richo!";
            DateTime now = DateTime.Now;
            int number = 42;
            PerformInsert(id, name, now, number);

            var rows = db
                .Query("SELECT id, name, datetime, number FROM Test")
                .Select(row => new
                {
                    id = row.GetGuid(0),
                    name = row.GetString(1),
                    datetime = row.GetDateTime(2),
                    number = row.GetInt32(3)
                })
                .ToArray();

            Assert.AreEqual(1, rows.Length);
            Assert.AreEqual(id, rows[0].id);
            Assert.AreEqual(name, rows[0].name);
            // Datetime precision is not exactly the same in C# and SQL server
            Assert.AreEqual(now.ToString(), rows[0].datetime.ToString());
            Assert.AreEqual(number, rows[0].number);
        }

        [TestMethod]
        public void TestNestedTransactionsAreNotSupportedInADONET()
        {
            PerformInsert(Guid.NewGuid(), "Ricardo");
            PerformInsert(Guid.NewGuid(), "Diego");
            PerformInsert(Guid.NewGuid(), "Sofía");

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var outerTran = conn.BeginTransaction();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = outerTran;
                    cmd.CommandText = "UPDATE Test SET name = @new_name WHERE name = @old_name";
                    cmd.Parameters.AddWithValue("@old_name", "Ricardo");
                    cmd.Parameters.AddWithValue("@new_name", "Richo");
                    var rows = cmd.ExecuteNonQuery();
                    Assert.AreEqual(1, rows);
                }

                // SqlConnection doesn't support parallel transactions!
                Assert.ThrowsException<InvalidOperationException>(() =>
                {
                    var innerTran = conn.BeginTransaction();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = innerTran;
                        cmd.CommandText = "DELETE FROM Test WHERE name = @name";
                        cmd.Parameters.AddWithValue("@name", "Richo");
                        var rows = cmd.ExecuteNonQuery();
                        Assert.AreEqual(1, rows);
                    }
                    innerTran.Commit();
                });

                outerTran.Commit();
            }
        }

        [TestMethod]
        public void TestNestedTransactions()
        {
            PerformInsert(Guid.NewGuid(), "Ricardo");
            PerformInsert(Guid.NewGuid(), "Diego");
            PerformInsert(Guid.NewGuid(), "Sofía");

            db.TransactionDo(outerTran =>
            {
                var rows = outerTran
                    .NonQuery("UPDATE Test SET name = @new_name WHERE name = @old_name")
                    .WithParameter("@old_name", "Ricardo")
                    .WithParameter("@new_name", "Richo")
                    .Execute();
                Assert.AreEqual(1, rows);

                outerTran.TransactionDo(innerTran =>
                {
                    var rows = innerTran
                        .NonQuery("DELETE FROM Test WHERE name = @name")
                        .WithParameter("@name", "Richo")
                        .Execute();
                    Assert.AreEqual(1, rows);
                });
            });

            Assert.AreEqual(2, db.Query("SELECT count(*) FROM Test").Scalar<int>());
        }

        [TestMethod]
        public void TestNestedTransactionsOuterRollback()
        {
            PerformInsert(Guid.NewGuid(), "Ricardo");
            PerformInsert(Guid.NewGuid(), "Diego");
            PerformInsert(Guid.NewGuid(), "Sofía");
            try
            {
                db.TransactionDo(outerTran =>
                {
                    var rows = outerTran
                        .NonQuery("UPDATE Test SET name = @new_name WHERE name = @old_name")
                        .WithParameter("@old_name", "Ricardo")
                        .WithParameter("@new_name", "Richo")
                        .Execute();
                    Assert.AreEqual(1, rows);

                    outerTran.TransactionDo(innerTran =>
                    {
                        var rows = innerTran
                            .NonQuery("DELETE FROM Test WHERE name = @name")
                            .WithParameter("@name", "Richo")
                            .Execute();
                        Assert.AreEqual(1, rows);
                    });

                    // This should rollback everything
                    throw new Exception("ROLLBACK!");
                });
            }
            catch (Exception ex)
            {
                Assert.AreEqual("ROLLBACK!", ex.Message);
            }

            Assert.AreEqual(3, db.Query("SELECT count(*) FROM Test").Scalar<int>());
        }

        [TestMethod]
        public void TestNestedTransactionsInnerRollback()
        {
            PerformInsert(Guid.NewGuid(), "Ricardo");
            PerformInsert(Guid.NewGuid(), "Diego");
            PerformInsert(Guid.NewGuid(), "Sofía");
            try
            {
                db.TransactionDo(outerTran =>
                {
                    var rows = outerTran
                        .NonQuery("UPDATE Test SET name = @new_name WHERE name = @old_name")
                        .WithParameter("@old_name", "Ricardo")
                        .WithParameter("@new_name", "Richo")
                        .Execute();
                    Assert.AreEqual(1, rows);

                    outerTran.TransactionDo(innerTran =>
                    {
                        var rows = innerTran
                            .NonQuery("DELETE FROM Test WHERE name = @name")
                            .WithParameter("@name", "Richo")
                            .Execute();
                        Assert.AreEqual(1, rows);

                        // This should rollback everything
                        throw new Exception("ROLLBACK!");
                    });
                });
            }
            catch (Exception ex)
            {
                Assert.AreEqual("ROLLBACK!", ex.Message);
            }

            Assert.AreEqual(3, db.Query("SELECT count(*) FROM Test").Scalar<int>());
        }

        [TestMethod]
        public void TestNestedTransactionsOuterExplicitCommit()
        {
            PerformInsert(Guid.NewGuid(), "Ricardo");
            PerformInsert(Guid.NewGuid(), "Diego");
            PerformInsert(Guid.NewGuid(), "Sofía");

            db.TransactionDo(outerTran =>
            {
                var rows = outerTran
                    .NonQuery("UPDATE Test SET name = @new_name WHERE name = @old_name")
                    .WithParameter("@old_name", "Ricardo")
                    .WithParameter("@new_name", "Richo")
                    .Execute();
                Assert.AreEqual(1, rows);

                outerTran.TransactionDo(innerTran =>
                {
                    var rows = innerTran
                        .NonQuery("DELETE FROM Test WHERE name = @name")
                        .WithParameter("@name", "Richo")
                        .Execute();
                    Assert.AreEqual(1, rows);
                });

                outerTran.Commit();
            });

            Assert.AreEqual(2, db.Query("SELECT count(*) FROM Test").Scalar<int>());
        }

        [TestMethod]
        public void TestNestedTransactionsInnerExplicitCommit()
        {
            PerformInsert(Guid.NewGuid(), "Ricardo");
            PerformInsert(Guid.NewGuid(), "Diego");
            PerformInsert(Guid.NewGuid(), "Sofía");

            db.TransactionDo(outerTran =>
            {
                var rows = outerTran
                    .NonQuery("UPDATE Test SET name = @new_name WHERE name = @old_name")
                    .WithParameter("@old_name", "Ricardo")
                    .WithParameter("@new_name", "Richo")
                    .Execute();
                Assert.AreEqual(1, rows);

                outerTran.TransactionDo(innerTran =>
                {
                    var rows = innerTran
                        .NonQuery("DELETE FROM Test WHERE name = @name")
                        .WithParameter("@name", "Richo")
                        .Execute();
                    Assert.AreEqual(1, rows);
                    
                    innerTran.Commit();
                });

            });

            Assert.AreEqual(2, db.Query("SELECT count(*) FROM Test").Scalar<int>());
        }

        [TestMethod]
        public void TestNestedTransactionsOuterExplicitRollback()
        {
            PerformInsert(Guid.NewGuid(), "Ricardo");
            PerformInsert(Guid.NewGuid(), "Diego");
            PerformInsert(Guid.NewGuid(), "Sofía");

            db.TransactionDo(outerTran =>
            {
                var rows = outerTran
                    .NonQuery("UPDATE Test SET name = @new_name WHERE name = @old_name")
                    .WithParameter("@old_name", "Ricardo")
                    .WithParameter("@new_name", "Richo")
                    .Execute();
                Assert.AreEqual(1, rows);

                outerTran.TransactionDo(innerTran =>
                {
                    var rows = innerTran
                        .NonQuery("DELETE FROM Test WHERE name = @name")
                        .WithParameter("@name", "Richo")
                        .Execute();
                    Assert.AreEqual(1, rows);
                });

                outerTran.Rollback();
            });

            Assert.AreEqual(3, db.Query("SELECT count(*) FROM Test").Scalar<int>());
        }

        [TestMethod]
        public void TestNestedTransactionsInnerExplicitRollback()
        {
            PerformInsert(Guid.NewGuid(), "Ricardo");
            PerformInsert(Guid.NewGuid(), "Diego");
            PerformInsert(Guid.NewGuid(), "Sofía");

            db.TransactionDo(outerTran =>
            {
                var rows = outerTran
                    .NonQuery("UPDATE Test SET name = @new_name WHERE name = @old_name")
                    .WithParameter("@old_name", "Ricardo")
                    .WithParameter("@new_name", "Richo")
                    .Execute();
                Assert.AreEqual(1, rows);

                outerTran.TransactionDo(innerTran =>
                {
                    var rows = innerTran
                        .NonQuery("DELETE FROM Test WHERE name = @name")
                        .WithParameter("@name", "Richo")
                        .Execute();
                    Assert.AreEqual(1, rows);

                    innerTran.Rollback();
                });

            });

            Assert.AreEqual(3, db.Query("SELECT count(*) FROM Test").Scalar<int>());
        }

        [TestMethod]
        public void TestNestedTransactionsCompletedException()
        {
            PerformInsert(Guid.NewGuid(), "Ricardo");
            PerformInsert(Guid.NewGuid(), "Diego");
            PerformInsert(Guid.NewGuid(), "Sofía");

            try
            {
                db.TransactionDo(outerTran =>
                {
                    var rows = outerTran
                        .NonQuery("UPDATE Test SET name = @new_name WHERE name = @old_name")
                        .WithParameter("@old_name", "Ricardo")
                        .WithParameter("@new_name", "Richo")
                        .Execute();
                    Assert.AreEqual(1, rows);

                    outerTran.TransactionDo(innerTran =>
                    {
                        var rows = innerTran
                            .NonQuery("DELETE FROM Test WHERE name = @name")
                            .WithParameter("@name", "Richo")
                            .Execute();
                        Assert.AreEqual(1, rows);

                        innerTran.Commit();
                    });

                    // The transaction has been commited, this should fail
                    Assert.AreEqual(2, outerTran.Query("SELECT count(*) FROM Test").Scalar<int>());
                    Assert.Fail("Execution should not reach here");
                });
                Assert.Fail("Execution should not reach here");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(DatabaseTransactionException));
            }

            Assert.AreEqual(2, db.Query("SELECT count(*) FROM Test").Scalar<int>());
        }


        [TestMethod]
        public void TestNestedTransactionsRollbackAfterCommit()
        {
            PerformInsert(Guid.NewGuid(), "Ricardo");
            PerformInsert(Guid.NewGuid(), "Diego");
            PerformInsert(Guid.NewGuid(), "Sofía");

            try
            {
                db.TransactionDo(outerTran =>
                {
                    var rows = outerTran
                        .NonQuery("UPDATE Test SET name = @new_name WHERE name = @old_name")
                        .WithParameter("@old_name", "Ricardo")
                        .WithParameter("@new_name", "Richo")
                        .Execute();
                    Assert.AreEqual(1, rows);

                    outerTran.TransactionDo(innerTran =>
                    {
                        var rows = innerTran
                            .NonQuery("DELETE FROM Test WHERE name = @name")
                            .WithParameter("@name", "Richo")
                            .Execute();
                        Assert.AreEqual(1, rows);

                        innerTran.Commit();
                    });

                    // The transaction has been commited, this should fail
                    outerTran.Rollback();
                    Assert.Fail("Execution should not reach here");
                });
                Assert.Fail("Execution should not reach here");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(DatabaseTransactionException));
            }

            Assert.AreEqual(2, db.Query("SELECT count(*) FROM Test").Scalar<int>());
        }
    }
}
