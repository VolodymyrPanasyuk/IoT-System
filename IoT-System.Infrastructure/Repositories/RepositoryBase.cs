using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Application.Models;
using IoT_System.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Repositories;

/// <summary>
/// Base repository implementation providing common CRUD operations with OperationResult wrapping.
/// </summary>
/// <typeparam name="TEntity">The entity type this repository manages.</typeparam>
public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
{
    protected readonly AuthDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the repository.
    /// </summary>
    /// <param name="context">The database context.</param>
    public RepositoryBase(AuthDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    /// <summary>
    /// Returns an IQueryable for building custom queries.
    /// </summary>
    /// <returns>IQueryable of TEntity.</returns>
    public IQueryable<TEntity> AsQueryable() => _dbSet.AsQueryable();

    /// <summary>
    /// Retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <returns>OperationResult containing the entity or null if not found.</returns>
    public Task<OperationResult<TEntity?>> GetByIdAsync(Guid id)
        => ExecuteAsync(async () => await _dbSet.FindAsync(id));

    /// <summary>
    /// Retrieves all entities from the database.
    /// </summary>
    /// <returns>OperationResult containing the collection of all entities.</returns>
    public Task<OperationResult<IEnumerable<TEntity>>> GetAllAsync()
        => ExecuteAsync<IEnumerable<TEntity>>(async () => await _dbSet.ToListAsync());

    /// <summary>
    /// Adds a new entity to the database.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>OperationResult containing the added entity.</returns>
    public Task<OperationResult<TEntity>> AddAsync(TEntity entity)
        => ExecuteAsync(async () =>
        {
            await _dbSet.AddAsync(entity);
            return entity;
        });

    /// <summary>
    /// Adds multiple entities to the database.
    /// </summary>
    /// <param name="entities">The collection of entities to add.</param>
    /// <returns>OperationResult containing the added entities.</returns>
    public Task<OperationResult<IEnumerable<TEntity>>> AddRangeAsync(IEnumerable<TEntity> entities)
        => ExecuteAsync(async () =>
        {
            await _dbSet.AddRangeAsync(entities);
            return entities;
        });

    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>OperationResult containing the updated entity.</returns>
    public Task<OperationResult<TEntity>> UpdateAsync(TEntity entity)
        => ExecuteAsync(() =>
        {
            _dbSet.Update(entity);
            return entity;
        });

    /// <summary>
    /// Updates multiple entities in the database.
    /// </summary>
    /// <param name="entities">The collection of entities to update.</param>
    /// <returns>OperationResult containing the updated entities.</returns>
    public Task<OperationResult<IEnumerable<TEntity>>> UpdateRangeAsync(IEnumerable<TEntity> entities)
        => ExecuteAsync(() =>
        {
            _dbSet.UpdateRange(entities);
            return entities;
        });

    /// <summary>
    /// Deletes an entity from the database.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <returns>OperationResult containing the deleted entity.</returns>
    public Task<OperationResult<TEntity>> DeleteAsync(TEntity entity)
        => ExecuteAsync(() =>
        {
            _dbSet.Remove(entity);
            return entity;
        });

    /// <summary>
    /// Deletes multiple entities from the database.
    /// </summary>
    /// <param name="entities">The collection of entities to delete.</param>
    /// <returns>OperationResult containing the deleted entities.</returns>
    public Task<OperationResult<IEnumerable<TEntity>>> DeleteRangeAsync(IEnumerable<TEntity> entities)
        => ExecuteAsync(() =>
        {
            _dbSet.RemoveRange(entities);
            return entities;
        });

    #region Protected Methods

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    /// <returns>True if any changes were saved; otherwise, false.</returns>
    protected async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Executes an operation that modifies the database and saves changes.
    /// Supports both synchronous and asynchronous operations through overloading.
    /// </summary>
    /// <typeparam name="TResult">The type of data returned by the operation.</typeparam>
    /// <param name="operation">The operation to execute that returns the result data.</param>
    /// <returns>OperationResult indicating success or failure with the data or exception.</returns>
    protected async Task<OperationResult<TResult>> ExecuteAsync<TResult>(Func<Task<TResult>> operation)
    {
        try
        {
            var data = await operation();
            var isSuccess = await SaveChangesAsync();
            return OperationResult<TResult>.Result(isSuccess, data);
        }
        catch (Exception ex)
        {
            return OperationResult<TResult>.Failure(ex);
        }
    }

    /// <summary>
    /// Executes a synchronous operation that modifies the database and saves changes.
    /// </summary>
    /// <typeparam name="TResult">The type of data returned by the operation.</typeparam>
    /// <param name="operation">The synchronous operation to execute that returns the result data.</param>
    /// <returns>OperationResult indicating success or failure with the data or exception.</returns>
    protected async Task<OperationResult<TResult>> ExecuteAsync<TResult>(Func<TResult> operation)
    {
        try
        {
            var data = operation();
            var isSuccess = await SaveChangesAsync();
            return OperationResult<TResult>.Result(isSuccess, data);
        }
        catch (Exception ex)
        {
            return OperationResult<TResult>.Failure(ex);
        }
    }

    #endregion
}