using Microsoft.EntityFrameworkCore;

namespace WTA.Infrastructure.Data;

public interface IDbSeed<T> where T : DbContext
{
    void Seed(T dbContext);
}