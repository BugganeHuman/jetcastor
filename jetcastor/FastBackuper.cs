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
                        BackupSingleItem(backupPath, copiedFilePath, exceptionPaths);
                    }
                }
            }
        }
    }

    private void BackupSingleItem (string backupPath, string copiedFilePath,
                                    List<string> exceptionPaths)
    {
        if (exceptionPaths.Contains(copiedFilePath))
        {
            //Console.WriteLine($"skiped exception from - {copiedFilePath}");
            return;
        }
        string destPath = Path.Combine(backupPath, Path.GetFileName(copiedFilePath));
        bool isDir = File.GetAttributes(copiedFilePath).HasFlag(FileAttributes.Directory);

        if (!File.Exists(destPath) && !isDir || !Directory.Exists(destPath) && isDir)
        {;
            if (!isDir)
            {
                Console.WriteLine($"Creating file from - {copiedFilePath}");
                try
                {
                File.Copy(copiedFilePath, destPath, overwrite: true);
                }
                catch(Exception error)
                {
                    Console.WriteLine(error);
                }
            }
            else if (isDir)
            {
                Console.WriteLine($"Creating dir from - {copiedFilePath}");
                try
                {
                FileSystem.CopyDirectory(copiedFilePath, destPath, overwrite: true);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error);
                }
            }
        }
        else
        {
            if (File.Exists(destPath))
            {
                var infoCopiedFile = new FileInfo(copiedFilePath);
                var infoCopiedFileInBackup = new FileInfo(destPath);
                if (infoCopiedFile.LastWriteTime != infoCopiedFileInBackup.LastWriteTime ||
                    infoCopiedFile.Length != infoCopiedFileInBackup.Length)
                {
                    Console.WriteLine($"Copying file from - {copiedFilePath}");
                    try
                    {
                    File.Copy(copiedFilePath, destPath, overwrite: true);
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error);
                    }
                }
                //копируем файл если изменилась метадата
            }
            else if (Directory.Exists(destPath))
            {
                foreach(var item in Directory.EnumerateFileSystemEntries(copiedFilePath))
                {
                    BackupSingleItem(destPath, item, exceptionPaths);
                    /*
                    var itemDestPath = Path.Combine(destPath, Path.GetFileName(item));
                    if (!File.Exists(itemDestPath))
                    {
                        File.Copy(item, itemDestPath);
                    }
                    else if (!Directory.Exists(itemDestPath))
                    {
                        FileSystem.CopyDirectory(item, itemDestPath);
                    }
                    */
                }
                /*
                 склеиваем destPath и item.name если такое не сущесвтует - если айтем не сущесвтует = копируем,
                если айтем сущесвтует - если файл чекаем по методате если не совподает копируемб,
                если сущесвтуеющий айтем папка - заходим в папку и там тот же алгоритм
                 */
            }
        }


    }
        
    public void FastBackup()
    {
        RunСoordinator(LoadPaths());
    }
    
    
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