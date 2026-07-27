using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Depots.ViewFilters;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Database_Testing.Managers;

using CSM_Foundation_Core.Core.Utils;

using CSM_Server_Core.Abstractions.Interfaces;
using CSM_Server_Core.Core.Models;

using CSM_Server_Core_Testing.Disposition.Abstractions.Bases;

using Xunit;
using Xunit.Sdk;

namespace CSM_Server_Core_Testing.Abstractions.Bases;

/// <summary>
///     Represents an integration tests for a <see cref="IService"/> implementation.
/// </summary>
/// <typeparam name="TService">
///     Type of the <see cref="IService"/> implementation to be tested.
/// </typeparam>
public abstract class ServiceIntegrationTestsBase<TService>
    : DataHandlerTestsBase
    where TService : IService {

    /// <summary>
    ///     Service instance to qualify operations.
    /// </summary>
    protected readonly TService _service;

    /// <inheritdoc/>
    public ServiceIntegrationTestsBase(params DatabaseFactory[] databaseFactories)
        : base(databaseFactories) {

        _service = ServiceFactory();
    }


    /// <summary>
    ///     Creates a new <typeparamref name="TService"/> instance that is <see cref="IService"/> 
    ///     implementation to be tested.
    /// </summary>
    /// <returns>
    ///     A new <typeparamref name="TService"/> instance.
    /// </returns>
    protected abstract TService ServiceFactory();
}

