using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Depots.ViewFilters;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Foundation_Core.Core.Utils;

using CSM_Server_Core.Abstractions.Bases;
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
    protected TEntity DraftEntity() {

        return RunEntityDraft(
                DraftEntity
            );
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

    /// <method>
    ///     <see cref="EntityServiceBase{TEntity, TDepot}.View(EntityServiceInput{ViewInput{TEntity}})"/>
    /// </method>
    /// <expectation>
    ///     A [View] is generated with no errors.
    /// </expectation>
    [Fact]
    public virtual async Task View_ComposesEntityView() {
        // Expectation.
        int sampleRange = 20;
        await Store(sampleRange, DraftEntity);

        ViewOutput<TEntity> viewOutput = await _service.View(
                new EntityServiceInput<ViewInput<TEntity>> {
                    Parameters = new() {
                        Retroactive = false,
                        Range = sampleRange,
                        Page = 1,
                    },
                }
            );

        Assert.Multiple(
            () => Assert.True(viewOutput.Pages > 0),
                () => Assert.True(viewOutput.Length > 0),
                () => Assert.Equal(1, viewOutput.Page),
                () => Assert.Equal(viewOutput.Length, viewOutput.Entities.Length)
            );
    }

    /// <method>
    ///     <see cref="EntityServiceBase{TEntity, TDepot}.View(EntityServiceInput{ViewInput{TEntity}})"/>
    /// </method>
    /// <expectation>
    ///     A [View] is generated with no errors and with entries filtered correctly by a test sampled name property. 
    /// </expectation>
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
            INamedEntity entity = (INamedEntity)RunEntityDraft(DraftEntity);
            entity.Name = $"{entity.Name}_{testKey}";

            entities.Add((TEntity)entity);
        }
        TEntity[] storedEntities = await Store([.. entities]);

        // Executing
        ViewOutput<TEntity> output = await _service.View(
                new EntityServiceInput<ViewInput<TEntity>> {
                    Parameters = new ViewInput<TEntity> {
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
                    },
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
}
