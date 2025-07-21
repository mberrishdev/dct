using Spectre.Console;
using Spectre.Console.Cli;
using DCT.Core;
using System.ComponentModel;

var app = new CommandApp();
app.Configure(config =>
{
    config.SetApplicationName("dct");
    config.ValidateExamples();
    config.PropagateExceptions();
    config.SetApplicationVersion("0.0.6");

    config.AddCommand<CreateCommand>("create")
        .WithDescription("Generate code artifacts");

    config.AddCommand<InitCommand>("init").WithDescription("Initialize config and templates");
});

return app.Run(args);

internal abstract class CreateCommand : Command<CreateCommand.Settings>
{
    public abstract class Settings : CommandSettings
    {
        [CommandArgument(1, "<pathOrName>")] public string PathOrName { get; set; } = string.Empty;

        [CommandArgument(0, "<artifact>")]
        [Description("Type of artifact: command, query, handler, class, interface")]
        public string Artifact { get; set; } = string.Empty;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine(
            $"[green]Generating[/] [blue]{settings.Artifact}[/] at path [yellow]{settings.PathOrName}[/]");

        var fullPath = settings.PathOrName;
        var name = Path.GetFileName(fullPath);
        var outputDir = Path.GetDirectoryName(fullPath);
        outputDir = string.IsNullOrWhiteSpace(outputDir) ? "." : outputDir;

        if (outputDir != ".")
        {
            Directory.CreateDirectory(outputDir);
        }

        var content = CodeGenerator.Generate(
            settings.Artifact,
            name,
            outputDir
        );

        if (content != null)
        {
            var outputPath = Path.Combine(outputDir, $"{name}.cs");
            File.WriteAllText(outputPath, content);
            var absolutePath = Path.GetFullPath(Path.Combine(outputDir, $"{name}.cs"));
            AnsiConsole.MarkupLine($"[green]Generated file at:[/] {absolutePath}");

            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Unknown artifact type or template not found.[/]");
            return 1;
        }
    }
}

internal abstract class InitCommand : Command<InitCommand.Settings>
{
    public abstract class Settings : CommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CodeGenerator.InitConfigAndTemplates();
        return 1;
    }
}