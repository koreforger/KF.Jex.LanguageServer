using KF.Jex.LanguageServer.Handlers;
using KF.Jex.LanguageServer.Services;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Xunit;

namespace KF.Jex.LanguageServer.Tests;

public class CompletionHandlerTests
{
    private readonly DocumentManager _documentManager;
    private readonly FunctionManifestLoader _manifestLoader;
    private readonly CompletionHandler _handler;

    public CompletionHandlerTests()
    {
        _documentManager = new DocumentManager();
        _manifestLoader = new FunctionManifestLoader();
        _handler = new CompletionHandler(_documentManager, _manifestLoader);
    }

    [Fact]
    public async Task Handle_ShouldReturnKeywords()
    {
        var uri = DocumentUri.From("file:///test.jex");
        _documentManager.UpdateDocument(uri, "");
        
        var request = new CompletionParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(0, 0)
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.Contains(result, c => c.Label == "%let");
        Assert.Contains(result, c => c.Label == "%set");
        Assert.Contains(result, c => c.Label == "%if");
        Assert.Contains(result, c => c.Label == "%foreach");
    }

    [Fact]
    public async Task Handle_ShouldReturnBuiltInVariables()
    {
        var uri = DocumentUri.From("file:///test.jex");
        _documentManager.UpdateDocument(uri, "$");
        
        var request = new CompletionParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(0, 1)
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.Contains(result, c => c.Label == "$in");
        Assert.Contains(result, c => c.Label == "$out");
        Assert.Contains(result, c => c.Label == "$meta");
    }

    [Fact]
    public async Task Handle_ShouldReturnStandardLibraryFunctions()
    {
        var uri = DocumentUri.From("file:///test.jex");
        _documentManager.UpdateDocument(uri, "");
        
        var request = new CompletionParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(0, 0)
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.Contains(result, c => c.Label == "trim");
        Assert.Contains(result, c => c.Label == "jp1");
        Assert.Contains(result, c => c.Label == "concat");
    }

    [Fact]
    public async Task Handle_ShouldFilterByPrefix()
    {
        var uri = DocumentUri.From("file:///test.jex");
        _documentManager.UpdateDocument(uri, "tri");
        
        var request = new CompletionParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(0, 3)
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.Contains(result, c => c.Label == "trim");
        Assert.DoesNotContain(result, c => c.Label == "lower");
    }

    [Fact]
    public async Task Handle_ShouldReturnUserDefinedFunctions()
    {
        var uri = DocumentUri.From("file:///test.jex");
        var content = @"%func myCustomFunc(a, b);
%return &a + &b;
%endfunc;
%let x = 1;";
        _documentManager.UpdateDocument(uri, content);
        
        var request = new CompletionParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(3, 0)
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.Contains(result, c => c.Label == "myCustomFunc");
    }

    [Fact]
    public async Task Handle_ShouldReturnVariablesInScope()
    {
        var uri = DocumentUri.From("file:///test.jex");
        var content = "%let myVar = 42;\n%let result = myVar;";
        _documentManager.UpdateDocument(uri, content);
        
        var request = new CompletionParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(1, 14)
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.Contains(result, c => c.Label == "myVar");
    }

    [Fact]
    public async Task Handle_ShouldReturnBooleanLiterals()
    {
        var uri = DocumentUri.From("file:///test.jex");
        _documentManager.UpdateDocument(uri, "");
        
        var request = new CompletionParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(0, 0)
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.Contains(result, c => c.Label == "true");
        Assert.Contains(result, c => c.Label == "false");
        Assert.Contains(result, c => c.Label == "null");
    }

    [Fact]
    public async Task Handle_ShouldHaveFunctionKindForFunctions()
    {
        var uri = DocumentUri.From("file:///test.jex");
        _documentManager.UpdateDocument(uri, "");
        
        var request = new CompletionParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(0, 0)
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        var trimCompletion = result.FirstOrDefault(c => c.Label == "trim");
        
        Assert.NotNull(trimCompletion);
        Assert.Equal(CompletionItemKind.Function, trimCompletion.Kind);
    }

    [Fact]
    public async Task Handle_ShouldHaveKeywordKindForKeywords()
    {
        var uri = DocumentUri.From("file:///test.jex");
        _documentManager.UpdateDocument(uri, "");
        
        var request = new CompletionParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(0, 0)
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        var letCompletion = result.FirstOrDefault(c => c.Label == "%let");
        
        Assert.NotNull(letCompletion);
        Assert.Equal(CompletionItemKind.Keyword, letCompletion.Kind);
    }
}
