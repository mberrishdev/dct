using System.Xml.Linq;
using Scriban;

namespace DCT.Core;

public static class CodeGenerator
{
    public static string Generate(string artifactType, string name, string outputDirectory, string projectRoot = "./")
    {
        var templatePath = artifactType.ToLower() switch
        {
            "command" => "./templates/command.scriban",
            "query" => "./templates/query.scriban",
            "handler" => "./templates/handler.scriban",
            _ => null
        };

        if (templatePath == null || !File.Exists(templatePath))
            return null;

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
