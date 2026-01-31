using CSM_Database_Core.Depots.Abstractions.Interfaces;
using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Server_Core.Abstractions.Bases;
using CSM_Server_Core.Abstractions.Interfaces;

using Moq;

using Xunit;

namespace CSM_Server_Core_Testing.Abstractions.Bases;



/// <summary>
///     Represents a <see cref="IService"/> tests class.
/// </summary>
/// <typeparam name="TService">
///     Type of the service class to be tested.
/// </typeparam>
public abstract class ServiceUnitTestsBase<TService>
    where TService : ServiceBase<IEntity, IDepot<IEntity>> {

    /// <summary>
    ///     
    /// </summary>
    /// <returns></returns>
    protected abstract ( TService, Mock<IDepot<IEntity>>) MockFactory();

    /// <summary>
    ///     Tests that the method <see cref="IServiceView{TEntity}.View(CSM_Database_Core.Depots.Models.QueryInput{TEntity, CSM_Database_Core.Depots.Models.ViewInput{TEntity}})"/> correctly generates a View object.
    /// </summary>
    [Fact]
    public virtual async Task View_GeneratesView() {
        // --> Mocking setup
        ( TService service, Mock<IDepot<IEntity>> depotMock) = MockFactory();

        depotMock.Setup(
                obj => obj.View(It.IsAny<QueryInput<IEntity, ViewInput<IEntity>>>())
            )
            .Returns(
                (QueryInput<IEntity, ViewInput<IEntity>> input) => {

                    return new ViewOutput<IEntity> {
                        Count = input.Parameters.Range,
                        Page = input.Parameters.Page,
                        Pages = 1,
                        Entities = [],
                    };
                }
            );

        // --> Executing operation.
        ViewOutput<IEntity> viewOutput = await service.View(
                new QueryInput<IEntity, ViewInput<IEntity>> {
                    Parameters = new ViewInput<IEntity> {
                        Page = 1,
                        Range = 10,
                        Retroactive = true,
                    }
                }
            );


        // --> Asserting
        Assert.Equal(1, viewOutput.Page);
    }
}
