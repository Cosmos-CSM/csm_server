using CSM_Sandbox_Database_Core.Depots.Abstractions.Interfaces;
using CSM_Sandbox_Database_Core.Entities;

using CSM_Sandbox_Database_Testing.Utils;

using CSM_Server_Core.Core.Utils.Abstractions.Interfaces;

using CSM_Server_Testing.Abstractions.Bases;

using Unit_Tests.Proxies;

namespace Unit_Tests.Tests;

/// <summary>
///     Entity Service Unit tests class for <see cref="EntityServiceBaseProxy"/>.
/// </summary>
public class EntityServiceBaseTests
    : EntityServiceUnitTestsBase<Order, IOrdersDepot, EntityServiceBaseProxy> {

    protected override async Task<Order> DraftEntity() {
        return DraftUtils.Order();
    }

    protected override async Task<EntityServiceBaseProxy> ServiceFactory(IOrdersDepot depotMock, IEntityServiceUtils entityServiceUtilsMock) {

        return new EntityServiceBaseProxy(depotMock, entityServiceUtilsMock);
    }
}
