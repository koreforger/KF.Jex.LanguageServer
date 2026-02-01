using Khaos.JEX.LanguageServer.Services;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Khaos.JEX.LanguageServer.Handlers;

/// <summary>
/// Handles completion requests for JEX documents.
/// </summary>
public sealed class CompletionHandler : CompletionHandlerBase
{
    private readonly DocumentManager _documentManager;
    private readonly FunctionManifestLoader _manifestLoader;

    private static readonly string[] Keywords = 
    {
        "%let", "%set", "%if", "%then", "%else", "%do", "%end", "%endfunc",
        "%foreach", "%in", "%break", "%continue", "%return", "%func"
    };

    private static readonly string[] BuiltInVariables = { "$in", "$out", "$meta" };
    private static readonly string[] BooleanLiterals = { "true", "false", "null" };

    public CompletionHandler(DocumentManager documentManager, FunctionManifestLoader manifestLoader)
    {
        _documentManager = documentManager;
        _manifestLoader = manifestLoader;
    }

    public override Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        var items = new List<CompletionItem>();
        var document = _documentManager.GetDocument(request.TextDocument.Uri);

        // Get context for smarter completions
        var line = document?.GetLine((int)request.Position.Line);
        var prefix = GetPrefix(line, (int)request.Position.Character);

        // Keywords
        foreach (var keyword in Keywords)
        {
            if (ShouldInclude(keyword, prefix))
            {
                items.Add(new CompletionItem
                {
                    Label = keyword,
                    Kind = CompletionItemKind.Keyword,
                    Detail = "JEX keyword",
                    InsertText = keyword
                });
            }
        }

        // Built-in variables
        foreach (var builtin in BuiltInVariables)
        {
            if (ShouldInclude(builtin, prefix))
            {
                items.Add(new CompletionItem
                {
                    Label = builtin,
                    Kind = CompletionItemKind.Variable,
                    Detail = builtin switch
                    {
                        "$in" => "Input JSON document",
                        "$out" => "Output JSON document",
                        "$meta" => "Metadata object",
                        _ => "Built-in variable"
                    },
                    InsertText = builtin
                });
            }
        }

        // Boolean/null literals
        foreach (var literal in BooleanLiterals)
        {
            if (ShouldInclude(literal, prefix))
            {
                items.Add(new CompletionItem
                {
                    Label = literal,
                    Kind = CompletionItemKind.Constant,
                    Detail = "Literal value",
                    InsertText = literal
                });
            }
        }

        // Standard library functions
        foreach (var func in StandardLibraryProvider.GetFunctions())
        {
            if (ShouldInclude(func.Name, prefix))
            {
                items.Add(CreateFunctionCompletionItem(func.Name, func.Signature, func.Description, "Standard library"));
            }
        }

        // Manifest functions (host-registered)
        foreach (var func in _manifestLoader.Functions)
        {
            if (ShouldInclude(func.Name, prefix))
            {
                items.Add(CreateFunctionCompletionItem(
                    func.Name,
                    func.Signature ?? $"{func.Name}(...)",
                    func.Description ?? "Host-registered function",
                    "Host function"));
            }
        }

        // User-defined functions from document
        if (document is not null)
        {
            foreach (var func in document.GetUserFunctions())
            {
                if (ShouldInclude(func.Name, prefix))
                {
                    var signature = $"{func.Name}({string.Join(", ", func.Parameters)})";
                    items.Add(CreateFunctionCompletionItem(func.Name, signature, func.Description, "User function"));
                }
            }

            // Variables in scope
            foreach (var variable in document.GetVariablesAtPosition((int)request.Position.Line + 1, (int)request.Position.Character + 1))
            {
                if (ShouldInclude(variable.Name, prefix))
                {
                    items.Add(new CompletionItem
                    {
                        Label = variable.Name,
                        Kind = CompletionItemKind.Variable,
                        Detail = variable.Description,
                        InsertText = variable.Name
                    });
                }
            }
        }

        return Task.FromResult(new CompletionList(items));
    }

    private static CompletionItem CreateFunctionCompletionItem(string name, string signature, string description, string category)
    {
        return new CompletionItem
        {
            Label = name,
            Kind = CompletionItemKind.Function,
            Detail = $"({category}) {signature}",
            Documentation = new MarkupContent
            {
                Kind = MarkupKind.Markdown,
                Value = $"```\n{signature}\n```\n\n{description}"
            },
            InsertText = $"{name}($0)",
            InsertTextFormat = InsertTextFormat.Snippet
        };
    }

    private static string GetPrefix(string? line, int character)
    {
        if (string.IsNullOrEmpty(line) || character <= 0) return "";

        var end = Math.Min(character, line.Length);
        var start = end;

        while (start > 0 && IsIdentifierChar(line[start - 1]))
        {
            start--;
        }

        // Include % or $ prefix if present
        if (start > 0 && (line[start - 1] == '%' || line[start - 1] == '$'))
        {
            start--;
        }

        return line[start..end];
    }

    private static bool IsIdentifierChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static bool ShouldInclude(string item, string prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return true;
        return item.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    public override Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
    {
        // No additional resolution needed - we provide full details upfront
        return Task.FromResult(request);
    }

    protected override CompletionRegistrationOptions CreateRegistrationOptions(
        CompletionCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new CompletionRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("jex"),
            TriggerCharacters = new Container<string>("%", "$", "."),
            ResolveProvider = false
        };
    }
}
