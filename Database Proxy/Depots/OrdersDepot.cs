using CSM_Database_Core.Depots.Abstractions.Bases;
using CSM_Database_Core.Depots.Abstractions.Interfaces;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Foundation_Core.Abstractions.Interfaces;

using Database_Proxy.Entities;

namespace Database_Proxy.Depots;

public interface IOrdersDepot : IDepot<Order> {

}

/// <summary>
///     Represents an <see cref="Order"/> entity depot.
/// </summary>
public class OrdersDepot : 
    DepotBase<DatabaseProxy, Order>, IOrdersDepot {

    /// <inheritdoc/>
    public OrdersDepot(DatabaseProxy database, IDisposer<IEntity>? disposer) 
        : base(database, disposer) {
    }
}
