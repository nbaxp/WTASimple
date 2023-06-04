using WTA.Shared.Domain;

namespace WTA.Shared.Data;

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
