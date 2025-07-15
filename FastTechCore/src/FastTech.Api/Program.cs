using FastTech.Api.Authorization;
using FastTech.Api.Filters;
using FastTech.Api.Logging;
using FastTech.Application.DataTransferObjects;
using FastTech.Application.Interfaces;
using FastTech.Application.Services;
using FastTech.Domain.Entities;
using FastTech.Domain.Enums;
using FastTech.Domain.Interfaces;
using FastTech.Domain.Interfaces.Infrastructure;
using FastTech.Domain.Interfaces.Security;
using FastTech.Domain.Services;
using FastTech.Domain.Services.Security;
using FastTech.Domain.Settings;
using FastTech.Infrastructure.Data;
using FastTech.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Prometheus;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using static FastTech.Domain.Constants.AppConstants;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

builder.Configuration
    .AddJsonFile("config/appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var jwtKeyConfig = builder.Configuration["Token:Key"];
if (string.IsNullOrEmpty(jwtKeyConfig))
    throw new InvalidOperationException("Token:Key configuration is missing or empty.");
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("Token"));

builder.Services.Configure<FastTechSettings>(builder.Configuration);

builder.Services.AddHealthChecks().ForwardToPrometheus();
builder.WebHost.UseUrls("http://localhost:5056");

builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtKeyConfig)),
        RequireExpirationTime = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicyWithPermission(Policies.Gerente, PermissionLevel.Gerente)
           .AddPolicyWithPermission(Policies.Funcionario, PermissionLevel.Funcionario)
           .AddPolicyWithPermission(Policies.Cliente, PermissionLevel.Cliente);
}).AddAuthorizationBuilder();

builder.Services.AddControllers(options => options.Filters.Add<UserFilter>()).AddNewtonsoftJson(options =>
{
    var settings = options.SerializerSettings;
    settings.NullValueHandling = NullValueHandling.Ignore;
    settings.FloatFormatHandling = FloatFormatHandling.DefaultValue;
    settings.FloatParseHandling = FloatParseHandling.Double;
    settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    settings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
    settings.Culture = new CultureInfo("en-US");
    settings.Converters.Add(new StringEnumConverter());
    settings.ContractResolver = new DefaultContractResolver(); // usa PascalCase (padrão)
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FastTech API", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.CustomSchemaIds(type =>
    {
        var namingStrategy = new SnakeCaseNamingStrategy();
        return namingStrategy.GetPropertyName(type.Name, false);
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization Header - utilizado com Bearer Authentication. \r\n\r\n Insira 'Bearer' [espaço] e então seu token na caixa abaixo.\r\n\r\nExemplo: (informar sem as aspas): 'Bearer 1234sdfgsdf' ",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAutoMapper((sp, cfg) =>
{
    cfg.AllowNullDestinationValues = true;
    cfg.AllowNullCollections = true;
    cfg.ConstructServicesUsing(sp.GetService);
}, Assembly.GetAssembly(typeof(BaseModel)));

builder.Logging.ClearProviders();
builder.Logging.AddProvider(new CustomLoggerProvider(new CustomLoggerProviderConfiguration
{
    LogLevel = LogLevel.Information
}));

builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLConnection"));
    options.LogTo(message => Debug.WriteLine(message), LogLevel.Information);
    options.EnableSensitiveDataLogging();
});

builder.Services.AddMemoryCache();

// Inject all Services and repositories

#region Repositories

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IItemCardapioRepository, ItemCardapioRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();


#endregion

#region Services

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IItemCardapioService, ItemCardapioService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IPedidoProducerService, PedidoProducerService>();


#endregion

#region Application Services

builder.Services.AddScoped<IUserApplicationService, UserApplicationService>();
builder.Services.AddScoped<ITokenApplicationService, TokenApplicationService>();
builder.Services.AddScoped<IPedidoApplicationService, PedidoApplicationService>();
builder.Services.AddScoped<IItemCardapioApplicationService, ItemCardapioApplicationService>();

#endregion

#region Authorization

builder.Services.AddSingleton<IAuthorizationHandler, RolesAuthorizationHandler>();

#endregion

#region Filters

builder.Services.AddScoped<IAuthorizationFilter, UserFilter>();
builder.Services.AddScoped(x => new UserData());

#endregion

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FastTech API 2025"));

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDBContext>();

    context.Database.Migrate();
}


app.UseHealthChecks("/health");
app.UseHttpMetrics();
app.MapMetrics();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
