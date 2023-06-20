using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using WTA.Shared.Data;

namespace WTA.Shared.Extensions;

public static class ServiceProviderExtensions
{
    public static void CreateDatabase(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        WebApp.Current.DbContextTypes.Keys.ForEach(dbContextType =>
        {
            var contextName = dbContextType.Name;
            if (serviceProvider.GetRequiredService(dbContextType) is DbContext initDbContext)
            {
                var dbCreator = (initDbContext.GetService<IRelationalDatabaseCreator>() as RelationalDatabaseCreator)!;
                if (!dbCreator.Exists())
                {
                    dbCreator.Create();
                    var createSql = "CREATE TABLE EFDbContext(Id varchar(255) NOT NULL,Hash varchar(255),Date datetime  NOT NULL,PRIMARY KEY (Id));";
                    initDbContext.Database.ExecuteSqlRaw(createSql);
                }
            }
            if (serviceProvider.GetRequiredService(dbContextType) is DbContext context)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    context.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
                    var dbCreator = (context.GetService<IRelationalDatabaseCreator>() as RelationalDatabaseCreator)!;
                    var sql = dbCreator.GenerateCreateScript();
                    var md5 = sql.ToMd5();
                    var path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location!)!, "scripts");
                    Directory.CreateDirectory(path);
                    using var sw = File.CreateText(Path.Combine(path, $"db.{context.Database.ProviderName}.{contextName}.sql"));
                    sw.Write(sql);
                    Console.WriteLine($"{contextName} 初始化开始");
                    Console.WriteLine($"ConnectionString:{context.Database.GetConnectionString()}");
                    // 查询当前DbContext是否已经初始化
                    var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    var connection = context.Database.GetDbConnection();
                    var command = connection.CreateCommand();
                    command.Transaction = transaction.GetDbTransaction();
                    command.CommandText = $"SELECT Hash FROM EFDbContext where Id='{contextName}'";
                    var hash = command.ExecuteScalar();
                    if (hash == null)
                    {
                        if (context.Database.ProviderName!.Contains("SqlServer"))
                        {
                            var pattern = @"(?<=;\s+)GO(?=\s\s+)";
                            var sqls = Regex.Split(sql, pattern).Where(o => !string.IsNullOrWhiteSpace(o)).ToList();
                            foreach (var item in sqls)
                            {
                                command.CommandText = Regex.Replace(sql, pattern, string.Empty);
                                command.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            command.CommandText = sql;
                            command.ExecuteNonQuery();
                        }
                        command.CommandText = $"INSERT INTO EFDbContext VALUES ('{contextName}', '{md5}','{now}');";
                        command.ExecuteNonQuery();
                        var dbSeedType = typeof(IDbSeed<>).MakeGenericType(dbContextType);
                        serviceProvider.GetServices(dbSeedType).ForEach(o => dbSeedType.GetMethod("Seed")?.Invoke(o, new object[] { context }));
                        Console.WriteLine($"{contextName} 初始化成功");
                    }
                    else
                    {
                        Console.WriteLine($"{contextName} 数据库结构{(hash.ToString() == md5 ? "正常" : "已过时")}");
                    }
                    context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    var message = $"{contextName} 初始化失败：{ex.Message}";
                    Console.WriteLine(message);
                    Console.WriteLine(ex.ToString());
                    throw new Exception(message, ex);
                }
                finally
                {
                    Console.WriteLine($"{contextName} 初始化结束");
                }
            }
        });
    }
}
