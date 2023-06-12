using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WTA.Shared.Domain;

namespace WTA.Shared.Data;

public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    private readonly DbContext _dbContext;

    public EfRepository(IServiceProvider serviceProvider)
    {
        var dbContextType = WebApp.Current.ModuleTypes.SelectMany(o => o.Value).FirstOrDefault(o => o.Value.Contains(typeof(TEntity))).Key;
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

    public void Delete(Guid[] guids)
    {
        this._dbContext.Set<TEntity>().Where(o => guids.Contains(o.Id)).ExecuteDelete();
    }

    public void Delete(Guid id)
    {
        this._dbContext.Set<TEntity>().Where(o => o.Id == id).ExecuteDelete();
    }

    public void Insert(TEntity entity)
    {
        this._dbContext.Set<TEntity>().Add(entity);
    }

    public void SaveChanges()
    {
        this._dbContext.SaveChanges();
    }

    public void IncludeDeleted()
    {
        this._dbContext.GetType().GetProperty("DisableSoftDeleteFilter")?.SetValue(this._dbContext, true);
    }
}