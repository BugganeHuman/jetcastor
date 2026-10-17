namespace jetcastor;

using System.IO;
using Microsoft.VisualBasic.FileIO;

public class FastBackuper
{
    private Dictionary<PathCategory, List<string>> LoadPaths()
    {
        Dictionary<PathCategory, string> PathTableMapper = new()
            {
             [PathCategory.Backup] = "backups",
             [PathCategory.CopiedFile] = "copied_files",
             [PathCategory.Exception] = "exceptions"
            };
        List<string> backupPaths = loadPaths(PathCategory.Backup);
        List<string> copiedFilePaths = loadPaths(PathCategory.CopiedFile);
        List<string> exceptionPaths = loadPaths(PathCategory.Exception);
        Dictionary<PathCategory, List<string>> paths = new(3)
        { 
            [PathCategory.Backup] = backupPaths,
            [PathCategory.CopiedFile] = copiedFilePaths,
            [PathCategory.Exception] = exceptionPaths
        };
        return paths;

        List<string> loadPaths(PathCategory pathCategory)
        {
            using var connection = DbContext.OpenConnection();
            using var command = connection.CreateCommand();
            List<string> selectPaths = new(16);
            command.CommandText = $"""
                SELECT path FROM {PathTableMapper[pathCategory]}
                """;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                selectPaths.Add(reader.GetString(0));
            }
            return selectPaths;
        }
     }

    private void RunСoordinator(Dictionary<PathCategory, List<string>> paths)
    {
        List<string> backupPaths = paths[PathCategory.Backup];
        List<string> copiedFilePaths = paths[PathCategory.CopiedFile];
        List<string> exceptionPaths = paths[PathCategory.Exception];
        // возвращает path_to_copied_file и path_to_backup
        foreach (string backupPath in backupPaths)
        {
            if (Directory.Exists(backupPath)) 
            {
                foreach(string copiedFilePath in copiedFilePaths)
                {
                    if (!exceptionPaths.Contains(copiedFilePath))
                    {
                        BackupSingleItem(backupPath, copiedFilePath);
                    }
                }
            }
        }
    }

    private void BackupSingleItem (string backupPath, string copiedFilePath)
    {
        string destPath = Path.Combine(backupPath, Path.GetFileName(copiedFilePath));

        if (!File.Exists(destPath) || !Directory.Exists(destPath))
        {
            if (!File.Exists(destPath))
            {
                File.Copy(copiedFilePath, destPath, overwrite: true);
            }
            else if (!Directory.Exists(destPath))
            {
                FileSystem.CopyDirectory(copiedFilePath, destPath, overwrite: true);
            }
        }
        else
        {
            if (File.Exists(destPath))
            {
                //копируем файл если изменилась метадата
            }
            else if (Directory.Exists(destPath))
            {
                // как ая писал - рекурсивно идем по папке
            }
        }


    }
        
    public void FastBackup() //{}
    
}





/*
 сначала надо залутать 3 листа (через select * from) с путями backups, copired_files, exceptions
 
потом запускается цикл, по поряжку - сначала по backups, if backup_path is dir - 
запускается циклю по copied_files - if copied_file in exceptions = skip, else - 

тут можно написать еще один метод который прнимает path_to_copied_filr и path_to_backup и склеивает в 
final_path- if final_path.not.exists - просто копируем. 
if final_path.exists - if final_path_item is file - копируем если изменились методата;
elif final_path_item is dir рекурсивно проходимя по папке, if  dir_item is dir - заходим в нее
и там же так по аналогии - если папка  - проверяем существует ли на бэкапе, если нет - тупо создаем,
если да - заходим, если файл сравниваем метадату и если поменялось
или не существует на бэкапе - копируем

 */