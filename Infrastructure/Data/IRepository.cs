using WTA.Infrastructure.Domain;

namespace WTA.Infrastructure.Data;

public interface IRepository<T> where T : BaseEntity
{
    IQueryable<T> Queryable();

    IQueryable<T> AsNoTracking();

    IQueryable<T> AsNoTrackingWithIdentityResolution();

    void Delete(Guid id);

    void Delete(Guid[] guids);

    void Insert(T entity);

    void SaveChanges();
}