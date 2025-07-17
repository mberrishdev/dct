using System.Text.Json;
using DCT.Core;

namespace DCT.UnitTests;

public class CodeGeneratorTests
{
    [Fact]
    public void InitConfigAndTemplates_CreatesDctFolderAndFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var dctDir = Path.Combine(tempDir, ".dct");

        try
        {
            // Act
            CodeGenerator.InitConfigAndTemplates(dctDir);

            // Assert
            Assert.True(Directory.Exists(dctDir));
            Assert.True(File.Exists(Path.Combine(dctDir, "dct-config.json")));
            Assert.True(File.Exists(Path.Combine(dctDir, "templates", "command.scriban")));
            Assert.True(File.Exists(Path.Combine(dctDir, "templates", "query.scriban")));
            Assert.True(File.Exists(Path.Combine(dctDir, "templates", "handler.scriban")));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void FindDctDirectory_ReturnsNullIfNotFound()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = typeof(CodeGenerator)
                .GetMethod("FindDctDirectory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.Invoke(null, [tempDir]);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void FindDctDirectory_FindsDctFolder()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var dctDir = Path.Combine(tempDir, ".dct");
        Directory.CreateDirectory(dctDir);

        try
        {
            // Act
            var result = typeof(CodeGenerator)
                .GetMethod("FindDctDirectory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.Invoke(null, [tempDir]) as string;

            // Assert
            Assert.Equal(dctDir, result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void InferNamespace_ReturnsExpectedNamespace()
    {
        // Arrange
        var method = typeof(CodeGenerator)
            .GetMethod("InferNamespace", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var ns = (string)method.Invoke(null, ["Base.Namespace", "Features/Commands", "."]);

        // Assert
        Assert.Equal("Base.Namespace.Features.Commands", ns);
    }

    [Fact]
    public void GetBaseNamespace_ReturnsDefaultIfNoCsproj()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var method = typeof(CodeGenerator)
                .GetMethod("GetBaseNamespace", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var ns = (string)method.Invoke(null, [tempDir]);

            // Assert
            Assert.Equal("Default.Namespace", ns);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetBaseNamespace_ReturnsCsprojName()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csprojPath = Path.Combine(tempDir, "TestProject.csproj");
        File.WriteAllText(csprojPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        try
        {
            var method = typeof(CodeGenerator)
                .GetMethod("GetBaseNamespace", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var ns = (string)method.Invoke(null, [tempDir]);

            // Assert
            Assert.Equal("TestProject", ns);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Generate_ReturnsNullIfTemplateNotFound()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = CodeGenerator.Generate("notfound", "Test", tempDir);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Generate_RendersTemplateWithNamespaceAndName()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var dctDir = Path.Combine(tempDir, ".dct");
        Directory.CreateDirectory(Path.Combine(dctDir, "templates"));
        File.WriteAllText(Path.Combine(dctDir, "dct-config.json"), JsonSerializer.Serialize(new DctConfig
        {
            TemplatePaths = new Dictionary<string, string>
                { { "command", Path.Combine(dctDir, "templates", "command.scriban") } }
        }));
        File.WriteAllText(Path.Combine(dctDir, "templates", "command.scriban"),
            "namespace {{ namespace }};\npublic record {{ name }}Command;");
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            // Act
            Directory.SetCurrentDirectory(tempDir);
            var result = CodeGenerator.Generate("command", "Test", tempDir);

            // Assert
            Assert.Contains("namespace", result);
            Assert.Contains("TestCommand", result);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Directory.Delete(tempDir, true);
        }
    }
}