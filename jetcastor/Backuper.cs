namespace jetcastor;

using Spectre.Console;
using System.IO;

public class Backuper
{
// --------------------------------------------------------------------------------------
    private static Dictionary<PathCategory, List<string>> LoadPaths()
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
// --------------------------------------------------------------------------------------
    private static void RunСoordinator(Dictionary<PathCategory, List<string>> paths)
    {
        List<string> backupPaths = paths[PathCategory.Backup];
        List<string> copiedFilePaths = paths[PathCategory.CopiedFile];
        List<string> exceptionPaths = paths[PathCategory.Exception];
        SettingsDTO settingsData = SettingsRepository.GetSettings();
        foreach (string backupPath in backupPaths)
        {
            if (Directory.Exists(backupPath)) 
            {
                if (settingsData.AbortOnLowSpace && GetDiskFreeSpace(backupPath) >=
                    settingsData.MinFreeSpace || !settingsData.AbortOnLowSpace)
                {
                    foreach(string copiedFilePath in copiedFilePaths)
                    {
                        string relative = Path.GetRelativePath(copiedFilePath, backupPath);
                        if (!Path.IsPathRooted(relative) && relative != ".." && 
                            !relative.StartsWith(".." + Path.DirectorySeparatorChar))
                        {
                            AnsiConsole.MarkupLine($"\n[orange1]{copiedFilePath} " +
                                $"WAS SKIPPED BECAUSE BACKUP {backupPath} " +
                                $"IS INSIDE OF THEM[/]\n");
                            continue;
                        }
                        if (settingsData.AbortOnLowSpace && GetDiskFreeSpace(backupPath) >=
                            settingsData.MinFreeSpace || !settingsData.AbortOnLowSpace)
                        {
                        if (!exceptionPaths.Contains(copiedFilePath))
                        {
                            BackupSingleItem(backupPath, copiedFilePath, exceptionPaths);
                        }
                        }
                        else
                        {
                            AnsiConsole.MarkupLine($"\n[red]Disk {Path.GetPathRoot(backupPath)}" +
                                    $" Have < {settingsData.MinFreeSpace}% free space. STOP[/]\n");
                            break;
                            
                        }
                    }
                }
                else 
                {
                    AnsiConsole.MarkupLine($"\n[red]Disk {Path.GetPathRoot(backupPath)}" +
                            $" Have < {settingsData.MinFreeSpace}% free space. Reject[/]\n"); 
                }
            }
        }
    }
// --------------------------------------------------------------------------------------
    private static void CopyDirectory (string copiedDirPath, string destPath,
                                        List<string> exceptionPaths)
    {
        Directory.CreateDirectory(destPath);
        foreach (string file in Directory.GetFiles(copiedDirPath))
        {
            if (!exceptionPaths.Contains(file))
            {
                string fileName = Path.GetFileName(file);
                try
                {
                File.Copy(file, Path.Combine(destPath, fileName));
                }
                catch
                {
                    AnsiConsole.MarkupLine($"[red]FILE {file} DID NOT COPIED[/]");
                }

            }
        }
        foreach (string dir in Directory.GetDirectories(copiedDirPath))
        {
            if (!exceptionPaths.Contains(dir))
            {
                try
                {
                    var attributes = File.GetAttributes(dir);
                    if (attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        continue;
                    }
                    string destPathOfDir = Path.Combine(destPath, Path.GetFileName(dir));
                    CopyDirectory(dir, destPathOfDir, exceptionPaths);

                }
                catch
                {
                    Console.WriteLine($"dir can't be copying from - {dir}");
                }
            }
        }
    }
// --------------------------------------------------------------------------------------
    private static double GetDiskFreeSpace (string pathOfDisk)
    {
        string? root = Path.GetPathRoot(pathOfDisk);
        if (root is not null)
        {
            var drive = new DriveInfo(root);
            if (drive.IsReady)
            {
                double freePercent = (double)drive.AvailableFreeSpace / drive.TotalSize * 100;
                //Console.WriteLine($"\n\n{root} HAVE {freePercent}%\n\n");
                return freePercent;
            }
            else
            {
                return -1;
            }
        }
        return -1;
    }
// --------------------------------------------------------------------------------------
    private static void BackupSingleItem (string backupPath, string copiedFilePath,
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
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }
            else if (isDir)
            {
                Console.WriteLine($"Creating dir from - {copiedFilePath}");
                CopyDirectory(copiedFilePath, destPath, exceptionPaths);
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
                    catch (UnauthorizedAccessException) { }
                    catch (IOException) { }
                }
                //копируем файл если изменилась метадата
            }
            else if (Directory.Exists(destPath))
            {
                try
                {
                foreach(var item in Directory.EnumerateFileSystemEntries(copiedFilePath))
                {
                    BackupSingleItem(destPath, item, exceptionPaths);
                }
                } 
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }

            }
        }
    }
// --------------------------------------------------------------------------------------
    public void FastBackup()
    {
        RunСoordinator(LoadPaths());
    }
    
    
}



/*
склеиваем destPath и item.name если такое не сущесвтует - если айтем не сущесвтует = копируем,
если айтем сущесвтует - если файл чекаем по методате если не совподает копируемб,
если сущесвтуеющий айтем папка - заходим в папку и там тот же алгоритм
*/


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