using CSM_Database_Core.Entities.Abstractions.Bases;

namespace Database_Proxy.Entities;

/// <summary>
///     Represents a business costumer.
/// </summary>
public class Customer : CatalogEntityBase {

    /// <inheritdoc/>
    public override Type Database { get; init; } = typeof(DatabaseProxy);
}
