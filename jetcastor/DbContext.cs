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

            CREATE TABLE IF NOT EXISTS settings (
            abort_on_low_space INTEGER NOT NULL DEFAULT 1 CHECK (abort_on_low_space IN (0, 1)),
            min_free_space REAL NOT NULL DEFAULT 10 CHECK (min_free_space BETWEEN 0 AND 100)
            );
            
            INSERT INTO settings (abort_on_low_space, min_free_space) 
            SELECT 1, 10 
            WHERE NOT EXISTS (SELECT 1 FROM settings);


            """;
        command.ExecuteNonQuery();
    }
}
