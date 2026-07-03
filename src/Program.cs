using Serilog;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using JetFlight.WebApi.Helpers;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Shared;
using JetFlight.ApplicationDataAccess;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.Service;

using Microsoft.EntityFrameworkCore;
using JetFlight.Service.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http.Features;
using SendGrid.Extensions.DependencyInjection;

using DotNetEnv;
using MassTransit;
using JetFlight.Service.Consumers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using JetFlight.IntegrationDataAccess;
using JetFlight.ApplicationDataAccess.Helpers;
using JetFlight.IntegrationDataAccess.Helpers;
using JetFlight.Shared.Models.Export;
using JetFlight.Shared.Helpers;
using JetFlight.Service.Jobs;
using Refit;
using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

{
    var services = builder.Services;

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = 104857600; // 100 MB
    });

    services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 104857600; // 100 MB
    });

    var env = builder.Environment;

    builder.Services.AddControllers()
    .AddXmlSerializerFormatters();

    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

    services.AddSwaggerGen(c =>
    {
        // Docs for Client API
        c.SwaggerDoc(RouteConstants.Client.BasePathName, new OpenApiInfo
        {
            Title = "JetFlight Client API",
            Version = "v1",
            Description = "Client endpoints available for web and mobile apps."
        });

        // Docs for
        c.SwaggerDoc(RouteConstants.Admin.BasePathName, new OpenApiInfo
        {
            Title = "JetFlight Admin API",
            Version = "v1",
            Description = "Endpoints restricted to admins."
        });

        c.SwaggerDoc(RouteConstants.Cashdesk.BasePathName, new OpenApiInfo
        {
            Title = "JetFlight Cashdesk API",
            Version = "v1",
            Description = "Cashdesk"
        });

        // Split docs by groups
        c.DocInclusionPredicate((docName, apiDesc) =>
        {
            var groupName = apiDesc.GroupName;
            return groupName != null && groupName.Equals(docName, StringComparison.OrdinalIgnoreCase);
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter your token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });
        c.OperationFilter<SwaggerOperationFilter>();
        c.SupportNonNullableReferenceTypes();
        c.RegisterPolymorphicTypes();
    });

    services.AddDbContext<IntegrationDataContext>();
    services.AddDbContext<ApplicationDataContext>();
    services.AddCors();
    services.AddControllers().AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        x.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

    services.AddMemoryCache();
    services.AddHttpContextAccessor();

    services.Configure<SmsSettings>(builder.Configuration.GetSection("SmsSettings"));
    services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
    services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    services.Configure<AssetsSettings>(builder.Configuration.GetSection("Assets"));
    services.Configure<MouseflowSettings>(builder.Configuration.GetSection("Mouseflow"));

    services.AddScoped<ISMSServiceClient, VodafoneSMSServiceClient>();
    services.AddScoped<IHtmlGenerationService, HtmlGenerationService>();
    services.AddScoped<IJwtUtils, JwtUtils>();
    services.AddScoped<INotificationService, NotificationService>();
    services.AddScoped<IDataUnitOfWork, DataUnitOfWork>();
    services.AddScoped<IAdminService, AdminService>();
    services.AddScoped<IStoreService, StoreService>();
    services.AddScoped<ISeoMetaService, SeoMetaService>();
    services.AddScoped<IContactUsService, ContactUsService>();
    services.AddScoped<IPageManagementService, PageManagementService>();
    services.AddScoped<IRoleService, RoleService>();
    services.AddScoped<IProductService, ProductService>();
    services.AddScoped<IPostService, PostService>();
    services.AddScoped<ILogHistoryService, LogHistoryService>();
    services.AddScoped<IAuthorizeService, AuthorizeService>();
    services.AddScoped<IGlobalSearchService, GlobalSearchService>();
    services.AddHttpContextAccessor();
    services.AddScoped<IFeedbackService, FeedbackService>();
    services.AddScoped<IRFMService, RFMService>();
    services.AddScoped<ITargetService, TargetService>();
    services.AddScoped<IAvatarService, AvatarService>();
    services.AddScoped<IFlightLoyaltyService, FlightLoyaltyService>();
    services.AddScoped<ICouponService, CouponService>();
    services.AddScoped<IAccumulationCardService, AccumulationCardService>();
    services.AddScoped<IQuestionaryService, QuestionaryService>();
    services.AddScoped<IMediaService, MediaService>();
    services.AddScoped<IFirebaseService, FirebaseService>();
    services.AddKeyedScoped<IExportService, ExportCsvService>(ExportFileFormat.Csv);
    services.AddKeyedScoped<IExportService, ExportXlsxService>(ExportFileFormat.Xlsx);

    //services.AddHostedService<PublishBackgroundService>();
    services.AddHostedService<ProductQueueProcessor>();
    services.AddHostedService<RefreshTokenCleanupService>();

    services.AddScoped<ISettingsService, SettingsService>();
    services.AddScoped<ITargetNotificationService, TargetNotificationService>();

    services.AddScoped<IReceiptService, ReceiptService>();
    services.AddScoped<ILoyaltyService, LoyaltyService>();
    services.AddScoped<IAnalyticService, AnalyticService>();
    services.AddScoped<IAnalyticExportService, AnalyticExportService>();
    services.AddScoped<IRfmSnapshotService, RfmSnapshotService>();

    services.AddSendGrid(options =>
    {
        options.ApiKey = Environment.GetEnvironmentVariable("SEND_GRID_API_KEY");
    });

    services.AddScoped<ISubscriptionService, SubscriptionService>();
    services.AddScoped<ICustomerService, CustomerService>();
    services.AddScoped<IRefreshTokenService, RefreshTokenService>();

    // Configure distributed cache to use Redis (shared across all API instances)
    var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL");
    if (string.IsNullOrWhiteSpace(redisUrl))
    {
        throw new InvalidOperationException("REDIS_URL environment variable is not set");
    }

    var redisUri = new Uri(redisUrl);
    var userInfo = redisUri.UserInfo; // format ":password" or "user:password"
    string? password = null;
    if (!string.IsNullOrEmpty(userInfo))
    {
        var parts = userInfo.Split(':', 2);
        if (parts.Length == 2)
        {
            password = parts[1];
        }
    }

    var redisConfiguration = $"{redisUri.Host}:{redisUri.Port}";
    if (!string.IsNullOrEmpty(password))
    {
        redisConfiguration += $",password={password}";
    }

    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConfiguration;
        options.InstanceName = "jetflight:";
    });

    services.AddMassTransit(x =>
    {
        x.AddConsumer<EmailConsumer>();
        x.AddConsumer<SmsConsumer>();
        x.AddConsumer<LogHistoryConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(Environment.GetEnvironmentVariable("RABBITMQ_URL"));

            cfg.ReceiveEndpoint(NotificationConstant.EmailQueue, e =>
            {
                e.ConfigureConsumer<EmailConsumer>(context);
            });


            cfg.ReceiveEndpoint(NotificationConstant.SmsQueue, e =>
            {
                if (!env.IsDevelopment())
                {
                    e.ConfigureConsumer<SmsConsumer>(context);
                };
            });

            cfg.ReceiveEndpoint(NotificationConstant.LogHistoryQueue, e =>
            {
                e.ConfigureConsumer<LogHistoryConsumer>(context);
            });
        });
    });

    var mouseFlowSettings = new MouseflowSettings();
    builder.Configuration.GetSection("Mouseflow").Bind(mouseFlowSettings);
    services.AddRefitClient<IMouseflowApi>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = mouseFlowSettings.Api;
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Environment.GetEnvironmentVariable("MOUSEFLOW_EMAIL")}:{Environment.GetEnvironmentVariable("MOUSEFLOW_TOKEN")}")));
    });

    services.AddJobServices(builder.Configuration);

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable is not set.");

            var key = Encoding.ASCII.GetBytes(secretKey);
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2), // Допуск для розсинхронізації годинників (мобільні пристрої)
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
            };
        });

    services.AddUserContext();

    builder.Host.UseSerilog((context, loggerConfiguration) =>
    {
        loggerConfiguration
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
    });
}

