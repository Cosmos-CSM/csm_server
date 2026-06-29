using CSM_Database_Core.Entities.Abstractions.Bases;

namespace Database_Proxy.Entities;

/// <summary>
///     Represents a business order.
/// </summary>
public class Order : CatalogEntityBase {

    /// <inheritdoc/>
    public override Type Database { get; init; } = typeof(DatabaseProxy);
}
