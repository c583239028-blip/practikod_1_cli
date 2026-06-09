using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

var allLanguages = new List<string> { ".java", ".js", ".css", ".html", ".py", ".c", ".cpp", ".cs", ".rb", ".php", ".go", ".swift", ".kt", ".ts", ".r", ".sql" };
var outputoption = new Option<FileInfo>("--output", "File path and name");
outputoption.AddAlias("--o");
var removeEmptyLinesOption = new Option<string>("--remove_empty_lines", "Whether to delete empty rows");
removeEmptyLinesOption.AddAlias("--rlp");
var noteOption = new Option<string>("--note", "Whether to list the source code as a comment in the bundle file");
noteOption.AddAlias("--n");
var sortOption = new Option<string>("--sort", "Order of copying the code files");
sortOption.AddAlias("--s");
var authorOption = new Option<string>("--author", "Register the name of the file creator.");
authorOption.AddAlias("--a");
var languagesOption = new Option<string>("--languages", "Specify one or more languages")
{
    IsRequired = true
};
languagesOption.AddAlias("--l");

var bundelecommand = new Command("bundel", "");
bundelecommand.AddOption(outputoption);
bundelecommand.AddOption(removeEmptyLinesOption);
bundelecommand.AddOption(noteOption);
bundelecommand.AddOption(sortOption);
bundelecommand.AddOption(authorOption);
bundelecommand.AddOption(languagesOption);

string currentDirectory = Directory.GetCurrentDirectory();
string[] files = Directory.GetFiles(currentDirectory).OrderBy(file => Path.GetFileName(file)).ToArray();
foreach (var file in files)
{
    Console.WriteLine($"File: {Path.GetFileName(file)}");
}

bundelecommand.SetHandler((output, languages, note, sort, removeEmptyLines, author) =>
{
    try
    {

        if (languages[0] == null) 
        {
            Console.WriteLine($"שגיאה: הכנס שפות או all");
            return;
        }
        var languageList = languages.Split(',').Select(lang => lang.Trim()).ToList();

        foreach (var lang in languageList)
        {
            if (!allLanguages.Contains(lang) && lang != "all")
            {
                Console.WriteLine($"שגיאה: השפה '{lang}' אינה נתמכת.");
                return;
            }
        }



        if (note != null && note != "not" && note != "yes" && note != "n" && note != "y")
        {
            Console.WriteLine($"שגיאה: note '{note}' אינה נתמכת. יש להזין 'yes', 'no', 'y', 'n' או 'not'.");
            return;
        }

        if (sort != null && sort != "type" && sort != "ab")
        {
            Console.WriteLine("שגיאה: ערך מיון לא תקין. יש להזין 'type' או 'ab'.");
            return;
        }

        if (removeEmptyLines != "y" && removeEmptyLines != "n" && removeEmptyLines != "yes" && removeEmptyLines != "not")
        {
            Console.WriteLine("שגיאה: ערך להסרת שורות ריקות לא תקין. יש להזין 'y', 'n', 'yes' או 'not'.");
            return;
        }


        if (output == null)
        {
            output = new FileInfo("123.txt");
        }
        using (var fs = File.Create(output.FullName))
        {
        }
        Console.WriteLine("File was created");
        if (author != null)
        {
            File.AppendAllText(output.FullName, author + "\n");
        }
        if (sort == "type")
        {
            files = Directory.GetFiles(currentDirectory).OrderBy(file => Path.GetExtension(file)).ToArray();
        }
        foreach (var file in files)
        {
            string extension = Path.GetExtension(file);
            if ((languages.Contains(extension) || languages.Trim() == "all") && allLanguages.Contains(extension))
            {
                if (note == "yes" || note == "y")
                {
                    string title = Path.GetFileName(file);
                    File.AppendAllText(output.FullName, title + "\n");
                }
                var content = File.ReadAllText(file);
                if (removeEmptyLines == "y"|| removeEmptyLines == "yes")
                {
                    content = string.Join("\n", content.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)));
                }
                File.AppendAllText(output.FullName, content + "\n");
            }
        }
        Console.WriteLine("ההעתקה הושלמה לקובץ ");
    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine($"Directory not found: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }

}, outputoption, languagesOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

var rootcommand = new RootCommand("reert");
rootcommand.AddCommand(bundelecommand);





var createRspCommand = new Command("create-rsp", "Create a response file for the bundel command");
createRspCommand.AddAlias("crc");
createRspCommand.SetHandler(async () =>
{
    Console.Write("Enter output file path: ");
    var output = Console.ReadLine();
    while (string.IsNullOrWhiteSpace(output))
    {
        Console.WriteLine("שגיאה: יש להזין נתיב תקני.");
        Console.Write("Enter output file path: ");
        output = Console.ReadLine();
    }

    Console.Write("Enter languages (comma-separated): ");
    var languages = Console.ReadLine();

    Console.Write("Enter note (or 'not' to skip): ");
    var note = Console.ReadLine();

    Console.Write("Enter sort option (or leave blank): ");
    var sort = Console.ReadLine();

    Console.Write("Remove empty lines? (y/n): ");
    var removeEmptyLines = Console.ReadLine();

    Console.Write("Enter author name (or leave blank): ");
    var author = Console.ReadLine();

    // בנה את הפקודה המלאה
    var command = $"bundel --output {output} --languages {languages} --note {note} --sort {sort} --remove_empty_lines {removeEmptyLines} --author {author}";

    // שמור את הפקודה בקובץ תגובה
    var responseFileName = "response.rsp"; // או כל שם אחר שתרצה
    await File.WriteAllTextAsync(responseFileName, command);

    Console.WriteLine($"Response file created: {responseFileName}");
});


rootcommand.AddCommand(createRspCommand);

await rootcommand.InvokeAsync(args);