using CSM_Database_Core.Depots.Models;
using CSM_Database_Core.Entities.Abstractions.Interfaces;

using CSM_Server_Core.Abstractions.Interfaces;
using CSM_Server_Core.Core.Attributes;
using CSM_Server_Core.Core.Models;

using Microsoft.AspNetCore.Mvc;

namespace CSM_Server_Core.Abstractions.Bases;

/// <summary>
///     Represents an entity controller.
/// </summary>
/// <typeparam name="TEntity">
///     Type of the <see cref="IEntity"/> the controller handles.
/// </typeparam>
/// <typeparam name="TEntityService">
///     Type of the <see cref="IEntity"/> service handler.
/// </typeparam>
[ApiController, Route("[Controller]/[Action]")]
public abstract class EntityControllerBase<TEntityService, TEntity>
    : ControllerBase
    where TEntityService : IEntityService<TEntity>
    where TEntity : class, IEntity {

    /// <summary>
    ///     Service reference.
    /// </summary>
    protected TEntityService _service;

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="service">
    ///     Service dependency.
    /// </param>
    public EntityControllerBase(TEntityService service) {
        _service = service;
    }

    /// <summary>
    ///     Controller for entity [View] operation.
    /// </summary>
    /// <param name="input">
    ///     Operation input.
    /// </param>
    /// <returns>
    ///     Http response.
    /// </returns>
    [HttpPost, Action("View")]
    public virtual async Task<IActionResult> View(EntityServiceInput<ViewInput<TEntity>> input) {
        return Ok(
                await _service.View(input)
            );
    }
}
