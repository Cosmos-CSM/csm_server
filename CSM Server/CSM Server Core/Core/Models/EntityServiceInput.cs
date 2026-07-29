using CSM_Database_Core.Entities.Abstractions.Interfaces;

namespace CSM_Server_Core.Core.Models;


/// <summary>
///     Represents an <see cref="IEntity"/> service input.
/// </summary>
/// <typeparam name="TParams"></typeparam>
public struct EntityServiceInput<TParams> {

    /// <summary>
    ///     Service parameters.
    /// </summary>
    public TParams Parameters { get; set; }

    /// <summary>
    ///     Entity relations to be loaded.
    /// </summary>
    /// <remarks>
    ///     Inner relations can be loaded at any level specifying the path with a '.' like: "Users.Permits.Actions",
    ///     the logic will determine the levels and configuration to correctly load relations.
    /// </remarks>
    public string[] Relations { get; set;}
}
