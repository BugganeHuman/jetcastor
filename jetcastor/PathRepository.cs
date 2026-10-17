namespace jetcastor;

using Spectre.Console;
using System.IO;
public enum PathCategory
{
    Backup,
    CopiedFile,
    Exception
}



public static class PathRepository
{
    private static Dictionary<PathCategory, string> PathTableMapper = new()
    {
        [PathCategory.Backup] = "backups",
        [PathCategory.CopiedFile] = "copied_files",
        [PathCategory.Exception] = "exceptions"
    };

    public static void AddPaths(PathCategory pathCategory, List<UserPath> paths)
    {
        using var connection = DbContext.OpenConnection();
        using var command = connection.CreateCommand();
        string table = PathTableMapper[pathCategory];
        command.CommandText = $"""
            INSERT INTO {table} (path) VALUES ($path)
            """;
        foreach (UserPath path in paths)
        {
            if (File.Exists(path.StringPath) || Directory.Exists(path.StringPath)){
                if (pathCategory is PathCategory.Backup && !Directory.Exists(path.StringPath))
                { continue; } // у бэкапов могут быть только папки

                command.Parameters.AddWithValue("$path", path.StringPath);
                command.ExecuteNonQuery();
                command.Parameters.Clear();
            }
        }
    }

    public static List<PathDTO> GetPaths (PathCategory pathCategory)
    {
        using var connection = DbContext.OpenConnection();
        using var command = connection.CreateCommand();
        string table = PathTableMapper[pathCategory];
        command.CommandText = $"""
            SELECT * FROM {table}
            """;
        using var reader = command.ExecuteReader();
        List<PathDTO> paths = new(16);
        while (reader.Read())
        {
            paths.Add(new PathDTO(reader.GetInt32(0), reader.GetString(1)));
        }
        return paths;
    }

    public static bool UpdatePath(PathCategory pathCategory, int id, string? path)
    {
        if (File.Exists(path) || Directory.Exists(path))
        {
            if (pathCategory is PathCategory.Backup && !Directory.Exists(path))
            {
                return false;
            }
            using var connection = DbContext.OpenConnection();
            using var command = connection.CreateCommand();
            string table = PathTableMapper[pathCategory];
            command.CommandText = $"""
                UPDATE {table} SET path = COALESCE($path, path) WHERE id = $id
                """;

            command.Parameters.AddWithValue("$path", (object?)path ?? DBNull.Value);
            command.Parameters.AddWithValue("$id", (object?)id ?? DBNull.Value);
            int pathsDeleted = command.ExecuteNonQuery();
            bool isUpdate = pathsDeleted > 0;
            return isUpdate;
        }
        else
        {
            return false;
        }
    }

    public static bool DeletePath(PathCategory pathCategory, int id)
    {
        using var connection = DbContext.OpenConnection();
        using var command = connection.CreateCommand();
        string table = PathTableMapper[pathCategory];
        command.CommandText = $"""
            DELETE FROM {table} WHERE id = $id
            """;
        command.Parameters.AddWithValue("$id", id);
        int pathDelted = command.ExecuteNonQuery();
        bool isDeleted = pathDelted > 0;
        return isDeleted;
    }


}
