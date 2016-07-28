# dotnet.Database

This is just a thin wrapper around ADO.NET.

To start you just have to create a Database object by specifying the connection string and the type of DbConnection you want to use. In this case, we will use the System.Data.SqlClient namespace so we specify the SqlConnection class:

```c#
Database<SqlConnection> db = new Database<SqlConnection>("your connection string");
```

Then, we can use the Database instance to perform queries:

```c#
var rows = db
	.Query("SELECT id, name FROM Test")
	.Select(row => 
	{
		Guid id = row.GetGuid("id");
		string name = row.GetString("name");
		return new Tuple<Guid, string>(id, name);
	});
```

If the query only returns one element, we can access it using the First(..) method instead of Select(..):

```c#
int count = db
	.Query("SELECT count(*) FROM Test")
	.First(row => row.GetInt32(0));
```
	
If we are only interested in iterating over the results, we can use the ForEach(..) method:

```c#
db
	.Query("SELECT id FROM Test")
	.ForEach(row => 
	{
		// Do something with the row...
	});
```
	
If our query has parameters, we can supply arguments by using the WithParameter(..) method:

```c#
IEnumerable<Guid> ids = db
	.Query("SELECT id FROM Test WHERE name = @name")
	.WithParameter("@name", "Juan")
	.Select(row => row.GetGuid(0));
```

We can also use the Database instance to perform inserts, updates, or deletes:

```c#
db
	.NonQuery("INSERT INTO Test (id, name) VALUES (@id, @name)")
	.WithParameter("@id", Guid.NewGuid())
	.WithParameter("@name", "Juan")
	.Execute();
```

Finally, you can also group several commands into a transaction. If any of the commands executed inside the transaction throws an exception, the transaction will be rolled back.

```c#
db.TransactionDo(transaction =>
{
	Guid id = Guid.NewGuid();
	transaction
		.NonQuery("INSERT INTO Test (id) VALUES (@id)")
		.WithParameter("@id", id)
		.Execute();
	
	// Duplicate id, if id is PK this insert should fail
	transaction
		.NonQuery("INSERT INTO Test (id) VALUES (@id)")
		.WithParameter("@id", id)
		.Execute();
});
```
