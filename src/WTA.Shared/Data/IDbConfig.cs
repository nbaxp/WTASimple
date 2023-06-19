using Microsoft.EntityFrameworkCore;

namespace WTA.Shared.Data;

public interface IDbConfig<T> where T : DbContext
{
}
