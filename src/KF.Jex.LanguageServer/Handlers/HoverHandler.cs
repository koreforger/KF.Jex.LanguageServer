using KF.Jex.LanguageServer.Services;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace KF.Jex.LanguageServer.Handlers;

/// <summary>
/// Handles hover requests for JEX documents.
/// </summary>
public sealed class HoverHandler : HoverHandlerBase
{
    private readonly DocumentManager _documentManager;
    private readonly FunctionManifestLoader _manifestLoader;

    private static readonly Dictionary<string, string> BuiltInDocs = new()
    {
        ["$in"] = "**$in** - Input JSON document\n\nThe JSON document passed to the JEX script for transformation.",
        ["$out"] = "**$out** - Output JSON document\n\nThe JSON document being constructed by the script. Use `%set` to modify.",
        ["$meta"] = "**$meta** - Metadata object\n\nOptional metadata passed alongside the input document.",
        ["%let"] = "**%let** - Variable declaration\n\n```jex\n%let variableName = expression;\n```\n\nDeclares a local variable.",
        ["%set"] = "**%set** - Set output path\n\n```jex\n%set $.path = expression;\n```\n\nSets a value in the output document.",
        ["%if"] = "**%if** - Conditional statement\n\n```jex\n%if (condition) %then %do;\n    // statements\n%end;\n```",
        ["%foreach"] = "**%foreach** - Loop over array\n\n```jex\n%foreach item %in array %do;\n    // statements using item\n%end;\n```",
        ["%func"] = "**%func** - Function declaration\n\n```jex\n%func myFunction(param1, param2);\n    // statements (use &param1, &param2 to reference params)\n    %return value;\n%endfunc;\n```",
        ["%return"] = "**%return** - Return from function\n\n```jex\n%return expression;\n```",
        ["%break"] = "**%break** - Exit loop\n\nExits the current `%foreach` or `%do` loop.",
        ["%continue"] = "**%continue** - Skip iteration\n\nSkips to the next iteration of the current loop.",
    };

    public HoverHandler(DocumentManager documentManager, FunctionManifestLoader manifestLoader)
    {
        _documentManager = documentManager;
        _manifestLoader = manifestLoader;
    }

    public override Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        var document = _documentManager.GetDocument(request.TextDocument.Uri);
        if (document is null)
            return Task.FromResult<Hover?>(null);

        var line = document.GetLine((int)request.Position.Line);
        if (string.IsNullOrEmpty(line))
            return Task.FromResult<Hover?>(null);

        var word = GetWordAtPosition(line, (int)request.Position.Character);
        if (string.IsNullOrEmpty(word))
            return Task.FromResult<Hover?>(null);

        // Check built-in docs
        if (BuiltInDocs.TryGetValue(word, out var builtInDoc))
        {
            return Task.FromResult<Hover?>(new Hover
            {
                Contents = new MarkedStringsOrMarkupContent(new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = builtInDoc
                })
            });
        }

        // Check standard library functions
        var stdFunc = StandardLibraryProvider.GetFunction(word);
        if (stdFunc is not null)
        {
            return Task.FromResult<Hover?>(new Hover
            {
                Contents = new MarkedStringsOrMarkupContent(new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = $"**{stdFunc.Name}** (Standard Library)\n\n```\n{stdFunc.Signature}\n```\n\n{stdFunc.Description}"
                })
            });
        }

        // Check manifest functions
        var manifestFunc = _manifestLoader.GetFunction(word);
        if (manifestFunc is not null)
        {
            var paramDocs = "";
            if (manifestFunc.Parameters?.Count > 0)
            {
                paramDocs = "\n\n**Parameters:**\n" + 
                    string.Join("\n", manifestFunc.Parameters.Select(p => 
                        $"- `{p.Name}` ({p.Type ?? "any"}){(p.Optional ? " - optional" : "")}: {p.Description ?? ""}"));
            }

            return Task.FromResult<Hover?>(new Hover
            {
                Contents = new MarkedStringsOrMarkupContent(new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = $"**{manifestFunc.Name}** (Host Function)\n\n```\n{manifestFunc.Signature ?? manifestFunc.Name + "(...)"}\n```\n\n{manifestFunc.Description ?? ""}{paramDocs}"
                })
            });
        }

        // Check user-defined functions
        var userFunc = document.GetUserFunctions().FirstOrDefault(f => 
            string.Equals(f.Name, word, StringComparison.OrdinalIgnoreCase));
        if (userFunc is not null)
        {
            var signature = $"{userFunc.Name}({string.Join(", ", userFunc.Parameters)})";
            return Task.FromResult<Hover?>(new Hover
            {
                Contents = new MarkedStringsOrMarkupContent(new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = $"**{userFunc.Name}** (User Function)\n\n```jex\n{signature}\n```\n\n{userFunc.Description}"
                })
            });
        }

        // Check variables
        var variable = document.GetVariablesAtPosition((int)request.Position.Line + 1, (int)request.Position.Character + 1)
            .FirstOrDefault(v => string.Equals(v.Name, word, StringComparison.OrdinalIgnoreCase));
        if (variable is not null)
        {
            return Task.FromResult<Hover?>(new Hover
            {
                Contents = new MarkedStringsOrMarkupContent(new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = $"**{variable.Name}** - {variable.Description}"
                })
            });
        }

        return Task.FromResult<Hover?>(null);
    }

    private static string GetWordAtPosition(string line, int character)
    {
        if (character < 0 || character >= line.Length)
        {
            character = Math.Max(0, Math.Min(character, line.Length - 1));
        }

        // Find word boundaries
        var start = character;
        var end = character;

        // Include % or $ prefix
        while (start > 0 && (char.IsLetterOrDigit(line[start - 1]) || line[start - 1] == '_'))
            start--;
        
        if (start > 0 && (line[start - 1] == '%' || line[start - 1] == '$'))
            start--;

        while (end < line.Length && (char.IsLetterOrDigit(line[end]) || line[end] == '_'))
            end++;

        if (start >= end) return "";
        return line[start..end];
    }

    protected override HoverRegistrationOptions CreateRegistrationOptions(
        HoverCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new HoverRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("jex")
        };
    }
}
