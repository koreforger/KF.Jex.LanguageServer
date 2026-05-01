# KF.Jex.LanguageServer

A Language Server Protocol (LSP) implementation for the JEX (JSON Expression) transformation DSL.

## Overview

This project provides IDE support for JEX files through the Language Server Protocol, enabling features like:

- **Syntax Error Diagnostics** - Real-time error detection
- **Code Completion** - Keywords, built-in variables, functions, user-defined functions
- **Hover Information** - Documentation on hover
- **Document Synchronization** - Tracks open/changed files

## Building

```powershell
.\scr\build-rebuild.ps1
```

## Testing

```powershell
.\scr\build-test.ps1
```

## Usage

This Language Server is typically bundled with the [KF.Jex.VSCode](../KF.Jex.VSCode) extension and runs automatically when you open a `.jex` file.

### Standalone Usage

```bash
# Run the Language Server (communicates via stdio)
dotnet run --project src/KF.Jex.LanguageServer
```

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Language Server                          │
│  ┌───────────────────────────────────────────────────────┐  │
│  │                    Handlers                            │  │
│  │  ┌────────────┐ ┌────────────┐ ┌──────────────────┐   │  │
│  │  │ Completion │ │   Hover    │ │ TextDocumentSync │   │  │
│  │  │  Handler   │ │  Handler   │ │     Handler      │   │  │
│  │  └─────┬──────┘ └─────┬──────┘ └────────┬─────────┘   │  │
│  └────────┼──────────────┼─────────────────┼─────────────┘  │
│           │              │                 │                │
│  ┌────────┴──────────────┴─────────────────┴─────────────┐  │
│  │                     Services                          │  │
│  │  ┌───────────┐ ┌─────────────┐ ┌─────────────────┐   │  │
│  │  │ Document  │ │  Standard   │ │    Function     │   │  │
│  │  │  Manager  │ │   Library   │ │ ManifestLoader  │   │  │
│  │  └─────┬─────┘ └─────────────┘ └─────────────────┘   │  │
│  │        │                                              │  │
│  │  ┌─────┴─────┐                                       │  │
│  │  │ Document  │ ◄─── Uses KoreForge.Jex Parser            │  │
│  │  │   State   │                                       │  │
│  │  └───────────┘                                       │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Dependencies

- **KoreForge.Jex** - Core JEX parser and runtime
- **OmniSharp.Extensions.LanguageServer** - LSP framework

## License

MIT License - See [LICENSE.md](LICENSE.md)