/// <inheritdoc cref="ServiceIntegrationTestsBase{TService}"/>
/// <typeparam name="TEntity">
///     Type of the <see cref="IEntity"/> that is used by the <typeparamref name="TEntityService"/>.
/// </typeparam>
/// <typeparam name="TEntityService">
///     Type of the <see cref="IService"/> implementation to be tested.
/// </typeparam>
public abstract class EntityServiceIntegrationTestsBase<TEntity, TEntityService>
    : ServiceIntegrationTestsBase<TEntityService>
    where TEntityService : IEntityService<TEntity>
    where TEntity : class, IEntity, new() {

    /// <inheritdoc/>
    public EntityServiceIntegrationTestsBase(params DatabaseFactory[] databaseFactories)
        : base(databaseFactories) {
    }

    /// <summary>
    ///     Runs <see cref="DraftEntity(string)"/> giving a random entropy value.
    /// </summary>
    /// <returns>
    ///     A drafter <typeparamref name="TEntity"/> object.
    /// </returns>
    protected async Task<TEntity> DraftEntity() {
        TEntity draft = await DraftEntity(RandomUtils.String(16));

        return await _testingStoreManager.Store(draft);
    }

    /// <summary>
    ///     Creates a new <typeparamref name="TEntity"/> draft instance. 
    /// </summary>
    /// <returns>
    ///     A new <typeparamref name="TEntity"/> data.
    /// </returns>
    /// <remarks>
    ///     This data is not saved in live data stores is only sample data.
    /// </remarks>
    protected abstract Task<TEntity> DraftEntity(string entropy);

    /// Use Case:
    ///     - Stores an entity.
    ///     - Calls delete operation giving entity id.
    /// Expectation:
    ///     - Entity deleted is correctly the given id one.
    ///     - Entity in fact exist no more in database.
    [Fact]
    public virtual async Task Delete_EntityDeleted() {
        // Setup.
        TEntity entity = await _testingStoreManager.Store(
                (entropy) => {
                    return DraftEntity(entropy).GetAwaiter().GetResult();
                }
            );

        // Act.
        TEntity deletedEntity = await _service.Delete(entity.Id);

        // Assert.
        Assert.NotNull(deletedEntity);
        Assert.Equal(entity.Id, deletedEntity.Id);

    }

    /// Use Case: 
    ///     - We store 20 (sample range) entities in the database.
    ///     - Generate a view with following configuration:
    ///         - Page: 1
    ///         - Range: 20
    ///         - Retroactive: false
    /// Expectation:
    ///     - We get and exactly 20 items view with page 1.
    [Fact]
    public virtual async Task View_SimpleView() {
        // Expect.
        int sampleRange = 20;
        await _testingStoreManager.Store(
                sampleRange,
                (entropy) => {
                    return DraftEntity(entropy).GetAwaiter().GetResult();
                }
            );

        // Act.
        ViewOutput<TEntity> viewOutput = await _service.View(
                new EntityServiceInput<ViewInput<TEntity>> {
                    Parameters = new() {
                        Retroactive = false,
                        Range = sampleRange,
                        Page = 1,
                    },
                }
            );

        // Assert.
        Assert.Multiple(
            () => Assert.True(viewOutput.Pages > 0),
                () => Assert.True(viewOutput.Length > 0),
                () => Assert.Equal(1, viewOutput.Page),
                () => Assert.Equal(viewOutput.Length, viewOutput.Entities.Length)
            );
    }

    /// <see cref="INamedEntity"/> EXCLUSIVE TEST, override for extended behavior.
    /// Use Case: 
    ///     - Stores 20 entities in database with an specific token.
    ///     - Generates a view with following configruations:
    ///         - Page: 1
    ///         - Range: 20
    ///         - Retroative: true
    ///         - Filters: ViewFilterProperty[CONTAINS(NAME => NAME == randomToken)]
    /// Expectation:
    ///     - We get exactly 20 items view.
    ///     - All view items have the filtered expected token.
    [Fact]
    public virtual async Task View_FilteredView() {

        if (!typeof(TEntity).IsAssignableTo(typeof(INamedEntity))) {
            throw SkipException.ForSkip("Test only supported for Named Entities.");
        }

        // Expect.
        string expNameToken = RandomUtils.String(8);
        int expRange = 20;

        // Setup.
        List<TEntity> entities = [];
        for (int i = 0; i <= expRange; i++) {
            INamedEntity entity = (INamedEntity)TestingStoreManager.RunEntityFactory(
                    (entropy) => DraftEntity(entropy).GetAwaiter().GetResult()
                );

            entity.Name = $"{entity.Name}_{expNameToken}";

            entities.Add((TEntity)entity);
        }
        TEntity[] storedEntities = await _testingStoreManager.Store([.. entities]);

        // Act.
        ViewOutput<TEntity> output = await _service.View(
                new EntityServiceInput<ViewInput<TEntity>> {
                    Parameters = new ViewInput<TEntity> {
                        Page = 1,
                        Range = expRange,
                        Retroactive = true,
                        Filters = [
                            new ViewFilterProperty<TEntity> {
                                    Operator = ViewFilterOperators.CONTAINS,
                                    Property = nameof(INamedEntity.Name),
                                    Value = expNameToken
                                }
                        ]
                    },
                }
            );

        // Assert.
        Assert.Equal(expRange, output.Length);
        Assert.All(
                output.Entities,
                entity => {

                    INamedEntity namedEntity = (INamedEntity)entity;
                    Assert.Multiple(
                            [
                                () => Assert.Contains(expNameToken, namedEntity.Name),
                                () => Assert.Contains(
                                            storedEntities,
                                               (storedEntity) => ((INamedEntity)storedEntity).Name == namedEntity.Name
                                        ),
                            ]
                        );
                }
            );
    }

    /// Use Case:
    ///     - Drafts 20 entities.
    ///     - Calls create service operation.
    /// Expectation:
    ///     - Entities got created with a valid generated id value.
    [Fact]
    public virtual async Task Create_BatchCreation() {
        // Expect.
        int expRange = 20;
        TEntity[] expEntities = [];

        // Setup.
        for (int i = 0; i < expRange; i++) {
            TEntity draftEntity = TestingStoreManager.RunEntityFactory(
                    (string entropy) => {
                        return DraftEntity(entropy).GetAwaiter().GetResult();
                    }
                );

            expEntities = [
                    ..expEntities, draftEntity
                ];
        }

        // Act.
        BatchOperationOutput<TEntity> createOutput = await _service.Create(expEntities);

        // Assert.
        Assert.False(createOutput.FullFailed);
        Assert.False(createOutput.Failed);
        Assert.Empty(createOutput.Failures);
        Assert.Equal(0, createOutput.FailuresCount);

        Assert.Equal(expRange, createOutput.OperationsCount);
        Assert.Equal(expRange, createOutput.SuccessesCount);
        Assert.Equal(expRange, createOutput.Successes.Length);

        Assert.All(
                createOutput.Successes,
                (createdEntity) => {

                    Assert.NotEqual(0, createdEntity.Id);
                }
            );
    }

    /// <see cref="INamedEntity"/> EXCLUSIVE TEST, override for extended behavior.
    /// Use Case:
    ///     - We store an entity with an specific description value.
    ///     - Update description value for a new one.
    ///     - Call update service operation to save new description.
    /// Expectation:
    ///     - Updates successes,
    ///     - Original entity matches previous values and entity correct id.
    ///     - Updated entity matches updated values and entity correct id.
    [Fact]
    public virtual async Task Update_EntityUpdated() {
        // Restrict
        if (!typeof(TEntity).IsAssignableTo(typeof(INamedEntity))) {
            throw SkipException.ForSkip("Test only supported for Named Entities.");
        }

        // Expect
        string expNewDescription = "new_description";
        string expOldDescription = "old_description";


        // Setup
        INamedEntity entity = (INamedEntity)TestingStoreManager.RunEntityFactory(
                (entropy) => {
                    return DraftEntity(entropy).GetAwaiter().GetResult();
                }
            );

        entity.Description = expOldDescription;
        entity = (INamedEntity)await _testingStoreManager.Store((TEntity)entity);

        // Act
        entity.Description = expNewDescription;
        UpdateOutput<TEntity> output = await _service.Update(
                new EntityServiceInput<UpdateInput<TEntity>> {
                    Parameters = new UpdateInput<TEntity> {
                        Entity = (TEntity)entity,
                    }
                }
            );

        // Assert.
        INamedEntity? original = (INamedEntity?)output.Original;
        Assert.NotNull(original);
        Assert.Equal(entity.Id, original.Id);
        Assert.Equal(entity.Name, original.Name);
        Assert.Equal(expOldDescription, original.Description);

        INamedEntity? updated = (INamedEntity?)output.Updated;
        Assert.NotNull(updated);
        Assert.Equal(entity.Id, updated.Id);
        Assert.Equal(entity.Name, updated.Name);
        Assert.Equal(expNewDescription, updated.Description);
    }
}
