using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using WTA.Shared.Domain;

namespace WTA.Shared.Data;

public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    private readonly DbContext _dbContext;

    public EfRepository(IServiceProvider serviceProvider)
    {
        var dbContextType = WebApp.Current.DbContextTypes.FirstOrDefault(o => o.Value.Contains(typeof(TEntity))).Key;
        var scope = serviceProvider.CreateScope();
        this._dbContext = (scope.ServiceProvider.GetRequiredService(dbContextType!) as DbContext)!;
    }

    public IQueryable<TEntity> Queryable()
    {
        return this._dbContext.Set<TEntity>();
    }

    public IQueryable<TEntity> AsNoTracking()
    {
        return this._dbContext.Set<TEntity>().AsNoTracking();
    }

    public IQueryable<TEntity> AsNoTrackingWithIdentityResolution()
    {
        return this._dbContext.Set<TEntity>().AsNoTrackingWithIdentityResolution();
    }

    public void Update(Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, Expression<Func<TEntity, bool>> predicate)
    {
        this._dbContext.Set<TEntity>().Where(predicate).ExecuteUpdate(setPropertyCalls);
    }

    public void Delete(Expression<Func<TEntity, bool>> predicate)
    {
        this._dbContext.Set<TEntity>().Where(predicate).ExecuteDelete();
    }

    public void Insert(TEntity entity)
    {
        this._dbContext.Set<TEntity>().Add(entity);
    }

    public void SaveChanges()
    {
        this._dbContext.SaveChanges();
    }

    public void DisableSoftDeleteFilter()
    {
        this._dbContext.GetType().GetProperty("DisableSoftDeleteFilter")?.SetValue(this._dbContext, true);
    }

    public void DisableTenantFilter()
    {
        this._dbContext.GetType().GetProperty("DisableTenantFilter")?.SetValue(this._dbContext, true);
    }
}
