using IoT_System.Application.Models;

namespace IoT_System.Application.Interfaces.Repositories.Auth;

/// <summary>
/// Base repository interface providing common CRUD operations and query support for entities.
/// All operations return OperationResult to enable consistent error handling across layers.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by this repository.</typeparam>
public interface IRepositoryBase<TEntity> where TEntity : class
{
    /// <summary>
    /// Returns an IQueryable for building custom queries against the entity set.
    /// </summary>
    /// <returns>An IQueryable of TEntity for LINQ query composition.</returns>
    IQueryable<TEntity> AsQueryable();

    /// <summary>
    /// Retrieves a single entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>An OperationResult containing the entity if found, or null if not found.</returns>
    Task<OperationResult<TEntity?>> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all entities from the database.
    /// </summary>
    /// <returns>An OperationResult containing a collection of all entities.</returns>
    Task<OperationResult<IEnumerable<TEntity>>> GetAllAsync();

    /// <summary>
    /// Adds a new entity to the database and persists changes.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>An OperationResult containing the added entity with any generated values (e.g., ID).</returns>
    Task<OperationResult<TEntity>> AddAsync(TEntity entity);

    /// <summary>
    /// Adds multiple entities to the database in a single operation and persists changes.
    /// </summary>
    /// <param name="entities">The collection of entities to add.</param>
    /// <returns>An OperationResult containing the added entities.</returns>
    Task<OperationResult<IEnumerable<TEntity>>> AddRangeAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// Updates an existing entity in the database and persists changes.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <returns>An OperationResult containing the updated entity.</returns>
    Task<OperationResult<TEntity>> UpdateAsync(TEntity entity);

    /// <summary>
    /// Updates multiple entities in the database in a single operation and persists changes.
    /// </summary>
    /// <param name="entities">The collection of entities with updated values.</param>
    /// <returns>An OperationResult containing the updated entities.</returns>
    Task<OperationResult<IEnumerable<TEntity>>> UpdateRangeAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// Deletes an entity from the database and persists changes.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <returns>An OperationResult containing the deleted entity.</returns>
    Task<OperationResult> DeleteAsync(TEntity entity);

    /// <summary>
    /// Deletes multiple entities from the database in a single operation and persists changes.
    /// </summary>
    /// <param name="entities">The collection of entities to delete.</param>
    /// <returns>An OperationResult containing the deleted entities.</returns>
    Task<OperationResult> DeleteRangeAsync(IEnumerable<TEntity> entities);
}