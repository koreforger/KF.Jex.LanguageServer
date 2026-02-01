using Khaos.JEX.LanguageServer.Services;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.General;

namespace Khaos.JEX.LanguageServer.Handlers;

/// <summary>
/// Handles text document synchronization (open, change, close).
/// </summary>
public sealed class TextDocumentSyncHandler : TextDocumentSyncHandlerBase
{
    private readonly DocumentManager _documentManager;
    private readonly ILanguageServerFacade _server;
    private readonly DiagnosticsPublisher _diagnosticsPublisher;

    public TextDocumentSyncHandler(
        DocumentManager documentManager,
        ILanguageServerFacade server,
        DiagnosticsPublisher diagnosticsPublisher)
    {
        _documentManager = documentManager;
        _server = server;
        _diagnosticsPublisher = diagnosticsPublisher;
    }

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
    {
        return new TextDocumentAttributes(uri, "jex");
    }

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        var state = _documentManager.UpdateDocument(
            request.TextDocument.Uri,
            request.TextDocument.Text,
            request.TextDocument.Version);

        _diagnosticsPublisher.PublishDiagnostics(state);
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        // We request full sync, so there's only one change with full content
        var content = request.ContentChanges.First().Text;
        var state = _documentManager.UpdateDocument(
            request.TextDocument.Uri,
            content,
            request.TextDocument.Version);

        _diagnosticsPublisher.PublishDiagnostics(state);
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        _documentManager.RemoveDocument(request.TextDocument.Uri);
        
        // Clear diagnostics for closed document
        _server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
        {
            Uri = request.TextDocument.Uri,
            Diagnostics = new Container<Diagnostic>()
        });

        return Unit.Task;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        // No special handling needed for save
        return Unit.Task;
    }

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
        TextSynchronizationCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new TextDocumentSyncRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("jex"),
            Change = TextDocumentSyncKind.Full,
            Save = new SaveOptions { IncludeText = false }
        };
    }
}

/// <summary>
/// Publishes diagnostics to the client.
/// </summary>
public sealed class DiagnosticsPublisher
{
    private readonly ILanguageServerFacade _server;

    public DiagnosticsPublisher(ILanguageServerFacade server)
    {
        _server = server;
    }

    public void PublishDiagnostics(DocumentState state)
    {
        var diagnostics = state.ParseErrors.Select(e => new Diagnostic
        {
            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range
            {
                Start = new Position(e.Span.Start.Line - 1, e.Span.Start.Column - 1),
                End = new Position(e.Span.End.Line - 1, e.Span.End.Column - 1)
            },
            Severity = e.Severity switch
            {
                Services.DiagnosticSeverity.Error => OmniSharp.Extensions.LanguageServer.Protocol.Models.DiagnosticSeverity.Error,
                Services.DiagnosticSeverity.Warning => OmniSharp.Extensions.LanguageServer.Protocol.Models.DiagnosticSeverity.Warning,
                Services.DiagnosticSeverity.Information => OmniSharp.Extensions.LanguageServer.Protocol.Models.DiagnosticSeverity.Information,
                Services.DiagnosticSeverity.Hint => OmniSharp.Extensions.LanguageServer.Protocol.Models.DiagnosticSeverity.Hint,
                _ => OmniSharp.Extensions.LanguageServer.Protocol.Models.DiagnosticSeverity.Error
            },
            Source = "jex",
            Message = e.Message
        }).ToList();

        _server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
        {
            Uri = state.Uri,
            Version = state.Version,
            Diagnostics = new Container<Diagnostic>(diagnostics)
        });
    }
}
