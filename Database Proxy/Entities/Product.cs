using CSM_Database_Core.Entities.Abstractions.Bases;

namespace Database_Proxy.Entities;

/// <summary>
///     Represents a business product.
/// </summary>
public class Product : CatalogEntityBase {

    /// <inheritdoc/>
    public override Type Database { get; init; } = typeof(DatabaseProxy);
}
