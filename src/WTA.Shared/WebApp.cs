using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Autofac;
using Coravel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.SwaggerGen;
using WTA.Shared.Attributes;
using WTA.Shared.Authentication;
using WTA.Shared.Controllers;
using WTA.Shared.Data;
using WTA.Shared.DataAnnotations;
using WTA.Shared.DependencyInjection;
using WTA.Shared.Domain;
using WTA.Shared.EventBus;
using WTA.Shared.Extensions;
using WTA.Shared.Job;
using WTA.Shared.Localization;
using WTA.Shared.Module;
using WTA.Shared.Options;
using WTA.Shared.Resources;
using WTA.Shared.SignalR;
using WTA.Shared.Swagger;
using WTA.Shared.Tenants;

namespace WTA.Shared;

public class WebApp
{
    public string OSPlatformName = OperatingSystem.IsWindows() ? nameof(OSPlatform.Windows) : (OperatingSystem.IsLinux() ? nameof(OSPlatform.Linux) : nameof(OSPlatform.OSX));
    public string EntryAssemblyName { get; } = Assembly.GetEntryAssembly()!.GetName().Name!;
    public static WebApp Current { get; } = new WebApp();
    public List<Assembly> Assemblies { get; } = new List<Assembly>();
    public List<Type> ModuleTypes { get; } = new List<Type>();
    public Dictionary<Type, List<Type>> DbSeedTypes { get; } = new Dictionary<Type, List<Type>>();
    public Dictionary<Type, List<Type>> DbConfigTypes { get; } = new Dictionary<Type, List<Type>>();
    public Dictionary<Type, List<Type>> DbContextTypes { get; } = new Dictionary<Type, List<Type>>();
    public IServiceProvider Services { get; private set; } = null!;

    public string Prefix { get; } = nameof(WTA);

    private WebApp()
    {
        // 获取程序集路径
        var path = Path.GetDirectoryName(AppContext.BaseDirectory)!;
        Directory.GetFiles(path, $"{this.Prefix}.*.dll").ForEach(p =>
        {
            // 加载未加载的自定义程序集并统一存放在 WebApp::Assemblies
            this.Assemblies.Add(AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.Location == p) ?? Assembly.LoadFrom(p));
        });
        // 获取数据上下文类型
        var dbContextTypes = this.Assemblies.SelectMany(o => o.GetTypes())
            .Where(o => o.IsClass && !o.IsAbstract && o.GetBaseClasses().Any(b => b.IsGenericType && b.GetGenericTypeDefinition() == typeof(BaseDbContext<>)));
        dbContextTypes.ForEach(dbContextType =>
        {
            var dbSeedTypes = this.Assemblies.SelectMany(o => o.GetTypes())
            .Where(o => o.IsClass && !o.IsAbstract && o.GetInterfaces().Any(b => b.IsGenericType && b.GetGenericTypeDefinition() == typeof(IDbSeed<>) && b.GetGenericArguments()[0] == dbContextType))
            .ToList();
            this.DbSeedTypes.Add(dbContextType, dbSeedTypes);
            var dbConfigTypes = this.Assemblies.SelectMany(o => o.GetTypes())
            .Where(o => o.IsClass && !o.IsAbstract && o.GetInterfaces().Any(b => b.IsGenericType && b.GetGenericTypeDefinition() == typeof(IDbConfig<>) && b.GetGenericArguments()[0] == dbContextType))
            .ToList();
            this.DbConfigTypes.Add(dbContextType, dbConfigTypes);
            // 获取实体类型
            var entityTypes = dbConfigTypes.SelectMany(o => o.GetInterfaces()).Where(o => o.IsGenericType && o.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
            .Select(o => o.GenericTypeArguments.First())
            .ToList();
            var moduleDbContextTypes = new Dictionary<Type, List<Type>>();
            this.DbContextTypes.Add(dbContextType, entityTypes);
        });
        // 获取全部模块类型
        this.ModuleTypes = this.Assemblies.SelectMany(o => o.GetTypes()).Where(o => o.IsClass && !o.IsAbstract && o.IsAssignableTo(typeof(BaseModule))).ToList();
    }

