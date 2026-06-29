using CSM_Server_Core.Abstractions.Bases;

using Database_Proxy.Depots;
using Database_Proxy.Entities;

namespace Unit_Tests.Proxies;

/// <summary>
///     Represents a proxy base class for <see cref="EntityServiceBase{TEntity, TDepot}"/> to call and unit tests their implementations.
/// </summary>
public class EntityServiceBaseProxy : EntityServiceBase<Order, IOrdersDepot> {

    /// <inheritdoc/>
    public EntityServiceBaseProxy(IOrdersDepot depot) : base(depot) {
    }
}
