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

        templatePath ??= Path.Combine(FindDctDirectory(Directory.GetCurrentDirectory()) ?? ".", "templates", $"{artifactType.ToLower()}.scriban");

        if (!File.Exists(templatePath))
        {
            AnsiConsole.MarkupLine($"[red]Template not found at: {templatePath}[/]");
            return null;
        }

        var @namespace = InferNamespace(GetBaseNamespace(projectRoot), outputDirectory, projectRoot);

        var templateContent = File.ReadAllText(templatePath);
        var template = Template.Parse(templateContent);

        var result = template.Render(new { name, @namespace });
        return result;
    }

    public static void InitConfigAndTemplates(string targetDirectory = "./.dct")
    {
        Directory.CreateDirectory(targetDirectory);

        var configFilePath = Path.Combine(targetDirectory, "dct-config.json");
        if (!File.Exists(configFilePath))
        {
            var defaultConfig = """
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

        File.WriteAllText(Path.Combine(templatesDir, "command.scriban"), "namespace {{ namespace }};\n\npublic record {{ name }}Command;");
        File.WriteAllText(Path.Combine(templatesDir, "query.scriban"), "namespace {{ namespace }};\n\npublic record {{ name }}Query;");
        File.WriteAllText(Path.Combine(templatesDir, "handler.scriban"), "namespace {{ namespace }};\n\npublic class {{ name }}Handler {}\n");

        var gitignorePath = Path.Combine(targetDirectory, "../.gitignore");
        var gitignoreEntry = ".dct/\n";
        if (!File.Exists(gitignorePath) || !File.ReadAllText(gitignorePath).Contains(gitignoreEntry))
        {
            File.AppendAllText(gitignorePath, gitignoreEntry);
        }
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
    
    private static string InferNamespace(string baseNamespace, string outputDirectory, string projectRoot)
    {
        var relativePath = Path.GetRelativePath(projectRoot, outputDirectory);

        var namespaceSuffix = relativePath
            .Replace(Path.DirectorySeparatorChar, '.')
            .Replace(Path.AltDirectorySeparatorChar, '.')
            .Trim('.');

        return string.IsNullOrWhiteSpace(namespaceSuffix)
            ? baseNamespace
            : $"{baseNamespace}.{namespaceSuffix}";
    }
    
    private static string GetBaseNamespace(string projectRoot)
    {
        var csprojFile = Directory.GetFiles(projectRoot, "*.csproj").FirstOrDefault();
        if (csprojFile == null)
            return "Default.Namespace";

        var xml = XDocument.Load(csprojFile);
        var rootNamespace = xml.Descendants("RootNamespace").FirstOrDefault()?.Value;

        if (!string.IsNullOrWhiteSpace(rootNamespace))
            return rootNamespace;

        return Path.GetFileNameWithoutExtension(csprojFile);
    }
}

public class DctConfig
{
    public Dictionary<string, string> TemplatePaths { get; set; } = new();
}
