namespace CSM_Server_Core.Core.Models;

/// <summary>
///     Represents a server settings data.
/// </summary>
public struct ServerSettings
{
    /// <summary>
    ///     Name of the company or organization owner for the server.
    /// </summary>
    public required string Tenant { get; init;}

    /// <summary>
    ///     Stores the identification host server name or address.
    /// </summary>
    public required string Host { get; init; }

    /// <summary>
    ///     CSM Ecosystem nets signature identification.
    /// </summary>

    public required string Signature { get; init; }

    /// <summary>
    ///     Addresses used by the server listening to serve.
    /// </summary>

    public required string[] Listeners { get; init; }

    /// <summary>
    ///     CORS Configuration, allows the configured origins to access the server.
    /// </summary>

    public string[] AllowedOrigins { get; init; }
}
