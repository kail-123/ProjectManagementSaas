namespace ProjectManagementSaaS.Application.Abstractions.Persistence;

public interface IRepository<TEntity>
    where TEntity : class
{
    IQueryable<TEntity> Query();

    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);
}
