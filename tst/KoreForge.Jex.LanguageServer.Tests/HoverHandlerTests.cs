using KoreForge.Jex.LanguageServer.Handlers;
using KoreForge.Jex.LanguageServer.Services;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Xunit;

namespace KoreForge.Jex.LanguageServer.Tests;

public class HoverHandlerTests
{
    private readonly DocumentManager _documentManager;
    private readonly FunctionManifestLoader _manifestLoader;
    private readonly HoverHandler _handler;

    public HoverHandlerTests()
    {
        _documentManager = new DocumentManager();
        _manifestLoader = new FunctionManifestLoader();
        _handler = new HoverHandler(_documentManager, _manifestLoader);
    }

    [Fact]
    public async Task Handle_ShouldReturnHoverForKeyword()
    {
        var uri = DocumentUri.From("file:///test.jex");
        _documentManager.UpdateDocument(uri, "%let x = 42;");
        
        var request = new HoverParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(0, 2) // On "let" part of %let
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.NotNull(result);
        var content = result.Contents.MarkupContent;
        Assert.NotNull(content);
        Assert.Contains("%let", content.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnHoverForBuiltInVariable()
    {
        var uri = DocumentUri.From("file:///test.jex");
        _documentManager.UpdateDocument(uri, "%let x = $in;");
        
        var request = new HoverParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(0, 10) // On $in
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.NotNull(result);
        var content = result.Contents.MarkupContent;
        Assert.NotNull(content);
        Assert.Contains("$in", content.Value);
        Assert.Contains("Input", content.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnHoverForStandardLibraryFunction()
    {
        var uri = DocumentUri.From("file:///test.jex");
        _documentManager.UpdateDocument(uri, "%let x = trim(y);");
        
        var request = new HoverParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(0, 10) // On "trim"
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.NotNull(result);
        var content = result.Contents.MarkupContent;
        Assert.NotNull(content);
        Assert.Contains("trim", content.Value);
        Assert.Contains("whitespace", content.Value.ToLower());
    }

    [Fact]
    public async Task Handle_ShouldReturnHoverForUserFunction()
    {
        var uri = DocumentUri.From("file:///test.jex");
        var code = @"%func myFunc(a, b);
%return &a + &b;
%endfunc;
%let result = myFunc(1, 2);";
        _documentManager.UpdateDocument(uri, code);
        
        var request = new HoverParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(3, 15) // On "myFunc" in the call
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.NotNull(result);
        var content = result.Contents.MarkupContent;
        Assert.NotNull(content);
        Assert.Contains("myFunc", content.Value);
        Assert.Contains("User Function", content.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnNullForUnknownWord()
    {
        var uri = DocumentUri.From("file:///test.jex");
        _documentManager.UpdateDocument(uri, "unknownIdentifier");
        
        var request = new HoverParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(0, 5)
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Should return null for unknown identifiers
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnNullForUnknownDocument()
    {
        var uri = DocumentUri.From("file:///unknown.jex");
        
        var request = new HoverParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(0, 0)
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnMarkdownContent()
    {
        var uri = DocumentUri.From("file:///test.jex");
        _documentManager.UpdateDocument(uri, "%let x = trim(y);");
        
        var request = new HoverParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(0, 10)
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.NotNull(result);
        var content = result.Contents.MarkupContent;
        Assert.NotNull(content);
        Assert.Equal(MarkupKind.Markdown, content.Kind);
    }
}
