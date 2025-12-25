namespace CSM_Server_Core.Core.Models;

/// <summary>
///     Represents a server settings data.
/// </summary>
public struct ServerSettings
{
    /// <summary>
    ///     Name of the company or organization owner for the server.
    /// </summary>
    public required string Tenant;

    /// <summary>
    ///     Stores the identification host server name or address.
    /// </summary>
    public required string Host;

    /// <summary>
    ///     CSM Ecosystem nets signature identification.
    /// </summary>

    public required string Signature;

    /// <summary>
    ///     Addresses used by the server listening to serve.
    /// </summary>

    public required string[] Listeners;

    /// <summary>
    ///     CORS Configuration, allows the configured origins to access the server.
    /// </summary>

    public string[] AllowedOrigins;
}
