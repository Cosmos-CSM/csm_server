using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Depots.ViewFilters;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Foundation_Core.Core.Utils;

using CSM_Server_Core.Abstractions.Interfaces;

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
/// <typeparam name="TService">
///     Type of the <see cref="IService"/> implementation to be tested.
/// </typeparam>
/// <typeparam name="TEntity">
///     Type of the <see cref="IEntity"/> that is used by the <typeparamref name="TService"/>.
/// </typeparam>
public abstract class ServiceIntegrationTestsBase<TService, TEntity>
    : ServiceIntegrationTestsBase<TService>
    where TService : IService<TEntity>
    where TEntity : class, IEntity {

    /// <inheritdoc/>
    public ServiceIntegrationTestsBase(params DatabaseFactory[] databaseFactories)
        : base(databaseFactories) {
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
    protected abstract TEntity DraftEntity(string entropy);

    /// <summary>
    ///     Tests that <see cref="IServiceView{TEntity}.View(ViewInput{TEntity})"/> composes an entity view.
    /// </summary>
    [Fact]
    public virtual async Task View_ComposesEntityView() {
        // Expectation.
        int sampleRange = 20;
        await Store(sampleRange, DraftEntity);

        ViewOutput<TEntity> viewOutput = await _service.View(
                new() {
                    Retroactive = false,
                    Range = sampleRange,
                    Page = 1,
                }
            );

        Assert.Multiple(
            () => Assert.True(viewOutput.Pages > 0),
                () => Assert.True(viewOutput.Length > 0),
                () => Assert.Equal(1, viewOutput.Page),
                () => Assert.Equal(viewOutput.Length, viewOutput.Entities.Length)
            );
    }

    /// <summary>
    ///     Tests that <see cref="IServiceView{TEntity}.View(ViewInput{TEntity})"/> composes an entity view filtered by a key name contains.
    /// </summary>
    [Fact]
    public virtual async Task View_ComposeEntityView_NameFilteredView() {

        if (!typeof(TEntity).IsAssignableTo(typeof(INamedEntity))) {
            throw SkipException.ForSkip("Test only supported for Named Entities.");
        }

        // Expectation
        string testKey = RandomUtils.String(8);

        // Sampling
        List<TEntity> entities = [];
        for (int i = 0; i <= 20; i++) {
            INamedEntity entity = (INamedEntity)RunEntityFactory(DraftEntity);
            entity.Name = $"{entity.Name}_{testKey}";

            entities.Add((TEntity)entity);
        }
        TEntity[] storedEntities = await Store([.. entities]);

        // Executing
        ViewOutput<TEntity> output = await _service.View(
                new ViewInput<TEntity> {
                    Retroactive = true,
                    Range = 20,
                    Page = 1,
                    Filters = [
                            new ViewFilterProperty<TEntity> {
                                    Operator = ViewFilterOperators.CONTAINS,
                                    Property = nameof(INamedEntity.Name),
                                    Value = testKey
                                }
                        ]
                }
            );

        // Asserting
        Assert.Equal(20, output.Length);
        Assert.All(
                output.Entities,
                entity => {

                    INamedEntity namedEntity = (INamedEntity)entity;
                    Assert.Multiple(
                            [
                                () => Assert.Contains(testKey, namedEntity.Name),
                                () => Assert.Contains(
                                            storedEntities,
                                               (storedEntity) => ((INamedEntity)storedEntity).Name == namedEntity.Name
                                        ),
                            ]
                        );
                }
            );

    }

    /// <summary>
    ///     Tests that <see cref="IServiceCreate{TEntity}.Create(TEntity)"/> successfuly creates a single entity.
    /// </summary>
    [Fact]
    public async Task Create_SingleEntity() {
        TEntity sampleEntity = RunEntityFactory(DraftEntity);
        TEntity createdEntity = await _service.Create(sampleEntity);

        Assert.True(
                createdEntity.Id > 0,
                $"Created entity Id must be greater than 0"
            );
    }
}
