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
/// <typeparam name="TEntity">
///     Type of the <see cref="IEntity"/> that the <typeparamref name="TService"/> handles.
/// </typeparam>
/// <typeparam name="TDepot">
///     Type of the <see cref="IDepot{TEntity}"/> that the <typeparamref name="TService"/> uses as main depot.
/// </typeparam>
/// <typeparam name="TService">
///     Type of the service class to be tested.
/// </typeparam>
public abstract class ServiceUnitTestsBase<TEntity, TDepot, TService>
    where TDepot : class, IDepot<TEntity>
    where TService : ServiceBase<TEntity, TDepot>
    where TEntity : class, IEntity, new() {

    /// <summary>
    ///     Generates a <typeparamref name="TService"/> instance that uses the given <paramref name="depotMock"/> as its default 
    ///     <see cref="IDepot{TEntity}"/> internal handler to mock operations.
    /// </summary>
    /// <returns>
    ///     A <typeparamref name="TService"/> instance using the given <paramref name="depotMock"/>.
    /// </returns>
    /// <remarks>
    ///     This is only used by <see cref="ServiceUnitTestsBase{TEntity, TDepot, TService}"/> to test <see cref="ServiceBase{TEntity, TDepot}"/> methods,
    ///     if one of this base methods got overriden, the unit test must be overriden.
    /// </remarks>
    protected abstract TService ServiceFactory(TDepot depotMock);

    /// <summary>
    ///     Tests that the method <see cref="IServiceView{TEntity}.View( ViewInput{TEntity})"/> correctly generates a View object.
    /// </summary>
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
        ViewOutput<TEntity> viewOutput = await service.View(expectation);


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

    /// <summary>
    ///     Tests that the method <see cref="IServiceCreate{TEntity}.Create(TEntity)"/> correctly creates a single entity.
    /// </summary>
    [Fact]
    public virtual async Task Create_SingleEntityCreated() {
        // --> Expectation.
        TEntity entity = new() {
            Id = 1,
            Timestamp = DateTime.UtcNow,
        };


        // --> Mocking setup
        Mock<TDepot> depotMock = new();
        TService service = ServiceFactory(depotMock.Object);

        depotMock.Setup(
                obj => obj.Create(It.IsAny<TEntity>())
            ).Callback(
                (TEntity entity) => {
                    return entity;
                }
            );


        // --> Executing.
        TEntity createdEntity = await service.Create(entity);


        // --> Asserting.
        Assert.Multiple(
                [
                    () => Assert.Equal(entity.Id, createdEntity.Id),
                    () => Assert.Equal (entity.Timestamp, createdEntity.Timestamp)
                ]
            );
        depotMock.Verify(
                obj => obj.Create(It.IsAny<TEntity>()),
                Times.Once()
            );
    }

    /// <summary>
    ///     Tests that the method <see cref="IServiceCreate{TEntity}.Create(TEntity[], bool)"/> correctly creates a synchronously multiple entities
    /// </summary>
    [Fact]
    public virtual async Task Create_MultipleEntitiesCreated_Sync() {
        // --> Expectations.
        TEntity[] expectedEntities = [
                new TEntity {
                        Id = 1,
                        Timestamp = DateTime.UtcNow,
                    },
                new TEntity {
                        Id = 2,
                        Timestamp = DateTime.UtcNow,
                    },
            ];


        // --> Mocking setup.
        Mock<TDepot> depotMock = new();
        TService service = ServiceFactory(depotMock.Object);

        depotMock.Setup(
                obj => obj.Create(
                        It.IsAny<TEntity[]>(),
                        It.Is<bool>(true, EqualityComparer<bool>.Default)
                    )
            ).Callback(
                (TEntity[] createdEntities, bool sync) => {
                    return new BatchOperationOutput<TEntity>(createdEntities, []);
                }
            );

        // --> Executing.
        BatchOperationOutput<TEntity> output = await service.Create(expectedEntities, true);

        // --> Asserting.
        Assert.False(output.Failed);
        Assert.Empty(output.Failures);
        Assert.Equal(0, output.FailuresCount);
        Assert.NotEmpty(output.Successes);
        Assert.Equal(expectedEntities.Length, output.SuccessesCount);
        Assert.Equal(expectedEntities.Length, output.OperationsCount);
        Assert.All(
                output.Successes,
                (createdEntity, index) => {

                    TEntity expectedEntity = expectedEntities[index];

                    Assert.Multiple(
                            [
                                () => Assert.Equal(expectedEntity.Id, createdEntity.Id),
                                () => Assert.Equal(expectedEntity.Timestamp, createdEntity.Timestamp),
                            ]
                        );
                }
            );
        depotMock.Verify(
                obj => obj.Create(
                        It.IsAny<TEntity[]>(),
                        It.Is<bool>(true, EqualityComparer<bool>.Default)
                    ),
                Times.Once()
            );
    }

    /// <summary>
    ///     Tests that the method <see cref="IServiceCreate{TEntity}.Create(TEntity[], bool)"/> correctly creates a not synchronously multiple entities
    /// </summary>
    [Fact]
    public virtual async Task Create_MultipleEntitiesCreated_NotSync() {
        // --> Expectations.
        TEntity[] expectedEntities = [
                new TEntity {
                        Id = 1,
                        Timestamp = DateTime.UtcNow,
                    },
                new TEntity {
                        Id = 2,
                        Timestamp = DateTime.UtcNow,
                    },
            ];


        // --> Mocking setup.
        Mock<TDepot> depotMock = new();
        TService service = ServiceFactory(depotMock.Object);

        depotMock.Setup(
                obj => obj.Create(
                        It.IsAny<TEntity[]>(),
                        It.Is<bool>(false, EqualityComparer<bool>.Default)
                    )
            ).Callback(
                (TEntity[] createdEntities, bool sync) => {
                    return new BatchOperationOutput<TEntity>(createdEntities, []);
                }
            );

        // --> Executing.
        BatchOperationOutput<TEntity> output = await service.Create(expectedEntities, false);

        // --> Asserting.

        Assert.False(output.Failed);
        Assert.Empty(output.Failures);
        Assert.Equal(0, output.FailuresCount);
        Assert.NotEmpty(output.Successes);
        Assert.Equal(expectedEntities.Length, output.SuccessesCount);
        Assert.Equal(expectedEntities.Length, output.OperationsCount);
        Assert.All(
                output.Successes,
                (createdEntity, index) => {

                    TEntity expectedEntity = expectedEntities[index];

                    Assert.Multiple(
                            [
                                () => Assert.Equal(expectedEntity.Id, createdEntity.Id),
                                () => Assert.Equal(expectedEntity.Timestamp, createdEntity.Timestamp),
                            ]
                        );
                }
            );
        depotMock.Verify(
                obj => obj.Create(
                        It.IsAny<TEntity[]>(),
                        It.Is<bool>(false, EqualityComparer<bool>.Default)
                    ),
                Times.Once()
            );
    }

    /// <summary>
    ///     Tests that the method <see cref="IServiceUpdate{TEntity}.Update(UpdateInput{TEntity})"/> correctly updates the given entities when create enabled.
    /// </summary>
    [Fact]
    public virtual async Task Update_UpdatesEntity_Create() {
        // --> Expectation
        UpdateInput<TEntity> expectation = new() {
            Entity = new TEntity {
                Id = 1,
                Timestamp = DateTime.UtcNow,
            },
            Create = true
        };

        // --> Mockin setup
        Mock<TDepot> depotMock = new();
        TService service = ServiceFactory(depotMock.Object);
        depotMock.Setup(
                obj => obj.Update(It.IsAny<QueryInput<TEntity, UpdateInput<TEntity>>>())
            ).Callback(
                (QueryInput<TEntity, UpdateInput<TEntity>> input) => {
                    if (input.Parameters.Create) {
                        return new UpdateOutput<TEntity> {
                            Original = null,
                            Updated = input.Parameters.Entity,
                        };
                    }

                    return null;
                }
            );

        // --> Executing.
        UpdateOutput<TEntity> output = await service.Update(expectation);

        // --> Asserting.
        Assert.Null(output.Original);
        Assert.Multiple(
                [
                    () => Assert.Equal(expectation.Entity.Id, output.Updated.Id),
                    () => Assert.Equal(expectation.Entity.Timestamp, output.Updated.Timestamp),
                ]
            );
        depotMock.Verify(
                obj => obj.Update(It.IsAny<QueryInput<TEntity, UpdateInput<TEntity>>>()),
                Times.Once()
            );
    }

    /// <summary>
    ///     Tests that the method <see cref="IServiceUpdate{TEntity}.Update(UpdateInput{TEntity})"/> correctly updates the given entities when create disabled.
    /// </summary>
    [Fact]
    public virtual async Task Update_UpdatesEntity_NotCreate() {
        // --> Expectation
        UpdateInput<TEntity> expectation = new() {
            Entity = new TEntity {
                Id = 1,
                Timestamp = DateTime.UtcNow,
            },
            Create = false
        };

        // --> Mockin setup
        Mock<TDepot> depotMock = new();
        TService service = ServiceFactory(depotMock.Object);
        depotMock.Setup(
                obj => obj.Update(It.IsAny<QueryInput<TEntity, UpdateInput<TEntity>>>())
            ).Callback(
                (QueryInput<TEntity, UpdateInput<TEntity>> input) => {
                    if (!input.Parameters.Create) {
                        return new UpdateOutput<TEntity> {
                            Original = new TEntity {
                                Id = 1,
                                Timestamp = DateTime.UtcNow.AddDays(-10),
                            },
                            Updated = input.Parameters.Entity,
                        };
                    }

                    return null;
                }
            );

        // --> Executing.
        UpdateOutput<TEntity> output = await service.Update(expectation);

        // --> Asserting.
        Assert.NotNull(output.Original);
        Assert.Multiple(
                [
                    () => Assert.Equal(expectation.Entity.Id, output.Updated.Id),
                    () => Assert.Equal(expectation.Entity.Timestamp, output.Updated.Timestamp),
                ]
            );
        depotMock.Verify(
                obj => obj.Update(It.IsAny<QueryInput<TEntity, UpdateInput<TEntity>>>()),
                Times.Once()
            );
    }

    /// <summary>
    ///     Tests that the method <see cref="IServiceDelete{TEntity}.Delete(long)"/> correctly deletes the given entity with the id.
    /// </summary>
    [Fact]
    public virtual async Task Delete_DeletesEntity_FromId() {
        // --> Expectation
        TEntity expectation = new() {
            Id = 1,
            Timestamp = DateTime.UtcNow,
        };

        // --> Mocking setup
        Mock<TDepot> depotMock = new();
        TService service = ServiceFactory(depotMock.Object);

        depotMock.Setup(
                obj => obj.Delete(
                        It.Is<long>(id => id == expectation.Id)
                    )
            ).Returns(
                () => {
                    return expectation;
                }
            );

        // --> Executing.
        TEntity deletedEntity = await service.Delete(expectation.Id);

        // --> Asserting.
        Assert.NotNull(deletedEntity);
        Assert.Multiple(
                [
                    () => Assert.Equal(expectation.Id, deletedEntity.Id),
                    () => Assert.Equal(expectation.Timestamp, deletedEntity.Timestamp),
                ]
            );
        depotMock.Verify(
                obj => obj.Delete(
                        It.Is<long>(id => id == expectation.Id)
                    ),
                Times.Once()
            );
    }

    /// <summary>
    ///     Tests that the method <see cref="IServiceDelete{TEntity}.Delete(TEntity)"/> correctly deletes the given entity with the id.
    /// </summary>
    [Fact]
    public virtual async Task Delete_DeletesEntity() {
        // --> Expectation
        TEntity expectation = new() {
            Id = 1,
            Timestamp = DateTime.UtcNow,
        };

        // --> Mocking setup
        Mock<TDepot> depotMock = new();
        TService service = ServiceFactory(depotMock.Object);

        depotMock.Setup(
                obj => obj.Delete(
                        It.IsAny<TEntity>()
                    )
            ).Returns(
                () => {
                    return expectation;
                }
            );

        // --> Executing.
        TEntity deletedEntity = await service.Delete(expectation);

        // --> Asserting.
        Assert.NotNull(deletedEntity);
        Assert.Multiple(
                [
                    () => Assert.Equal(expectation.Id, deletedEntity.Id),
                    () => Assert.Equal(expectation.Timestamp, deletedEntity.Timestamp),
                ]
            );
        depotMock.Verify(
                obj => obj.Delete(It.IsAny<TEntity>()),
                Times.Once()
            );
    }

    /// <summary>
    ///     Tests that the method <see cref="IServiceDelete{TEntity}.Delete(long[])"/> correctly deletes the given entity with the id.
    /// </summary>
    [Fact]
    public virtual async Task Delete_DeletesEntities_FromIds() {
        // --> Expectation
        long[] idsToRemove = [1, 2, 3];

        // --> Mocking setup
        Mock<TDepot> depotMock = new();
        TService service = ServiceFactory(depotMock.Object);

        depotMock.Setup(
                obj => obj.Delete(
                        It.IsAny<long[]>()
                    )
            ).Returns(
                (long[] ids) => {
                    return new BatchOperationOutput<TEntity>(
                            [
                                ..ids.Select(
                                    id => new TEntity {
                                        Id = id,
                                        Timestamp = DateTime.UtcNow
                                    }
                                )
                            ],
                            []
                        );
                }
            );

        // --> Executing.
        BatchOperationOutput<TEntity> output = await service.Delete(idsToRemove);

        // --> Asserting.
        Assert.False(output.Failed);
        Assert.Empty(output.Failures);
        Assert.Equal(0, output.FailuresCount);
        Assert.NotEmpty(output.Successes);
        Assert.Equal(idsToRemove.Length, output.SuccessesCount);
        Assert.Equal(idsToRemove.Length, output.OperationsCount);
        Assert.All(
                output.Successes,
                (deletedEntity, index) => {

                    long expectedId = idsToRemove[index];

                    Assert.Multiple(
                            [
                                () => Assert.Equal(expectedId, deletedEntity.Id),
                            ]
                        );
                }
            );
        depotMock.Verify(
                obj => obj.Delete(
                        It.IsAny<long[]>()
                    ),
                Times.Once()
            );
    }

    /// <summary>
    ///     Tests that the method <see cref="IServiceDelete{TEntity}.Delete(TEntity[])"/> correctly deletes the given entity with the id.
    /// </summary>
    [Fact]
    public virtual async Task Delete_DeletesEntities() {
        // --> Expectation
        TEntity[] entitesToRemove = [
                new TEntity {
                        Id = 1,
                        Timestamp = DateTime.UtcNow
                    },
                new TEntity {
                        Id = 2,
                        Timestamp = DateTime.UtcNow
                    },
                new TEntity {
                        Id = 3,
                        Timestamp = DateTime.UtcNow
                    },
            ];

        // --> Mocking setup
        Mock<TDepot> depotMock = new();
        TService service = ServiceFactory(depotMock.Object);

        depotMock.Setup(
                obj => obj.Delete(
                        It.IsAny<TEntity[]>()
                    )
            ).Returns(
                (TEntity[] entities) => {
                    return new BatchOperationOutput<TEntity>(
                            entities,
                            []
                        );
                }
            );

        // --> Executing.
        BatchOperationOutput<TEntity> output = await service.Delete(entitesToRemove);

        // --> Asserting.
        Assert.False(output.Failed);
        Assert.Empty(output.Failures);
        Assert.Equal(0, output.FailuresCount);
        Assert.NotEmpty(output.Successes);
        Assert.Equal(entitesToRemove.Length, output.SuccessesCount);
        Assert.Equal(entitesToRemove.Length, output.OperationsCount);
        Assert.All(
                output.Successes,
                (deletedEntity, index) => {

                    TEntity expectedEntity = entitesToRemove[index];

                    Assert.Multiple(
                            [
                                () => Assert.Equal(expectedEntity.Id, deletedEntity.Id),
                                () => Assert.Equal(expectedEntity.Timestamp, deletedEntity.Timestamp)
                            ]
                        );
                }
            );
        depotMock.Verify(
                obj => obj.Delete(
                        It.IsAny<TEntity[]>()
                    ),
                Times.Once()
            );
    }
}