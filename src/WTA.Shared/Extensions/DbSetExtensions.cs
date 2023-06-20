using Microsoft.EntityFrameworkCore;
using WTA.Shared.Domain;

namespace WTA.Shared.Extensions;

public static class DbSetExtensions
{
    public static void AddOrUpdate<T>(this DbSet<T> set, T entity) where T : BaseEntity
    {
        if (set.Any(o => o.Id == entity.Id))
        {
            set.Update(entity);
        }
        else
        {
            set.Add(entity);
        }
    }
}