    public virtual void Start(string[] args, Action<WebApplicationBuilder>? configureBuilder = null, Action<WebApplication>? configureApplication = null)
    {
        // 配置日志
        Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
        var loggerConfiguration = new LoggerConfiguration();
        LoggerConfigure(loggerConfiguration);
        Log.Logger = loggerConfiguration.CreateBootstrapLogger();
        try
        {
            Log.Logger.Information($"{this.EntryAssemblyName} Start");
            // 实例化模块
            var modules = this.ModuleTypes.Select(o => Activator.CreateInstance(o) as BaseModule).Where(o => o != null).ToList();
            // 创建 WebApplicationBuilder
            var builder = WebApplication.CreateBuilder(args);
            // 默认配置
            this.ConfigureServices(builder);
            // 模块配置
            modules.ForEach(o => o?.ConfigureServices(builder));
            // 自定义配置
            configureBuilder?.Invoke(builder);
            // 创建 WebApplication
            var app = builder.Build();
            // 配置 WebApp
            this.Services = app.Services;
            // 默认配置
            Configure(app);
            // 模块配置
            modules.ForEach(o => o?.Configure(app));
            // 自定义配置
            configureApplication?.Invoke(app);
            // 启动应用
            app.Run();
        }
        catch (Exception ex)
        {
            if (ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
            {
                throw;
            }
            Log.Fatal(ex, $"App terminated unexpectedly!");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    protected virtual void LoggerConfigure(LoggerConfiguration cfg)
    {
        var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log.txt");
        cfg.MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Volo.Abp", LogEventLevel.Information)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Verbose)
            .Enrich.FromLogContext()
            .Enrich.WithProcessName()
            .Enrich.WithProcessId()
            .Enrich.WithProperty("ApplicationName", Assembly.GetEntryAssembly()?.GetName().Name!)
            .Enrich.WithProperty("Version", Assembly.GetEntryAssembly()?.GetName().Version!)
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.Async(c => c.File(logFile, rollOnFileSizeLimit: true, rollingInterval: RollingInterval.Day, formatProvider: CultureInfo.InvariantCulture));
    }

    public virtual void UseScheduler<T>(WebApplication app) where T : BaseEntity
    {
        app.Services.UseScheduler(scheduler =>
        {
            //1. 从数据库中读取定时任务加入队列
            //2. 任务增删改查时，队列同步改动
            using var scope = app.Services.CreateScope();
            var JobItemRepository = scope.ServiceProvider.GetRequiredService<IRepository<T>>();
            JobItemRepository.Queryable().ToList().ForEach(job =>
            {
                using var scope = app.Services.CreateScope();
                if (typeof(T).GetProperty("Service")?.GetValue(job) is string service && typeof(T).GetProperty("Cron")?.GetValue(job) is string cron)
                {
                    var serviceType = Current.Assemblies.SelectMany(a => a.GetTypes()).FirstOrDefault(o => o.FullName == service);
                    if (serviceType != null)
                    {
                        if (scope.ServiceProvider.GetService(serviceType) is IJobService jobService)
                        {
                            scheduler.Schedule(() => jobService.Invoke()).Cron(cron);
                        }
                    }
                }
            });
        });
    }

    public virtual void ConfigureServices(WebApplicationBuilder builder)
    {
        // 配置依赖注入
        //builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(containerBuilder =>
        //{
        //    var file = Path.Combine(Directory.GetCurrentDirectory(), "autofac.json");
        //    var config = new ConfigurationBuilder().AddJsonFile(file, true, true).Build();
        //    containerBuilder.RegisterModule(new ConfigurationModule(config));
        //}));
        // 添加日志
        builder.Host.UseSerilog((hostingContext, services, configBuilder) =>
        {
            LoggerConfigure(configBuilder);
            var logServer = hostingContext.Configuration.GetValue<string>("LogServer");
            if (logServer != null)
            {
                configBuilder.WriteTo.Http(logServer, null);
            }
        }, writeToProviders: true);
        builder.Host.UseDefaultServiceProvider((context, options) =>
        {
            options.ValidateOnBuild = false;
        });
        // 添加 JWT
        AddJwtAuthentication(builder);
        // 添加 JSON
        var defaultJsonOptions = AddJson(builder);
        // 添加 HTTP
        AddHttp(builder);
        // 添加本地化
        AddLocalization(builder);
        // 添加 MVC
        AddMvc(builder, defaultJsonOptions);
        // 添加 SignalR
        AddSignalR(builder);
        //Embed wwwroot
        builder.Services.ConfigureOptions(new EmbeddedConfigureOptions());
        // 添加 Swagger
        AddSwagger(builder);
        // 添加默认服务
        this.AddDefaultServices(builder);
        // 添加默认配置
        this.AddDefaultOptions(builder);
        // 添加本地事件
        this.AddEventBus(builder);
        // 添加缓存
        builder.Services.AddMemoryCache();
        // 添加数据上下文
        this.AddDbContext(builder);
        // 定时任务
        builder.Services.AddScheduler();
        Current.Assemblies.SelectMany(o => o.GetTypes())
            .Where(o => o.IsClass && !o.IsAbstract && o.IsAssignableTo(typeof(IJobService)))
            .ForEach(o => builder.Services.AddTransient(o));
    }

    public virtual void AddSignalR(WebApplicationBuilder builder)
    {
        //SignalR
        var signalRServerBuilder = builder.Services.AddSignalR(o =>
        {
            o.EnableDetailedErrors = true;
        });
        var redisConnectionString = builder.Configuration.GetConnectionString("SignalR");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            var prefix = Assembly.GetEntryAssembly()!.GetName().Name;
            signalRServerBuilder.AddStackExchangeRedis(redisConnectionString, o => o.Configuration.ChannelPrefix = prefix);
        }
    }

    public virtual void AddMvc(WebApplicationBuilder builder, JsonSerializerOptions defaultJsonOptions)
    {        // 配置路由
        builder.Services.AddRouting(options => options.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer));
        // 配置 CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            });
        });
        builder.Services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationFormats.Add("/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
            options.ViewLocationFormats.Add("/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
            options.ViewLocationFormats.Add("/Views/Shared/Default" + RazorViewEngine.ViewExtension);// add for default

            options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
            options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
            options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/Default" + RazorViewEngine.ViewExtension);// add for default
            options.AreaViewLocationFormats.Add("/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
        });
        builder.Services.AddRouting(options => options.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer));
        builder.Services.AddSingleton<IModelMetadataProvider, CustomModelMetaDataProvider>();
        builder.Services.AddSingleton<ValidationAttributeAdapterProvider>();
        builder.Services.AddMvc(o =>
        {
            //SuppressImplicitRequiredAttributeForNonNullableReferenceTypes 为 false 时 D
            //ataAnnotationsMetadataProvider 中会自动为不可空引用类型添加一个无法自定义 ErrorMessage 的 RequiredAttribute
            //因此禁用此行为，并在 CustomValidationMetadataProvider 中手动重新添加 RequiredAttribute
            o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            o.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
            o.ModelMetadataDetailsProviders.Insert(0, new CustomDisplayMetadataProvider());
            o.ModelMetadataDetailsProviders.Add(new CustomValidationMetadataProvider());
            o.Conventions.Add(new ControllerModelConvention());
            o.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
            o.Conventions.Add(new GenericControllerRouteConvention());
        }).ConfigureApiBehaviorOptions(o =>
        {
            o.SuppressModelStateInvalidFilter = true;
        }).AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = defaultJsonOptions.PropertyNamingPolicy;
            o.JsonSerializerOptions.DictionaryKeyPolicy = defaultJsonOptions.DictionaryKeyPolicy;
            o.JsonSerializerOptions.ReferenceHandler = defaultJsonOptions.ReferenceHandler;
            o.JsonSerializerOptions.Encoder = defaultJsonOptions.Encoder;
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            if (builder.Environment.IsDevelopment())
            {
                o.JsonSerializerOptions.WriteIndented = true;
            }
        })
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
        .AddDataAnnotationsLocalization(options =>
        {
            options.DataAnnotationLocalizerProvider = (type, factory) =>
            {
                var localizer = factory.Create(typeof(Resource));
                return localizer;
            };
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
            //options.InvalidModelStateResponseFactory = context =>
            //{
            //    context.ActionDescriptor.Parameters[0].ParameterType.GetMetadataForType(context.HttpContext.RequestServices);
            //    if (!context.ModelState.IsValid)
            //    {
            //        var errors = context.ModelState.ToErrors();
            //    }
            //    return new BadRequestObjectResult(context.ModelState);
            //};
        })
        .ConfigureApplicationPartManager(o => o.FeatureProviders.Add(new GenericControllerFeatureProvider()))
        .AddControllersAsServices();
    }

    public virtual JsonSerializerOptions AddJson(WebApplicationBuilder builder)
    {
        var defaultJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        defaultJsonOptions.Converters.Add(new JsonStringEnumConverter());
        builder.Services.AddSingleton(defaultJsonOptions);
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = defaultJsonOptions.PropertyNamingPolicy;
            options.SerializerOptions.DictionaryKeyPolicy = defaultJsonOptions.DictionaryKeyPolicy;
            options.SerializerOptions.ReferenceHandler = defaultJsonOptions.ReferenceHandler;
            options.SerializerOptions.Encoder = defaultJsonOptions.Encoder;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        return defaultJsonOptions;
    }

    protected void AddSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, CustomSwaggerGenOptions>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SchemaFilter<CustomSwaggerFilter>();
            options.OperationFilter<CustomSwaggerFilter>();
            options.DocInclusionPredicate((docName, api) =>
            {
                if (docName == "Default" && api.GroupName == null)
                {
                    return true;
                }
                return api.GroupName == docName;
            });
            options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    public virtual void Configure(WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseHttpMethodOverride();
        app.UseStaticFiles();
        app.UseRouting();
        UseLocalization(app);
        UseAuthorization(app);
        app.MapDefaultControllerRoute();
        app.MapHub<PageHub>("/api/hub");
        app.UseCors();// must after maphub
        UseSwagger(app);
        UseDbContext(app);
    }

    protected virtual void UseAuthorization(WebApplication app)
    {
        app.UseCors(this.EntryAssemblyName);
        app.UseAuthentication();
        app.UseAuthorization();
    }

    protected virtual void AddHttp(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(this.EntryAssemblyName!, builder =>
            {
                builder.SetIsOriginAllowed(isOriginAllowed => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
        });
        builder.Services.AddWebEncoders(options => options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        });
        builder.Services.Configure<FormOptions>(options =>
        {
            options.ValueCountLimit = int.MaxValue;
            options.MultipartBodyLengthLimit = long.MaxValue;
        });
        // 配置表单
        builder.Services.Configure<FormOptions>(options =>
        {
            options.ValueCountLimit = int.MaxValue;
            options.MultipartBodyLengthLimit = long.MaxValue;
        });
    }

    protected virtual void AddLocalization(WebApplicationBuilder builder)
    {
        builder.Services.AddLocalization();
        builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
        builder.Services.AddSingleton<IStringLocalizer>(o => o.GetRequiredService<IStringLocalizer<Resource>>());
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("zh"),
                new CultureInfo("en")
            };
            options.DefaultRequestCulture = new RequestCulture(supportedCultures.First());
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider());
        });
    }

    protected virtual void UseLocalization(WebApplication app)
    {
        var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>()!.Value;
        app.UseRequestLocalization(localizationOptions);
        Thread.CurrentThread.CurrentCulture = localizationOptions.DefaultRequestCulture.Culture;
        Thread.CurrentThread.CurrentUICulture = localizationOptions.DefaultRequestCulture.UICulture;
    }

    protected void UseSwagger(WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var apiDescriptionGroups = app.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>().ApiDescriptionGroups.Items;
            foreach (var description in apiDescriptionGroups)
            {
                if (description.GroupName is not null)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
                }
                else
                {
                    options.SwaggerEndpoint($"/swagger/Default/swagger.json", "Default");
                }
            }
        });
    }

    protected virtual void AddJwtAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<CustomJwtSecurityTokenHandler>();
        builder.Services.AddSingleton<JwtSecurityTokenHandler, CustomJwtSecurityTokenHandler>();
        builder.Services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, CustomJwtBearerPostConfigureOptions>();
        builder.Services.Configure<IdentityOptions>(builder.Configuration.GetSection(IdentityOptions.Position));
        var jwtOptions = new IdentityOptions();
        builder.Configuration.GetSection(IdentityOptions.Position).Bind(jwtOptions);
        var issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
        builder.Services.AddSingleton(new SigningCredentials(issuerSigningKey, SecurityAlgorithms.HmacSha256Signature));
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = issuerSigningKey,
            NameClaimType = nameof(ClaimTypes.Name),
            RoleClaimType = nameof(ClaimTypes.Role),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
        builder.Services.AddSingleton(tokenValidationParameters);
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = tokenValidationParameters;
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token ??= context.Request.Query["access_token"];
                    return Task.CompletedTask;
                }
            };
        });
        //builder.Services.AddTransient<ITokenService, TokenService>();
        builder.Services.AddAuthorization();
    }

    public virtual void AddDefaultServices(WebApplicationBuilder builder)
    {
        this.Assemblies
            .Where(o => o.FullName!.StartsWith(nameof(WTA)))
            .Where(o => o.GetTypes()
            .Any(o => o.GetCustomAttributes(typeof(ImplementAttribute<>)).Any()))
            .SelectMany(o => o.GetTypes())
            .Where(type => type.GetCustomAttributes(typeof(ImplementAttribute<>)).Any())
            .ForEach(type =>
            {
                foreach (var implementation in type.GetCustomAttributes(typeof(ImplementAttribute<>)).Select(o => (o as IImplementAttribute)!))
                {
                    var currentPlatformType = (PlatformType)Enum.Parse(typeof(PlatformType), this.OSPlatformName);
                    if (implementation.PlatformType.HasFlag(currentPlatformType))
                    {
                        if (implementation.ServiceType.IsAssignableTo(typeof(IHostedService)))
                        {
                            var method = typeof(ServiceCollectionHostedServiceExtensions)
                            .GetMethod(nameof(ServiceCollectionHostedServiceExtensions.AddHostedService),
                            new[] { typeof(IServiceCollection) });
                            method?.MakeGenericMethod(type).Invoke(null, new object[] { builder.Services });
                        }
                        else
                        {
                            if (implementation.Lifetime == ServiceLifetime.Singleton)
                            {
                                builder.Services.TryAddSingleton(implementation.ServiceType, type);
                            }
                            else if (implementation.Lifetime == ServiceLifetime.Scoped)
                            {
                                builder.Services.TryAddScoped(implementation.ServiceType, type);
                            }
                            else
                            {
                                builder.Services.TryAddTransient(implementation.ServiceType, type);
                            }
                        }
                    }
                }
            });
    }

    public void AddDefaultOptions(WebApplicationBuilder builder)
    {
        var configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethod(nameof(OptionsConfigurationServiceCollectionExtensions.Configure),
            new[] { typeof(IServiceCollection), typeof(IConfiguration) });

        AppDomain.CurrentDomain.GetAssemblies()
            .Where(o => o.FullName!.StartsWith(nameof(WTA)))
            .Where(o => o.GetTypes()
            .Any(o => o.GetCustomAttributes(typeof(OptionsAttribute)).Any()))
            .SelectMany(o => o.GetTypes())
            .Where(type => type.GetCustomAttributes<OptionsAttribute>().Any())
            .ForEach(type =>
            {
                var attribute = type.GetCustomAttribute<OptionsAttribute>()!;
                var configurationSection = builder.Configuration.GetSection(attribute.Section ?? type.Name.TrimEnd("Options"));
                configureMethod?.MakeGenericMethod(type).Invoke(null, new object[] { builder.Services, configurationSection });
            });
    }

    public void AddEventBus(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IEventPublisher, DefaultEventPublisher>();
        this.Assemblies?
            .SelectMany(o => o.GetTypes())
            .Where(t => t.GetInterfaces().Any(o => o.IsGenericType && o.GetGenericTypeDefinition() == typeof(IEventHander<>)))
            .ToList()
            .ForEach(type =>
            {
                type.GetInterfaces()
                .Where(o => o.IsGenericType && o.GetGenericTypeDefinition() == typeof(IEventHander<>)).ToList()
                .ForEach(o => builder.Services.AddScoped(o, type));
            });
    }

    public void AddDbContext(WebApplicationBuilder builder)
    {
        var parameterTypes = new Type[]
        {
            typeof(IServiceCollection),
            typeof(Action<DbContextOptionsBuilder>),
            typeof(ServiceLifetime),
            typeof(ServiceLifetime)
        };
        var method = typeof(EntityFrameworkServiceCollectionExtensions)
            .GetMethods()
            .FirstOrDefault(o => o.Name == nameof(EntityFrameworkServiceCollectionExtensions.AddDbContext) &&
            o.GetParameters().Select(x => x.ParameterType).SequenceEqual(parameterTypes));
        this.DbContextTypes.Keys.ForEach(dbContextType =>
        {
            Action<DbContextOptionsBuilder> action = optionsBuilder =>
            {
                var connectionStringName = dbContextType.Name.TrimEnd("DbContext");
                var connectionString = builder.Configuration.GetConnectionString(connectionStringName);
                if (!dbContextType.CustomAttributes.Any(o => o.AttributeType == typeof(IgnoreMultiTenancyAttribute)))
                {
                    var tenantService = this.Services.GetService<ITenantService>();
                    if (tenantService != null)
                    {
                        var tenantId = tenantService?.TenantId;
                        if (tenantId != null)
                        {
                            connectionString = tenantService?.GetConnectionString(connectionStringName);
                            if (string.IsNullOrEmpty(connectionString))
                            {
                                throw new Exception("租户不存在");
                            }
                        }
                    }
                }
                optionsBuilder.UseSqlite(connectionString);
            };
            method?.MakeGenericMethod(dbContextType).Invoke(null, new object[] { builder.Services, action, ServiceLifetime.Scoped, ServiceLifetime.Scoped });
            var dbSeedType = typeof(IDbSeed<>).MakeGenericType(dbContextType);
            if (this.DbSeedTypes.TryGetValue(dbContextType, out var seedTypes))
            {
                seedTypes.ForEach(o => builder.Services.AddTransient(dbSeedType, o));
            }
        });
        builder.Services.AddTransient(typeof(IRepository<>), typeof(EfRepository<>));
    }

    public virtual void UseDbContext(WebApplication app)
    {
        app.Services.CreateDatabase();
    }
}
