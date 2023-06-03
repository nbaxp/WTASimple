using Microsoft.EntityFrameworkCore;

namespace WTA.Shared.Data;

public interface IDbSeed<T> where T : DbContext
{
    void Seed(T dbContext);
}