namespace jetcastor;

using Microsoft.Data.Sqlite;

public static class DbContext
{
    public static SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection("Data Source=jetcastor_db.db");
        connection.Open();
        return connection;
    }

    public static void Start()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS backups (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            path TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS copied_files (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            path TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS exceptions (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            path TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }
}
