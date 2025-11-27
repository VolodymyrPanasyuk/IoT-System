using static IoT_System.Application.Common.Helpers.ExecutionHelper;
using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Repositories;

/// <summary>
/// Base repository implementation providing common CRUD operations with OperationResult wrapping.
/// Generic over both entity type and database context for maximum flexibility and type safety.
/// All derived repositories must specify their concrete DbContext type.
/// </summary>
/// <typeparam name="TEntity">The entity type this repository manages.</typeparam>
/// <typeparam name="TDbContext">The database context type (must inherit from DbContext).</typeparam>
public abstract class RepositoryBase<TEntity, TDbContext> : IRepositoryBase<TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    protected readonly TDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the repository with the specified database context.
    /// </summary>
    /// <param name="context">The database context instance used for data access.</param>
    protected RepositoryBase(TDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    /// <summary>
    /// Returns an IQueryable for building custom queries.
    /// </summary>
    /// <returns>IQueryable of TEntity.</returns>
    public virtual IQueryable<TEntity> AsQueryable() => _dbSet;

    /// <summary>
    /// Retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <returns>OperationResult containing the entity or null if not found.</returns>
    public virtual Task<OperationResult<TEntity?>> GetByIdAsync(Guid id)
        => ExecuteAsync(async () => await _dbSet.FindAsync(id));

    /// <summary>
    /// Retrieves all entities from the database.
    /// </summary>
    /// <returns>OperationResult containing the collection of all entities.</returns>
    public virtual Task<OperationResult<IEnumerable<TEntity>>> GetAllAsync()
        => ExecuteAsync<IEnumerable<TEntity>>(async () => await _dbSet.ToListAsync());

    /// <summary>
    /// Adds a new entity to the database.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>OperationResult containing the added entity.</returns>
    public virtual Task<OperationResult<TEntity>> AddAsync(TEntity entity)
        => ExecuteAsync(async () =>
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        });

    /// <summary>
    /// Adds multiple entities to the database.
    /// </summary>
    /// <param name="entities">The collection of entities to add.</param>
    /// <returns>OperationResult containing the added entities.</returns>
    public virtual Task<OperationResult<IEnumerable<TEntity>>> AddRangeAsync(IEnumerable<TEntity> entities)
        => ExecuteAsync(async () =>
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return entities;
        });

    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>OperationResult containing the updated entity.</returns>
    public virtual Task<OperationResult<TEntity>> UpdateAsync(TEntity entity)
        => ExecuteAsync(async () =>
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        });

    /// <summary>
    /// Updates multiple entities in the database.
    /// </summary>
    /// <param name="entities">The collection of entities to update.</param>
    /// <returns>OperationResult containing the updated entities.</returns>
    public virtual Task<OperationResult<IEnumerable<TEntity>>> UpdateRangeAsync(IEnumerable<TEntity> entities)
        => ExecuteAsync(async () =>
        {
            _dbSet.UpdateRange(entities);
            await _context.SaveChangesAsync();
            return entities;
        });

    /// <summary>
    /// Deletes an entity from the database.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <returns>OperationResult containing the deleted entity.</returns>
    public virtual Task<OperationResult> DeleteAsync(TEntity entity)
        => ExecuteAsync(async () =>
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        });

    /// <summary>
    /// Deletes multiple entities from the database.
    /// </summary>
    /// <param name="entities">The collection of entities to delete.</param>
    /// <returns>OperationResult containing the deleted entities.</returns>
    public virtual Task<OperationResult> DeleteRangeAsync(IEnumerable<TEntity> entities)
        => ExecuteAsync(async () =>
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
        });
}