using CSM_Sandbox_Database_Core;
using CSM_Sandbox_Database_Core.Depots;
using CSM_Sandbox_Database_Core.Depots.Abstractions.Interfaces;
using CSM_Sandbox_Database_Core.Entities;

using CSM_Sandbox_Database_Testing.Managers;
using CSM_Sandbox_Database_Testing.Utils;

using CSM_Server_Core.Core.Utils;
using CSM_Server_Core.Core.Utils.Abstractions.Interfaces;

using CSM_Server_Testing.Abstractions.Bases;

using Integration_Tests.Proxies;

namespace Integration_Tests.Tests;

/// <summary>
///     Entity Service Integration tests class for <see cref="EntityServiceBaseProxy"/>.
/// </summary>
public class EntityServiceBaseTests
    : EntityServiceIntegrationTestsBase<Order, EntityServiceBaseProxy> {

    /// <summary>
    ///     <see cref="SandboxDatabase"/> testing data store manager.
    /// </summary>
    readonly StoreManager sandboxStoreManager;

    public EntityServiceBaseTests()
        : base(
                [
                    () => new SandboxDatabase()
                ]
            ) {


        sandboxStoreManager = new StoreManager(_testingStoreManager);
    }

    protected override EntityServiceBaseProxy ServiceFactory() {
        SandboxDatabase sandboxDatabase = new();

        IEntityServiceUtils entityServiceUtils = new EntityServiceUtils();

        IOrdersDepot ordersDepot = new OrdersDepot(sandboxDatabase, _disposer);


        return new EntityServiceBaseProxy(ordersDepot, entityServiceUtils);
    }

    protected override async Task<Order> DraftEntity(string entropy) {
        Customer customer = await sandboxStoreManager.Customer();

        return DraftUtils.Order(
                new Order {
                    Customer = customer,
                }
            );
    }
}
