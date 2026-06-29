using CSM_Database_Core.Abstractions.Interfaces;
using CSM_Database_Core.Depots.Abstractions.Interfaces;
using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Server_Core.Abstractions.Bases;
using CSM_Server_Core.Abstractions.Interfaces;
using CSM_Server_Core.Core.Models;

using Moq;

using Xunit;

namespace CSM_Server_Core_Testing.Abstractions.Bases;

/// <summary>
///     Represents a <see cref="IService"/> tests class.
/// </summary>
/// <typeparam name="TEntity">
///     Type of the <see cref="IEntity"/> that the <typeparamref name="TService"/> handles.
/// </typeparam>
/// <typeparam name="TDepot">
///     Type of the <see cref="IDepot{TEntity}"/> that the <typeparamref name="TService"/> uses as main depot.
/// </typeparam>
/// <typeparam name="TService">
///     Type of the service class to be tested.
/// </typeparam>
public abstract class EntityServiceUnitTestsBase<TEntity, TDepot, TService>
    where TDepot : class, IDepot<TEntity>
    where TService : EntityServiceBase<TEntity, TDepot>
    where TEntity : class, IEntity, new() {

    /// <summary>
    ///     Generates a <typeparamref name="TService"/> instance that uses the given <paramref name="depotMock"/> as its default 
    ///     <see cref="IDepot{TEntity}"/> internal handler to mock operations.
    /// </summary>
    /// <returns>
    ///     A <typeparamref name="TService"/> instance using the given <paramref name="depotMock"/>.
    /// </returns>
    /// <remarks>
    ///     This is only used by <see cref="EntityServiceUnitTestsBase{TEntity, TDepot, TService}"/> to test <see cref="EntityServiceBase{TEntity, TDepot}"/> methods,
    ///     if one of this base methods got overriden, the unit test must be overriden.
    /// </remarks>
    protected abstract TService ServiceFactory(TDepot depotMock);

    /// <method>
    ///     <see cref="EntityServiceBase{TEntity, TDepot}.View(EntityServiceInput{ViewInput{TEntity}})"/>
    /// </method>
    /// <expectation>
    ///     
    /// </expectation>
    [Fact]
    public virtual async Task View_GeneratesView() {

        // --> Expectation
        ViewInput<TEntity> expectation = new() {
            Page = 1,
            Range = 10,
            Retroactive = true,
        };

        // --> Mocking setup
        Mock<TDepot> depotMock = new();
        TService service = ServiceFactory(depotMock.Object);

        depotMock.Setup(
                obj => obj.View(It.IsAny<QueryInput<TEntity, ViewInput<TEntity>>>())
            )
            .Returns(
                async (QueryInput<TEntity, ViewInput<TEntity>> input) => {

                    return new ViewOutput<TEntity> {
                        Count = input.Parameters.Range,
                        Page = input.Parameters.Page,
                        Pages = 1,
                        Entities = [],
                    };
                }
            );

        // --> Executing operation.
        ViewOutput<TEntity> viewOutput = await service.View(
                new EntityServiceInput<ViewInput<TEntity>> {
                    Parameters = expectation,
                }
            );


        // --> Asserting
        Assert.Multiple(
                [
                    () => Assert.Equal(expectation.Range, viewOutput.Count),
                    () => Assert.Equal(expectation.Page, viewOutput.Page),
                    () => Assert.Equal(1, viewOutput.Pages),
                    () => Assert.Empty(viewOutput.Entities)
                ]
            );
        depotMock.Verify(
                obj => obj.View(It.IsAny<QueryInput<TEntity, ViewInput<TEntity>>>()),
                Times.Once()
            );
    }

}