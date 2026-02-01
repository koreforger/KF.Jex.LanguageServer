using Khaos.JEX.LanguageServer.Services;
using OmniSharp.Extensions.LanguageServer.Protocol;
using Xunit;

namespace Khaos.JEX.LanguageServer.Tests;

public class DocumentStateTests
{
    [Fact]
    public void Constructor_ShouldParseValidJexCode()
    {
        var uri = DocumentUri.From("file:///test.jex");
        var content = "%let x = 42;";
        
        var state = new DocumentState(uri, content, 1);
        
        Assert.NotNull(state.Ast);
        Assert.Empty(state.ParseErrors);
    }

    [Fact]
    public void Constructor_ShouldCaptureParseErrors()
    {
        var uri = DocumentUri.From("file:///test.jex");
        var content = "%let x = ;"; // Invalid - missing value
        
        var state = new DocumentState(uri, content, 1);
        
        Assert.NotEmpty(state.ParseErrors);
        Assert.Contains(state.ParseErrors, e => e.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void GetUserFunctions_ShouldReturnDeclaredFunctions()
    {
        var uri = DocumentUri.From("file:///test.jex");
        var content = @"%func add(a, b);
%return &a + &b;
%endfunc;
";
        
        var state = new DocumentState(uri, content, 1);
        var functions = state.GetUserFunctions().ToList();
        
        Assert.Single(functions);
        Assert.Contains(functions, f => f.Name == "add" && f.Parameters.Count == 2);
    }

    [Fact]
    public void GetVariablesAtPosition_ShouldReturnVariablesInScope()
    {
        var uri = DocumentUri.From("file:///test.jex");
        var content = @"%let x = 1;
%let y = 2;
%let z = x + y;";
        
        var state = new DocumentState(uri, content, 1);
        var variables = state.GetVariablesAtPosition(3, 10).ToList();
        
        Assert.Contains(variables, v => v.Name == "x");
        Assert.Contains(variables, v => v.Name == "y");
    }

    [Fact]
    public void GetVariablesAtPosition_ShouldNotIncludeFutureVariables()
    {
        var uri = DocumentUri.From("file:///test.jex");
        var content = @"%let x = 1;
%let y = 2;
%let z = 3;";
        
        var state = new DocumentState(uri, content, 1);
        var variables = state.GetVariablesAtPosition(1, 5).ToList();
        
        Assert.DoesNotContain(variables, v => v.Name == "y");
        Assert.DoesNotContain(variables, v => v.Name == "z");
    }

    [Fact]
    public void GetVariablesAtPosition_ShouldIncludeLoopIterator()
    {
        var uri = DocumentUri.From("file:///test.jex");
        var content = @"%let items = [1, 2, 3];
%foreach item %in items %do;
    %let doubled = item * 2;
%end;";
        
        var state = new DocumentState(uri, content, 1);
        // Position inside the loop body (line 3)
        var variables = state.GetVariablesAtPosition(3, 15).ToList();
        
        Assert.Contains(variables, v => v.Name == "items");
        Assert.Contains(variables, v => v.Name == "item");
    }

    [Fact]
    public void GetLine_ShouldReturnCorrectLine()
    {
        var uri = DocumentUri.From("file:///test.jex");
        var content = "line1\nline2\nline3";
        
        var state = new DocumentState(uri, content, 1);
        
        Assert.Equal("line1", state.GetLine(0));
        Assert.Equal("line2", state.GetLine(1));
        Assert.Equal("line3", state.GetLine(2));
    }

    [Fact]
    public void GetLine_ShouldReturnNullForInvalidIndex()
    {
        var uri = DocumentUri.From("file:///test.jex");
        var content = "line1";
        
        var state = new DocumentState(uri, content, 1);
        
        Assert.Null(state.GetLine(-1));
        Assert.Null(state.GetLine(5));
    }
}
