using KF.Jex.LanguageServer.Services;
using Xunit;

namespace KF.Jex.LanguageServer.Tests;

public class FunctionManifestLoaderTests : IDisposable
{
    private readonly string _tempDir;

    public FunctionManifestLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void LoadManifest_ShouldLoadValidManifest()
    {
        var manifestPath = Path.Combine(_tempDir, "test.jex.functions.json");
        var json = @"{
            ""functions"": [
                {
                    ""name"": ""customFunc"",
                    ""signature"": ""customFunc(a, b)"",
                    ""description"": ""A custom function"",
                    ""minArgs"": 2,
                    ""maxArgs"": 2
                }
            ]
        }";
        File.WriteAllText(manifestPath, json);
        
        var loader = new FunctionManifestLoader();
        loader.LoadManifest(manifestPath);
        
        Assert.Single(loader.Functions);
        Assert.Equal("customFunc", loader.Functions[0].Name);
    }

    [Fact]
    public void LoadManifest_ShouldHandleInvalidJson()
    {
        var manifestPath = Path.Combine(_tempDir, "invalid.jex.functions.json");
        File.WriteAllText(manifestPath, "not valid json {{{");
        
        var loader = new FunctionManifestLoader();
        loader.LoadManifest(manifestPath);
        
        Assert.Empty(loader.Functions);
    }

    [Fact]
    public void LoadManifest_ShouldNotLoadSameFileTwice()
    {
        var manifestPath = Path.Combine(_tempDir, "test.jex.functions.json");
        var json = @"{
            ""functions"": [
                { ""name"": ""func1"" }
            ]
        }";
        File.WriteAllText(manifestPath, json);
        
        var loader = new FunctionManifestLoader();
        loader.LoadManifest(manifestPath);
        loader.LoadManifest(manifestPath);
        
        Assert.Single(loader.Functions);
    }

    [Fact]
    public void LoadManifestsFromDirectory_ShouldLoadAllManifests()
    {
        var json1 = @"{ ""functions"": [{ ""name"": ""func1"" }] }";
        var json2 = @"{ ""functions"": [{ ""name"": ""func2"" }] }";
        File.WriteAllText(Path.Combine(_tempDir, "a.jex.functions.json"), json1);
        File.WriteAllText(Path.Combine(_tempDir, "b.jex.functions.json"), json2);
        
        var loader = new FunctionManifestLoader();
        loader.LoadManifestsFromDirectory(_tempDir);
        
        Assert.Equal(2, loader.Functions.Count);
    }

    [Fact]
    public void LoadManifestsFromDirectory_ShouldIgnoreNonMatchingFiles()
    {
        var json = @"{ ""functions"": [{ ""name"": ""func1"" }] }";
        File.WriteAllText(Path.Combine(_tempDir, "other.json"), json);
        
        var loader = new FunctionManifestLoader();
        loader.LoadManifestsFromDirectory(_tempDir);
        
        Assert.Empty(loader.Functions);
    }

    [Fact]
    public void GetFunction_ShouldReturnFunctionByName()
    {
        var manifestPath = Path.Combine(_tempDir, "test.jex.functions.json");
        var json = @"{
            ""functions"": [
                { ""name"": ""myFunc"", ""description"": ""My function"" }
            ]
        }";
        File.WriteAllText(manifestPath, json);
        
        var loader = new FunctionManifestLoader();
        loader.LoadManifest(manifestPath);
        
        var func = loader.GetFunction("myFunc");
        Assert.NotNull(func);
        Assert.Equal("My function", func.Description);
    }

    [Fact]
    public void GetFunction_ShouldBeCaseInsensitive()
    {
        var manifestPath = Path.Combine(_tempDir, "test.jex.functions.json");
        var json = @"{ ""functions"": [{ ""name"": ""MyFunc"" }] }";
        File.WriteAllText(manifestPath, json);
        
        var loader = new FunctionManifestLoader();
        loader.LoadManifest(manifestPath);
        
        var func = loader.GetFunction("MYFUNC");
        Assert.NotNull(func);
    }

    [Fact]
    public void Clear_ShouldRemoveAllFunctions()
    {
        var manifestPath = Path.Combine(_tempDir, "test.jex.functions.json");
        var json = @"{ ""functions"": [{ ""name"": ""func1"" }] }";
        File.WriteAllText(manifestPath, json);
        
        var loader = new FunctionManifestLoader();
        loader.LoadManifest(manifestPath);
        Assert.Single(loader.Functions);
        
        loader.Clear();
        Assert.Empty(loader.Functions);
    }

    [Fact]
    public void LoadManifest_ShouldLoadFunctionParameters()
    {
        var manifestPath = Path.Combine(_tempDir, "test.jex.functions.json");
        var json = @"{
            ""functions"": [{
                ""name"": ""myFunc"",
                ""parameters"": [
                    { ""name"": ""input"", ""type"": ""string"", ""description"": ""The input"" },
                    { ""name"": ""format"", ""type"": ""string"", ""optional"": true }
                ]
            }]
        }";
        File.WriteAllText(manifestPath, json);
        
        var loader = new FunctionManifestLoader();
        loader.LoadManifest(manifestPath);
        
        var func = loader.GetFunction("myFunc");
        Assert.NotNull(func);
        Assert.NotNull(func.Parameters);
        Assert.Equal(2, func.Parameters.Count);
        Assert.Equal("input", func.Parameters[0].Name);
        Assert.True(func.Parameters[1].Optional);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }
}
