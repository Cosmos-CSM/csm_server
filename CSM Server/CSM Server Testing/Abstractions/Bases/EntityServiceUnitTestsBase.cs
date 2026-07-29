using CSM_Database_Core.Depots.Abstractions.Interfaces;
using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Server_Core.Abstractions.Bases;
using CSM_Server_Core.Abstractions.Interfaces;
using CSM_Server_Core.Core.Models;
using CSM_Server_Core.Core.Utils.Abstractions.Interfaces;

using Moq;

using Xunit;
using Xunit.Sdk;

namespace CSM_Server_Testing.Abstractions.Bases;

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
    ///     Drafts a <typeparamref name="TEntity"/> object for test purposes.
    /// </summary>
    /// <returns>
    ///     A drafted entity.
    /// </returns>
    protected abstract Task<TEntity> DraftEntity();

    /// <summary>
    ///     Generates a <typeparamref name="TService"/> instance that uses the given <paramref name="depotMock"/> as its default 
    ///     <see cref="IDepot{TEntity}"/> internal handler to mock operations.
    /// </summary>
    /// <param name="depotMock">
    ///     Service depot mocked instance.
    /// </param>
    /// <param name="entityServiceUtilsMock">
    ///     Entity service utils mocked instance.
    /// </param>
    /// <returns>
    ///     A <typeparamref name="TService"/> instance using the given <paramref name="depotMock"/>.
    /// </returns>
    /// <remarks>
    ///     This is only used by <see cref="EntityServiceUnitTestsBase{TEntity, TDepot, TService}"/> to test <see cref="EntityServiceBase{TEntity, TDepot}"/> methods,
    ///     if one of this base methods got overriden, the unit test must be overriden.
    /// </remarks>
    protected abstract Task<TService> ServiceFactory(TDepot depotMock, IEntityServiceUtils entityServiceUtilsMock);

    /// Method: 
    ///     <see cref="EntityServiceBase{TEntity, TDepot}.View(EntityServiceInput{ViewInput{TEntity}})"/>
    /// 
    /// Expectation: 
    ///     The [Entity Service Utils] are called to include relations, and the method correctly returns expected values.
    ///     
    [Fact]
    public virtual async Task View_GeneratesView() {
        // --> Expectation
        ViewInput<TEntity> expectation = new() {
            Page = 1,
            Range = 10,
            Retroactive = true,
        };

        // --> Setup
        (
            Mock<TDepot> depotMock,
            Mock<IEntityServiceUtils> entityServiceUtilsMock,
            TService service
        ) = await MockService();

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

        // --> Act.
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
        entityServiceUtilsMock.Verify(
                obj => obj.IncludeRelations<TEntity>(
                        It.IsAny<string[]>()
                    ),
                Times.Once()
            );
    }

    /// Method: 
    ///     <see cref="EntityServiceBase{TEntity, TDepot}.Create(TEntity[])"/>
    /// 
    /// Expectation:
    ///     The depot method got called once.
    [Fact]
    public virtual async Task Create_BatchCreated() {

        if (!typeof(TEntity).IsAssignableTo(typeof(INamedEntity))) {
            throw SkipException.ForSkip("This tests are intended to work only on INamedEntity, cause it uses [Name] property to match. Please implement your own for different entities");
        }

        // Expect
        TEntity[] entities = [
                await DraftEntity(),
                await DraftEntity(),
            ];

        // Setup
        (
            Mock<TDepot> depotMock,
            Mock<IEntityServiceUtils> entityServiceUtilsMock,
            TService service
        ) = await MockService();

        depotMock.Setup(
               obj => obj.Create(
                        It.IsAny<TEntity[]>(),
                        It.IsAny<bool>()
                   )
           )
           .Returns(
               async (TEntity[] entities, bool sync) => {

                   return new BatchOperationOutput<TEntity>(entities, []);
               }
           );

        // Act
        BatchOperationOutput<TEntity> output = await service.Create(entities);

        // Asserting
        depotMock.Verify(
                obj => obj.Create(
                        It.IsAny<TEntity[]>(),
                        It.IsAny<bool>()
                    ),
                Times.Once
            );

        Assert.NotEmpty(entities);
        Assert.Equal(entities.Length, output.SuccessesCount);
        Assert.All(
                output.Successes,
                (outputEntity) => {


                    Assert.Contains(
                            entities,
                            entity => ((INamedEntity)entity).Name == ((INamedEntity)outputEntity).Name
                        );
                }
            );
    }

    /// Method:
    ///     <see cref="EntityServiceBase{TEntity, TDepot}.Update(EntityServiceInput{UpdateInput{TEntity}})"/>
    /// 
    /// Expectation:
    ///     Update depot method is called once and entity service utilities include relations are called also once.
    [Fact]
    public virtual async Task Update_UpdatedEntity() {
        // Expectation
        TEntity entity = await DraftEntity();
        entity.Id = -100;

        // Setup
        (
            Mock<TDepot> depotMock,
            Mock<IEntityServiceUtils> entityServiceUtilsMock,
            TService service
        ) = await MockService();

        depotMock.Setup(
                obj => obj.Update(
                        It.IsAny<QueryInput<TEntity, UpdateInput<TEntity>>>()
                    )
            )
            .Returns(
                    async (QueryInput<TEntity, UpdateInput<TEntity>> queryInput) => {
                        return new UpdateOutput<TEntity> {
                            Original = queryInput.Parameters.Entity,
                            Updated = queryInput.Parameters.Entity
                        };
                    }
                );

        // Act
        UpdateOutput<TEntity> output = await service.Update(
                new EntityServiceInput<UpdateInput<TEntity>> {
                    Parameters = new UpdateInput<TEntity> {
                        Entity = entity
                    },
                    Relations = [],
                }
            );

        // Asserting.
        Assert.NotNull(output.Original);
        Assert.NotNull(output.Updated);
        Assert.Equal(entity.Id, output.Original.Id);
        Assert.Equal(entity.Id, output.Updated.Id);
    }

    /// Method:
    ///     <see cref="EntityServiceBase{TEntity, TDepot}.Delete(long)"/>
    /// 
    /// Expectaction:
    ///     Delete depot method is called from id value.
    [Fact]
    public virtual async Task Delete_DeletedEntityById() {
        // Expect.
        long id = -100;

        // Setup 
        (
            Mock<TDepot> depotMock,
            Mock<IEntityServiceUtils> entityServiceUtilsMock,
            TService service
        ) = await MockService();

        depotMock.Setup(
                obj => obj.Delete(
                        It.Is<long>(
                                paramValue => paramValue == id
                            )
                    )
            )
            .Returns(
                async (long id) => {
                    return new TEntity {
                        Id = id
                    };
                }
            );

        // Act
        TEntity deletedEntityObj = await service.Delete(id);

        // Asserting
        Assert.NotNull(deletedEntityObj);
        Assert.Equal(id, deletedEntityObj.Id);
        depotMock.Verify(
                obj => obj.Delete(
                        It.Is<long>(
                                paramVal => paramVal == id
                            )
                    ),
                Times.Once
            );
    }

    /// <summary>
    ///     Mocks service dependencies and generates the service instance.
    /// </summary>
    /// <returns>
    ///     Mocks and service instance.
    /// </returns>
    async Task<(Mock<TDepot>, Mock<IEntityServiceUtils>, TService)> MockService() {
        Mock<TDepot> depotMock = new();
        Mock<IEntityServiceUtils> entityServiceUtilsMock = new();
        TService service = await ServiceFactory(depotMock.Object, entityServiceUtilsMock.Object);


        return (depotMock, entityServiceUtilsMock, service);
    }
}