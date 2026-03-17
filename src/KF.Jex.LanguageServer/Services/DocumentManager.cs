using System.Collections.Concurrent;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace KF.Jex.LanguageServer.Services;

/// <summary>
/// Manages open text documents and their parsed state.
/// </summary>
public sealed class DocumentManager
{
    private readonly ConcurrentDictionary<DocumentUri, DocumentState> _documents = new();

    /// <summary>
    /// Opens or updates a document with new content.
    /// </summary>
    public DocumentState UpdateDocument(DocumentUri uri, string content, int? version = null)
    {
        var state = new DocumentState(uri, content, version);
        _documents[uri] = state;
        return state;
    }

    /// <summary>
    /// Gets the current state of a document.
    /// </summary>
    public DocumentState? GetDocument(DocumentUri uri)
    {
        _documents.TryGetValue(uri, out var state);
        return state;
    }

    /// <summary>
    /// Removes a document from tracking.
    /// </summary>
    public void RemoveDocument(DocumentUri uri)
    {
        _documents.TryRemove(uri, out _);
    }

    /// <summary>
    /// Gets all tracked documents.
    /// </summary>
    public IEnumerable<DocumentState> GetAllDocuments() => _documents.Values;
}
