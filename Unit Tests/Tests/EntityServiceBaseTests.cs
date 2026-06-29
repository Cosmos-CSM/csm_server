using CSM_Server_Core_Testing.Abstractions.Bases;

using Database_Proxy.Depots;
using Database_Proxy.Entities;

using Unit_Tests.Proxies;

namespace Unit_Tests.Tests;


/// <summary>
///     Unit tests class for <see cref="EntityServiceBaseProxy"/>.
/// </summary>
public class EntityServiceBaseTests : EntityServiceUnitTestsBase<Order, IOrdersDepot, EntityServiceBaseProxy> {

    protected override EntityServiceBaseProxy ServiceFactory(IOrdersDepot depotMock) {

        return new EntityServiceBaseProxy(depotMock);
    }
}
