using Khaos.JEX.Lexer;
using Khaos.JEX.Parser;
using OmniSharp.Extensions.LanguageServer.Protocol;
using JexProgram = Khaos.JEX.Parser.Program;

namespace Khaos.JEX.LanguageServer.Services;

/// <summary>
/// Represents the parsed state of a document.
/// </summary>
public sealed class DocumentState
{
    public DocumentUri Uri { get; }
    public string Content { get; }
    public int? Version { get; }
    public JexProgram? Ast { get; private set; }
    public List<DiagnosticInfo> ParseErrors { get; } = new();
    public List<string> Lines { get; }

    public DocumentState(DocumentUri uri, string content, int? version)
    {
        Uri = uri;
        Content = content;
        Version = version;
        Lines = content.Split('\n').ToList();
        Parse();
    }

    private void Parse()
    {
        try
        {
            var parser = new JexParser(Content);
            Ast = parser.Parse();
        }
        catch (JexCompileException ex)
        {
            ParseErrors.Add(new DiagnosticInfo(
                ex.Message,
                ex.Span ?? new SourceSpan(new SourcePosition(1, 1, 0), new SourcePosition(1, 1, 0)),
                DiagnosticSeverity.Error
            ));
        }
        catch (Exception ex)
        {
            ParseErrors.Add(new DiagnosticInfo(
                $"Unexpected error: {ex.Message}",
                new SourceSpan(new SourcePosition(1, 1, 0), new SourcePosition(1, 1, 0)),
                DiagnosticSeverity.Error
            ));
        }
    }

    /// <summary>
    /// Gets all user-defined functions in this document.
    /// </summary>
    public IEnumerable<FunctionInfo> GetUserFunctions()
    {
        if (Ast is null) yield break;

        foreach (var stmt in Ast.Statements)
        {
            if (stmt is FunctionDeclaration func)
            {
                yield return new FunctionInfo(
                    func.Name,
                    func.Parameters,
                    $"User-defined function with {func.Parameters.Count} parameter(s)",
                    func.Span
                );
            }
        }
    }

    /// <summary>
    /// Gets all variables declared in this document up to a given position.
    /// </summary>
    public IEnumerable<VariableInfo> GetVariablesAtPosition(int line, int column)
    {
        if (Ast is null) yield break;

        foreach (var variable in CollectVariables(Ast.Statements, line, column))
        {
            yield return variable;
        }
    }

    private IEnumerable<VariableInfo> CollectVariables(IEnumerable<Statement> statements, int line, int column)
    {
        foreach (var stmt in statements)
        {
            // Only include variables declared before the cursor position
            if (stmt.Span.Start.Line > line || 
                (stmt.Span.Start.Line == line && stmt.Span.Start.Column > column))
            {
                continue;
            }

            if (stmt is LetStatement let)
            {
                yield return new VariableInfo(let.VariableName, "Local variable", let.Span);
            }
            else if (stmt is ForeachStatement forEach)
            {
                // Iterator variable is available inside the loop
                if (IsPositionInBlock(forEach.Body, line, column))
                {
                    yield return new VariableInfo(forEach.IteratorName, "Loop iterator", forEach.Span);
                }
                foreach (var v in CollectVariables(forEach.Body, line, column))
                    yield return v;
            }
            else if (stmt is DoLoopStatement doLoop)
            {
                if (IsPositionInBlock(doLoop.Body, line, column))
                {
                    yield return new VariableInfo(doLoop.IteratorName, "Loop counter", doLoop.Span);
                }
                foreach (var v in CollectVariables(doLoop.Body, line, column))
                    yield return v;
            }
            else if (stmt is IfStatement ifStmt)
            {
                foreach (var v in CollectVariables(ifStmt.ThenBlock, line, column))
                    yield return v;
                if (ifStmt.ElseBlock is not null)
                {
                    foreach (var v in CollectVariables(ifStmt.ElseBlock, line, column))
                        yield return v;
                }
            }
            else if (stmt is FunctionDeclaration func)
            {
                // Function parameters are available inside the function
                if (IsPositionInBlock(func.Body, line, column))
                {
                    foreach (var param in func.Parameters)
                    {
                        yield return new VariableInfo(param, "Function parameter", func.Span);
                    }
                }
                foreach (var v in CollectVariables(func.Body, line, column))
                    yield return v;
            }
        }
    }

    private bool IsPositionInBlock(List<Statement> block, int line, int column)
    {
        if (block.Count == 0) return false;
        var first = block[0];
        var last = block[^1];
        
        if (line < first.Span.Start.Line) return false;
        if (line > last.Span.End.Line) return false;
        
        return true;
    }

    /// <summary>
    /// Gets the line at the specified 0-based index.
    /// </summary>
    public string? GetLine(int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= Lines.Count) return null;
        return Lines[lineIndex];
    }
}

public enum DiagnosticSeverity
{
    Error,
    Warning,
    Information,
    Hint
}

public record DiagnosticInfo(string Message, SourceSpan Span, DiagnosticSeverity Severity);
public record FunctionInfo(string Name, List<string> Parameters, string Description, SourceSpan Span);
public record VariableInfo(string Name, string Description, SourceSpan Span);
