using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Server;
using KoreForge.Jex.LanguageServer.Handlers;
using KoreForge.Jex.LanguageServer.Services;

namespace KoreForge.Jex.LanguageServer;

/// <summary>
/// Entry point for the JEX Language Server.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var server = await OmniSharp.Extensions.LanguageServer.Server.LanguageServer.From(options =>
        {
            options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Warning);
                })
                .WithServices(ConfigureServices)
                .WithServerInfo(new ServerInfo
                {
                    Name = "JEX Language Server",
                    Version = "0.1.0"
                });

            // Register LSP handlers
            options.WithHandler<TextDocumentSyncHandler>();
            options.WithHandler<CompletionHandler>();
            options.WithHandler<HoverHandler>();
        });

        await server.WaitForExit;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<DocumentManager>();
        services.AddSingleton<FunctionManifestLoader>();
        services.AddSingleton<DiagnosticsPublisher>();
    }
}
