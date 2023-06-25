using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using WTA.Shared.Domain;

namespace WTA.Shared.Data;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    IQueryable<TEntity> Queryable();

    IQueryable<TEntity> AsNoTracking();

    IQueryable<TEntity> AsNoTrackingWithIdentityResolution();

    void Update(Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, Expression<Func<TEntity, bool>> predicate);

    void Delete(Expression<Func<TEntity, bool>> predicate);

    void Insert(TEntity entity);

    void SaveChanges();

    void DisableSoftDeleteFilter();

    void DisableTenantFilter();
}
