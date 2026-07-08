using Microsoft.EntityFrameworkCore;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Domain.Organizations;

namespace ProjectManagementSaaS.Infrastructure.Persistence;

internal sealed class Repository<TEntity>(ApplicationDbContext dbContext) : IRepository<TEntity>
    where TEntity : class
{
    public IQueryable<TEntity> Query()
    {
        return dbContext.Set<TEntity>().AsQueryable();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        if (typeof(TEntity) == typeof(Team))
        {
            var team = await dbContext.Teams
                .Include(entity => entity.Members)
                .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

            return team as TEntity;
        }

        return await dbContext.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        dbContext.Set<TEntity>().Update(entity);
    }

    public void Remove(TEntity entity)
    {
        dbContext.Set<TEntity>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        dbContext.Set<TEntity>().RemoveRange(entities);
    }
}
