using System.Text.Json;
using System.Xml.Linq;
using Scriban;
using Spectre.Console;

namespace DCT.Core;

public static class CodeGenerator
{
    public static string Generate(string artifactType, string name, string outputDirectory, string projectRoot = "./")
    {
        var config = LoadConfig();

        string? templatePath = null;
        if (config?.TemplatePaths != null && config.TemplatePaths.TryGetValue(artifactType.ToLower(), out var path))
        {
            templatePath = path;
        }

        templatePath ??= Path.Combine(FindDctDirectory(Directory.GetCurrentDirectory()) ?? ".", "templates",
            $"{artifactType.ToLower()}.scriban");

        if (!File.Exists(templatePath))
        {
            AnsiConsole.MarkupLine($"[red]Template not found at: {templatePath}[/]");
            return null;
        }

        var @namespace = InferNamespace(GetBaseNamespace(outputDirectory), outputDirectory);

        var templateContent = File.ReadAllText(templatePath);
        var template = Template.Parse(templateContent);

        var result = template.Render(new { name, @namespace });
        return result;
    }

    public static void InitConfigAndTemplates(string targetDirectory = "./.dct")
    {
        AnsiConsole.MarkupLine($"[green]Initializing DCT configuration in:[/] [yellow]{targetDirectory}[/]");

        if (Directory.Exists(targetDirectory))
        {
            AnsiConsole.MarkupLine($"[yellow]Directory already exists:[/] {targetDirectory}");
        }
        else
        {
            Directory.CreateDirectory(targetDirectory);
            AnsiConsole.MarkupLine($"[green]Created directory:[/] {targetDirectory}");
        }

        var configFilePath = Path.Combine(targetDirectory, "dct-config.json");
        if (!File.Exists(configFilePath))
        {
            const string defaultConfig = """
                                         {
                                             "templatePaths": {
                                                 "command": "./.dct/templates/command.scriban",
                                                 "query": "./.dct/templates/query.scriban",
                                                 "handler": "./.dct/templates/handler.scriban"
                                             }
                                         }
                                         """;
            File.WriteAllText(configFilePath, defaultConfig);
        }

        var templatesDir = Path.Combine(targetDirectory, "templates");
        Directory.CreateDirectory(templatesDir);

        File.WriteAllText(Path.Combine(templatesDir, "command.scriban"),
            "namespace {{ namespace }};\n\npublic record {{ name }}Command;");
        File.WriteAllText(Path.Combine(templatesDir, "query.scriban"),
            "namespace {{ namespace }};\n\npublic record {{ name }}Query;");
        File.WriteAllText(Path.Combine(templatesDir, "handler.scriban"),
            "namespace {{ namespace }};\n\npublic class {{ name }}Handler {}\n");

        var gitignorePath = Path.Combine(targetDirectory, ".gitignore");
        const string gitignoreContent = """
                                        *
                                        !templates/
                                        !dct-config.json
                                        dct-config.json
                                        """;

        File.WriteAllText(gitignorePath, gitignoreContent);
    }

    private static DctConfig? LoadConfig()
    {
        var dctDir = FindDctDirectory(Directory.GetCurrentDirectory());
        if (dctDir == null)
        {
            AnsiConsole.MarkupLine("[red].dct folder not found. Please run 'dct init' in your project root.[/]");
            return null;
        }

        var configPath = Path.Combine(dctDir, "dct-config.json");
        if (!File.Exists(configPath))
            return null;

        var json = File.ReadAllText(configPath);
        return JsonSerializer.Deserialize<DctConfig>(json);
    }

    private static string? FindDctDirectory(string startPath)
    {
        var currentDir = new DirectoryInfo(startPath);
        int depth = 0;

        while (currentDir != null && depth < 10)
        {
            var dctPath = Path.Combine(currentDir.FullName, ".dct");
            if (Directory.Exists(dctPath))
            {
                return dctPath;
            }

            currentDir = currentDir.Parent;
            depth++;
        }

        return null;
    }

    private static string InferNamespace(string baseNamespace, string outputDirectory)
    {
        if (!outputDirectory.Contains(baseNamespace))
        {
            return $"{baseNamespace}.{outputDirectory}"
                .Replace(Path.DirectorySeparatorChar, '.')
                .Replace(Path.AltDirectorySeparatorChar, '.')
                .Trim('.');
        }

        var namespaceSuffix =
            outputDirectory[(outputDirectory.IndexOf(baseNamespace, StringComparison.Ordinal) + baseNamespace.Length)..]
                .Replace(Path.DirectorySeparatorChar, '.')
                .Replace(Path.AltDirectorySeparatorChar, '.')
                .Trim('.');

        return string.IsNullOrWhiteSpace(namespaceSuffix)
            ? baseNamespace
            : $"{baseNamespace}.{namespaceSuffix}";
    }

    private static string GetBaseNamespace(string projectRoot)
    {
        var csprojFile = FindClosestCsproj(projectRoot);

        if (csprojFile == null)
            return "Default.Namespace";

        var xml = XDocument.Load(csprojFile);
        var rootNamespace = xml.Descendants("RootNamespace").FirstOrDefault()?.Value;

        if (!string.IsNullOrWhiteSpace(rootNamespace))
            return rootNamespace;

        return Path.GetFileNameWithoutExtension(csprojFile);
    }

    private static string? FindClosestCsproj(string startPath, int maxDepth = 10)
    {
        var currentDir = new DirectoryInfo(startPath);
        int depth = 0;

        AnsiConsole.MarkupLine($"[grey]Starting search for .csproj from: {startPath}[/]");

        while (currentDir != null && depth < maxDepth)
        {
            AnsiConsole.MarkupLine($"[grey]Checking in: {currentDir.FullName} (depth: {depth})[/]");

            var csproj = currentDir.GetFiles("*.csproj").FirstOrDefault();
            if (csproj != null)
            {
                AnsiConsole.MarkupLine($"[green]Found .csproj at: {csproj.FullName}[/]");
                return csproj.FullName;
            }

            currentDir = currentDir.Parent;
            depth++;
        }

        AnsiConsole.MarkupLine($"[red]No .csproj found within depth {maxDepth} starting from {startPath}[/]");
        return null;
    }
}

public class DctConfig
{
    public Dictionary<string, string> TemplatePaths { get; set; } = new();
}