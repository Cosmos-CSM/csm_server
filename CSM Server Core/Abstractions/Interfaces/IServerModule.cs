using CSM_Server_Core.Core.Models;

using Microsoft.AspNetCore.Builder;

namespace CSM_Server_Core.Abstractions.Interfaces;

/// <summary>
///     Represents an entry configuration for a customization layer of a product server.
/// </summary>
public interface IServerModule {

    /// <summary>
    ///     Server module sign identification.
    /// </summary>
    public string Sign { get; }

    /// <summary>
    ///     Module app configuration call.
    /// </summary>
    /// <param name="app">
    ///     Current server application instance.
    /// </param>
    /// <param name="serverSettings">
    ///     Current server loaded settings.
    /// </param>
    public Task ConfigureApp(WebApplication app, ServerSettings serverSettings);

    /// <summary>
    ///     Module app builder call.
    /// </summary>
    /// <param name="builder">
    ///     Current server application builder instance.
    /// </param>
    /// <param name="serverSettings">
    ///     Current server loaded settings.
    /// </param>
    public Task BuildApp(WebApplicationBuilder builder, ServerSettings serverSettings);
}
