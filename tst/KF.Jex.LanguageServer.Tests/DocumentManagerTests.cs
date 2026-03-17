using KF.Jex.LanguageServer.Services;
using OmniSharp.Extensions.LanguageServer.Protocol;
using Xunit;

namespace KF.Jex.LanguageServer.Tests;

public class DocumentManagerTests
{
    [Fact]
    public void UpdateDocument_ShouldStoreDocument()
    {
        var manager = new DocumentManager();
        var uri = DocumentUri.From("file:///test.jex");
        
        var state = manager.UpdateDocument(uri, "%let x = 42;", 1);
        
        Assert.NotNull(state);
        Assert.Equal(uri, state.Uri);
        Assert.Equal("%let x = 42;", state.Content);
        Assert.Equal(1, state.Version);
    }

    [Fact]
    public void GetDocument_ShouldReturnStoredDocument()
    {
        var manager = new DocumentManager();
        var uri = DocumentUri.From("file:///test.jex");
        manager.UpdateDocument(uri, "%let x = 42;");
        
        var state = manager.GetDocument(uri);
        
        Assert.NotNull(state);
        Assert.Equal("%let x = 42;", state.Content);
    }

    [Fact]
    public void GetDocument_ShouldReturnNullForUnknownDocument()
    {
        var manager = new DocumentManager();
        var uri = DocumentUri.From("file:///unknown.jex");
        
        var state = manager.GetDocument(uri);
        
        Assert.Null(state);
    }

    [Fact]
    public void RemoveDocument_ShouldRemoveStoredDocument()
    {
        var manager = new DocumentManager();
        var uri = DocumentUri.From("file:///test.jex");
        manager.UpdateDocument(uri, "%let x = 42;");
        
        manager.RemoveDocument(uri);
        var state = manager.GetDocument(uri);
        
        Assert.Null(state);
    }

    [Fact]
    public void UpdateDocument_ShouldReplaceExistingDocument()
    {
        var manager = new DocumentManager();
        var uri = DocumentUri.From("file:///test.jex");
        manager.UpdateDocument(uri, "%let x = 1;", 1);
        
        manager.UpdateDocument(uri, "%let x = 2;", 2);
        var state = manager.GetDocument(uri);
        
        Assert.NotNull(state);
        Assert.Equal("%let x = 2;", state.Content);
        Assert.Equal(2, state.Version);
    }

    [Fact]
    public void GetAllDocuments_ShouldReturnAllStoredDocuments()
    {
        var manager = new DocumentManager();
        manager.UpdateDocument(DocumentUri.From("file:///a.jex"), "a");
        manager.UpdateDocument(DocumentUri.From("file:///b.jex"), "b");
        
        var docs = manager.GetAllDocuments().ToList();
        
        Assert.Equal(2, docs.Count);
    }
}