var app = builder.Build();


app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
         StorageConstants.PhysicalPath),
    RequestPath = StorageConstants.AppPath
});

var currentDirectory = AppContext.BaseDirectory;
var avatarsFolder = Path.Combine(currentDirectory, "Avatars");

var avatarDestDirectory = $"{StorageConstants.PhysicalPath}/Avatars";
if (!Directory.Exists(avatarDestDirectory))
{
    Directory.CreateDirectory(avatarDestDirectory);
}

foreach (var file in Directory.GetFiles(avatarsFolder))
{
    var fileName = Path.GetFileName(file);
    var destFile = Path.Combine(avatarDestDirectory, fileName);
    if (!File.Exists(destFile))
    {
        File.Copy(file, destFile);
    }
}

var imagesFolder = Path.Combine(currentDirectory, "Images");
foreach (var file in Directory.GetFiles(imagesFolder))
{
    var fileName = Path.GetFileName(file);
    var destFile = Path.Combine(StorageConstants.PhysicalPath, fileName);
    if (!File.Exists(destFile))
    {
        File.Copy(file, destFile);
    }
}

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"/swagger/{RouteConstants.Client.BasePathName}/swagger.json", "JetFlight Client API");
    c.SwaggerEndpoint($"/swagger/{RouteConstants.Admin.BasePathName}/swagger.json", "JetFlight Admin API");
    c.SwaggerEndpoint($"/swagger/{RouteConstants.Cashdesk.BasePathName}/swagger.json", "JetFlight Cashdesk API");
});

{
    // TODO: fix cors origins. Disable to allow vercel branches
    // var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();
    app.UseCors(x => x
        .WithOrigins("*")
        .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
        .AllowAnyHeader());
    app.UseMiddleware<ErrorHandlerMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<PermissionContextMiddleware>();
    app.MapControllers();
}


using (var scope = app.Services.CreateScope())
{
    var appDb = scope.ServiceProvider.GetRequiredService<ApplicationDataContext>();
    await appDb.Database.MigrateAsync();
    await appDb.Seed(app.Configuration);
    await appDb.Database.ResetSenquencesAsync();

    var cities = await appDb.Cities
        .OrderBy(c => c.Name)
        .ToListAsync();

    var airportHubs = await appDb.Stores
        .OrderBy(s => s.Title)
        .ToListAsync();

    var personalDataSelectOptions = new PersonalDataSelectOptions
    {
        Cities = cities.ToDictionary(x => x.Id.ToString(), x => x.Name),
        AirportHubs = airportHubs.ToDictionary(x => x.Id.ToString(), x => $"{x.Title} ({x.Number})"),
    };

    var intDb = scope.ServiceProvider.GetRequiredService<IntegrationDataContext>();
    await intDb.Database.MigrateAsync();
    await intDb.Seed(personalDataSelectOptions);

    var jobScheduler = scope.ServiceProvider.GetRequiredService<IJobSchedulerService>();
    await jobScheduler.AddJobsAsync();
}

app.Run();
