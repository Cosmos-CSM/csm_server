using System.Net;
using System.Net.Sockets;
using System.Text.Json;

using CSM_Foundation_Core.Abstractions.Interfaces;
using CSM_Foundation_Core.Core.Exceptions;
using CSM_Foundation_Core.Core.Utils;
using CSM_Foundation_Core.Errors.Abstractions.Interfaces;

using CSM_Server_Core.Core.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSM_Server_Core.Core.Utils;

/// <summary>
///     Provide server context scoped utilities to handle common server operations.
/// </summary>
public static class ServerUtils {
    /// <summary>
    ///     Stores if the logs were disabled globally for all server utils operations.
    /// </summary>
    static bool _useLogs = true;

    /// <summary>
    ///     Changes the configuration for this utilities to use or not logs on its operations.
    /// </summary>
    /// <param name="useLogs">
    ///     Whether to use logs or not.
    /// </param>
    public static void ToogleGlobalLogs(bool useLogs) => _useLogs = useLogs;

    /// <summary>
    ///     Starts the server engines and prepares standard usages and tools, giving simplified methods to build and configure the application.
    /// </summary>
    /// <param name="sign">
    ///     Application built time sign identification.
    /// </param>
    /// <param name="buildApplication">
    ///     Application building process custom call.
    /// </param>
    /// <param name="configApp">
    ///     Application running configuration custom call.
    /// </param>
    /// <exception cref="InvalidDataException"></exception>
    public static async void Start(string sign, Func<WebApplicationBuilder, ServerSettings, Task> buildApplication, Func<WebApplication, ServerSettings, Task> configApp) {
        try {
            sign = sign.ToUpper();

            ConsoleUtils.Announce("Starting server engine...");

            ServerSettings serverSettings = GetServerSettings(sign);

            if (sign != serverSettings.Signature)
                throw new InvalidDataException($"Server built signature ({sign}) doesn't match server settings configured signature ({serverSettings.Signature})");

            WebApplicationBuilder builder = WebApplication.CreateBuilder();

            builder.Logging.ClearProviders();
            builder.Services.AddControllers(
                    options => options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>()
                );

            builder.Services.AddCors(
                (CorsOptions) => {
                    CorsOptions.AddDefaultPolicy(
                    (PolicyBuilder) => {
                                PolicyBuilder.AllowAnyHeader();
                                PolicyBuilder.AllowAnyMethod();
                                PolicyBuilder.SetIsOriginAllowed(
                                    (Origin) => {
                                        string[] corsPolicies = serverSettings.AllowedOrigins;
                                        Uri parsedUrl = new(Origin);

                                        bool isCorsAllowed = corsPolicies.Contains(parsedUrl.Host);
                                        if (!isCorsAllowed) {
                                            ConsoleUtils.Warning(
                                                "Origin not allowed, blocked by CORS policies.",
                                                new() {
                                                { nameof(parsedUrl), parsedUrl }
                                                }
                                            );
                                        }

                                        return isCorsAllowed;
                                    }
                                );
                            }
                );
                }
                );
            builder.Services.AddHttpContextAccessor();

            // --> Passing implementation server configuration priority.
            await buildApplication(builder, serverSettings);

            WebApplication app = builder.Build();
            app.MapControllers();
            app.UseCors();

            await configApp(app, serverSettings);

            ConsoleUtils.Success("Server engine set up.");

            app.Run();
        } catch (Exception X) when (X is IError AX) {
            ConsoleUtils.Exception(AX);
            throw;
        } catch (Exception X) {
            ConsoleUtils.Exception(new SystemError($"Engine start exception", X));
        } finally {
            Console.WriteLine($"Press any key to close...");
            Console.ReadKey();
        }
    }

    /// <summary>
    ///     Gets the server settings based on the given <paramref name="sign"/>.
    /// </summary>
    /// <param name="sign">
    ///     Server running instance signature identification.
    /// </param>
    /// <param name="useLogs">
    ///     Whether the operation should use logs or not.
    /// </param>
    /// <returns>
    ///     A correctly loaded <see cref="ServerSettings"/> data.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static ServerSettings GetServerSettings(string sign, bool useLogs = true) {
        SystemEnvs systemEnv = SystemUtils.GetEnv();
        string baseDir = AppContext.BaseDirectory;

        if (_useLogs && useLogs) {
            ConsoleUtils.Note(
                "Getting server settings",
                new Dictionary<string, object?> {
                        { "Signature", sign},
                        { "Environment", systemEnv },
                        { "Application Dir", baseDir },
                    }
                );
        }

        // --> Calculating settings file path from env vars.
        string? filePath = SystemUtils.GetGlobalVar($"{sign}_server_settings");

        // --> Calculating settings file path from default.
        if (string.IsNullOrWhiteSpace(filePath)) {

            switch (systemEnv) {
                case SystemEnvs.PROD:
                    filePath = $"{baseDir}{sign}_server_settings.production.json";
                    break;
                case SystemEnvs.DEV:
                    filePath = $"{baseDir}{sign}_server_settings.development.json";
                    break;
                case SystemEnvs.QA:
                    filePath = $"{baseDir}{sign}_server_settings.quality.json";
                    break;
                case SystemEnvs.LAB:
                    filePath = $"{baseDir}{sign}_server_settings.laboratory.json";
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(filePath);

        string host = GetHost();
        string formattedPath = FileUtils.FormatLocation(filePath);
        string[] listeners = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(";") ?? [];


        Dictionary<string, object?> tmpObject = FileUtils.Deserealize<Dictionary<string, object?>>(formattedPath);

        tmpObject.Add(nameof(ServerSettings.Host), host);
        tmpObject.Add(nameof(ServerSettings.Listeners), listeners);

        ServerSettings? serverSettings = JsonSerializer.Deserialize<ServerSettings?>(JsonSerializer.Serialize(tmpObject))
            ?? throw new Exception($"Wrong [Settings] file format.");

        if (_useLogs && useLogs) {
            ConsoleUtils.Success(
                    "Server settings correctly loaded",
                    details: new Dictionary<string, object?> {
                        { "File Path", formattedPath },
                        { "File Content", tmpObject },
                    }
                );
        }

        return (ServerSettings)serverSettings;
    }

    /// <summary>
    ///     Gets the current runtime host dns direction where the server is running on.
    /// </summary>
    /// <returns>
    ///     DNS Parsed server host direction, N/A if there's no resolvable host.
    /// </returns>
    public static string GetHost() {
        string hn = Dns.GetHostName();
        IPAddress[] @as = Dns.GetHostAddresses(hn);
        string h = @as.Where(I => I.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault()?.ToString() ?? "N/A";
        return h;
    }
}

