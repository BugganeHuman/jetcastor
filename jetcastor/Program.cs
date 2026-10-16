using jetcastor;
using Spectre.Console;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("\nHi I'm Jetcastor your backuper\n");
DbContext.Start();

Dictionary<string, PathCategory> pathCategoryMapper = new()
{
    ["Copied Files"] = PathCategory.CopiedFile,
    ["Backups"] = PathCategory.Backup,
    ["Exceptions"] = PathCategory.Exception
};

while (true)
{
    string mainMenuChoice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("")
        .HighlightStyle(new Style(Color.SeaGreen1))
        .AddChoices("Fast Backup", "Permament Paths", "Escape")
        );
// --------------------------------------------------------------------------------------
    if (mainMenuChoice == "Escape") { break; }
// --------------------------------------------------------------------------------------
    else if (mainMenuChoice == "Permament Paths")
    {
        while (true)
        {
            string pathCategoryChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("")
                .HighlightStyle(new Style(Color.DeepPink1_1)) //.DarkViolet_1
                .AddChoices("Copied Files", "Backups", "Exceptions", "Back"));
// --------------------------------------------------------------------------------------
            if (pathCategoryChoice == "Back") { break; }
// --------------------------------------------------------------------------------------
            else if (pathCategoryChoice == "Copied Files" ||
                     pathCategoryChoice == "Backups" ||
                     pathCategoryChoice == "Exceptions")
            {
                var pathCategory = pathCategoryMapper[pathCategoryChoice];
                while (true)
                {
                    string pathActionChoice = AnsiConsole.Prompt(
                       new SelectionPrompt<string>()
                       .Title("")
                       .HighlightStyle(new Style(Color.DeepSkyBlue1))
                       .AddChoices("Read All Paths", "Add Paths", "Delete", "Update","Back"));
// --------------------------------------------------------------------------------------
                    if (pathActionChoice == "Back") { break; }
// --------------------------------------------------------------------------------------
                    else if (pathActionChoice == "Add Paths")
                    {
                        List<UserPath> pathsToAdd = [];
                        while (true)
                        {
                            AnsiConsole.Markup("Write path to file or dir," +
                                " or [yellow]0[/] to done: ");
                            string input = Console.ReadLine() ?? "";
                            string path = input.Trim('"').Trim();
                            //Console.WriteLine(path);
                            if (path == "0") { break; }

                            if (File.Exists(path) || Directory.Exists(path))
                            {
                                pathsToAdd.Add(new UserPath(path));
                            }
                            else
                            {
                                AnsiConsole.MarkupLine("\n[orange1]Path is not exists[/]\n");
                            }
                        }
                        PathRepository.AddPaths(pathCategory, pathsToAdd);
                        AnsiConsole.MarkupLine("\n[green]DONE[/]\n");
                    }
// --------------------------------------------------------------------------------------
                    else if (pathActionChoice == "Read All Paths")
                    {
                        List<PathDTO> paths = PathRepository.GetPaths(pathCategory);
                        var table = new Table();
                        table.AddColumns("ID", "Path");
                        table.ShowRowSeparators();
                        foreach (PathDTO path in paths)
                        {
                            table.AddRow(path.Id.ToString(), Markup.Escape(path.PathString));
                        }
                        AnsiConsole.Write(table);
                    }
// --------------------------------------------------------------------------------------
                    else if (pathActionChoice == "Delete")
                    {
                        int id = AnsiConsole.Ask<int>("Write ID of path: ");
                        bool isSure = AnsiConsole.Confirm("Are you sure?", false);
                        if (isSure)
                        {
                            bool isDeleted = PathRepository.DeletePath(pathCategory, id);
                            if (isDeleted)
                            {
                                AnsiConsole.MarkupLine("\n[green]DONE[/]\n");
                            }
                            else 
                            {
                                AnsiConsole.MarkupLine("\n[orange1]DONE, NOTHING DELETED[/]\n"); 
                            }
                        }
                        else 
                        {
                            AnsiConsole.MarkupLine("\n[red]REJECTED[/]\n"); 
                        }
                    }
// --------------------------------------------------------------------------------------
                    else if (pathActionChoice == "Update")
                    {
                        int id = AnsiConsole.Ask<int>("Write ID of path: ");
                        AnsiConsole.Markup("Write updated path," +
                                " or [yellow]0[/] to get back: ");
                        string input = Console.ReadLine() ?? "";
                        string updatedPath = input.Trim('"').Trim();
                        if (updatedPath == "0") { return; }

                        if (File.Exists(updatedPath) || Directory.Exists(updatedPath))
                        {
                            bool isUpdated = PathRepository.UpdatePath(pathCategory, id,
                                                                       updatedPath);
                            if (isUpdated)
                            {
                                AnsiConsole.MarkupLine("\n[green]DONE[/]\n");
                            }
                            else
                            {
                                AnsiConsole.MarkupLine("\n[orange1]DONE, NOTHING UPDATED[/]\n");
                            }

                        }
                        else
                        {
                            AnsiConsole.MarkupLine("\n[orange1]Path is not exists[/]\n");
                        }
                    }
// --------------------------------------------------------------------------------------
                    
                }
            }
        }
    }
}
