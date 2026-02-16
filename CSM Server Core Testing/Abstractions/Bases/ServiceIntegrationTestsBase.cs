using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Server_Core.Abstractions.Interfaces;

using CSM_Server_Core_Testing.Disposition.Abstractions.Bases;

using Xunit;

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
    ///     Tests that <see cref="IServiceView{TEntity}.View(ViewInput{TEntity})"/> composes an entty view.
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
