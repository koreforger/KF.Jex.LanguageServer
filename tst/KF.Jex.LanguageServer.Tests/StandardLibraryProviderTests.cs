using KF.Jex.LanguageServer.Services;
using Xunit;

namespace KF.Jex.LanguageServer.Tests;

public class StandardLibraryProviderTests
{
    [Fact]
    public void GetFunctions_ShouldReturnAllFunctions()
    {
        var functions = StandardLibraryProvider.GetFunctions();
        
        Assert.NotEmpty(functions);
        Assert.True(functions.Count > 50, "Expected at least 50 standard library functions");
    }

    [Fact]
    public void GetFunctions_ShouldIncludeJsonPathFunctions()
    {
        var functions = StandardLibraryProvider.GetFunctions();
        
        Assert.Contains(functions, f => f.Name == "jp1");
        Assert.Contains(functions, f => f.Name == "jpAll");
        Assert.Contains(functions, f => f.Name == "existsPath");
    }

    [Fact]
    public void GetFunctions_ShouldIncludeStringFunctions()
    {
        var functions = StandardLibraryProvider.GetFunctions();
        
        Assert.Contains(functions, f => f.Name == "trim");
        Assert.Contains(functions, f => f.Name == "lower");
        Assert.Contains(functions, f => f.Name == "upper");
        Assert.Contains(functions, f => f.Name == "concat");
    }

    [Fact]
    public void GetFunctions_ShouldIncludeMathFunctions()
    {
        var functions = StandardLibraryProvider.GetFunctions();
        
        Assert.Contains(functions, f => f.Name == "abs");
        Assert.Contains(functions, f => f.Name == "round");
        Assert.Contains(functions, f => f.Name == "min");
        Assert.Contains(functions, f => f.Name == "max");
    }

    [Fact]
    public void GetFunction_ShouldReturnFunctionByName()
    {
        var func = StandardLibraryProvider.GetFunction("trim");
        
        Assert.NotNull(func);
        Assert.Equal("trim", func.Name);
        Assert.Equal(1, func.MinArgs);
        Assert.Equal(1, func.MaxArgs);
    }

    [Fact]
    public void GetFunction_ShouldBeCaseInsensitive()
    {
        var func = StandardLibraryProvider.GetFunction("TRIM");
        
        Assert.NotNull(func);
        Assert.Equal("trim", func.Name);
    }

    [Fact]
    public void GetFunction_ShouldReturnNullForUnknownFunction()
    {
        var func = StandardLibraryProvider.GetFunction("unknownFunction");
        
        Assert.Null(func);
    }

    [Theory]
    [InlineData("jp1", 2, 2)]
    [InlineData("concat", 0, int.MaxValue)]
    [InlineData("substring", 2, 3)]
    [InlineData("iif", 3, 3)]
    public void GetFunction_ShouldHaveCorrectArgCounts(string name, int minArgs, int maxArgs)
    {
        var func = StandardLibraryProvider.GetFunction(name);
        
        Assert.NotNull(func);
        Assert.Equal(minArgs, func.MinArgs);
        Assert.Equal(maxArgs, func.MaxArgs);
    }
}
