namespace jetcastor;



public static class SettingsRepository
{
    public static SettingsDTO GetSettings ()
    {
        using var connection = DbContext.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT * FROM settings
            """;
        try
        {
        using var reader = command.ExecuteReader();
        reader.Read();
        bool isAbortOnLowSpace = reader.GetInt32(0) == 1;
        var settings = new SettingsDTO(isAbortOnLowSpace, reader.GetDouble(1));
        return settings;
        }
        catch
        {
            return new SettingsDTO(true, 10);
        }
    }

    public static bool SetAbortOnLowSpace(bool isEnabled)
    {
        int isEnabledInt = 0;
        if (isEnabled) 
        {
            isEnabledInt = 1; 
        }
        using var connection = DbContext.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE settings SET abort_on_low_space = $abort_on_low_space
            """;
        command.Parameters.AddWithValue("$abort_on_low_space", isEnabledInt);
        try
        {
        bool isUpdated = command.ExecuteNonQuery() > 0;
        return isUpdated;
        }
        catch (Exception error)
        {
            Console.WriteLine(error);
            return false;
        }
    }

    public static bool SetMinFreeSpace (double minFreeSpace)
    {
        using var connection = DbContext.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE settings SET min_free_space = $min_free_space
            """;
        command.Parameters.AddWithValue("$min_free_space", minFreeSpace);
        try
        {
        bool isUpdated = command.ExecuteNonQuery() > 0;
        return isUpdated;
        }
        catch (Exception error)
        {
            Console.WriteLine(error);
            return false;
        }
    }
}


/*
 надо  методы - 
    гет где возворщает словарь, 
    устоновить значение для тру или фолз для abort_on_low_space,
    устоновить дабл значение для min_free_space

 */