
using Library.API.Consumer;
using Library.Common.RabbitMqMessages.LoggingMessages;
using Library.Common.StringConstants;
using Library.Domain.Data;
using Library.Domain.Repositories;
using Library.Infrastructure.Logging.Interfaces;
using Library.Infrastructure.Logging.Services;
using Library.Infrastructure.Mongo;
using Library.Infrastructure.RabbitMQ.Configuation;
using Library.Services.Interfaces;
using Library.Services.Services;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Library.Seeder;

var builder = WebApplication.CreateBuilder(args);

// MongoDB Guid serialization
BsonSerializer.RegisterSerializer(
    new GuidSerializer(GuidRepresentation.Standard)
);

// Add DbContext
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add controllers + JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Register Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Library API", Version = "v1" });
    c.UseInlineDefinitionsForEnums();
});

// Library Services
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();

// MongoContext
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    string mongoConnection = configuration["MongoSettings:ConnectionString"]
        ?? throw new InvalidOperationException("Mongo connection string is missing in configuration.");
    string mongoDbName = configuration["MongoSettings:DatabaseName"]
        ?? throw new InvalidOperationException("Mongo database name is missing in configuration.");

    return new MongoContext(mongoConnection, mongoDbName);
});

// Logging services
builder.Services.AddSingleton<IExceptionLoggerService, ExceptionLoggerService>();
builder.Services.AddSingleton<IMessageLoggerService, MessageLoggerService>();
builder.Services.AddSingleton<IFailedLoggerService, FailedLoggerService>();

// MassTransit setup
var rabbitMqSettings = builder.Configuration
    .GetSection("RabbitMqSettings")
    .Get<RabbitMqSettings>();

bool useRabbitMq = false; // set to true to enable RabbitMQ

if (useRabbitMq && rabbitMqSettings != null)
{
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<ExceptionLogsConsumer>();
        x.AddConsumer<MessageLogsConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitMqSettings.HostName, h =>
            {
                h.Username(rabbitMqSettings.UserName);
                h.Password(rabbitMqSettings.Password);
            });

            cfg.ReceiveEndpoint(rabbitMqSettings.ExceptionQueue, e =>
            {
                e.ConfigureConsumer<ExceptionLogsConsumer>(context);
            });

            cfg.ReceiveEndpoint(rabbitMqSettings.MessageQueue, e =>
            {
                e.ConfigureConsumer<MessageLogsConsumer>(context);
            });
        });
    });
}
else
{
    // Optional: fallback to in-memory transport so the app still runs
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<ExceptionLogsConsumer>();
        x.AddConsumer<MessageLogsConsumer>();

        x.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    });
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // "Bearer"
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    // Allow non-HTTPS metadata in dev (important for localhost testing)
    options.RequireHttpsMetadata = false;

    var jwtKey = builder.Configuration["Jwt:Key"]
                 ?? throw new InvalidOperationException("Jwt:Key is not configured.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        ),

        // Prevent clock skew issues (default is 5 minutes)
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // This is for missing/invalid token
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\":\"Invalid or missing token\"}");
        },
        OnForbidden = context =>
        {
            // This is for valid token but insufficient permissions
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\":\"User not authorized\"}");
        }
    };
});


// Authorization policies
builder.Services.AddAuthorization(options =>
{
    foreach (var perm in PermissionNames.All)
    {
        options.AddPolicy(perm, policy =>
            policy.RequireClaim("Permission", perm));
    }
});

var app = builder.Build();

// Exception handler must come first
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception != null)
        {
            var logger = context.RequestServices.GetRequiredService<IExceptionLoggerService>();

            var exceptionDto = new ExceptionLogMessage
            {
                Guid = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Level = MyLogLevel.Exception,
                ServiceName = $"{context.Request.Method} {context.Request.Path}",
                Request = $"{context.Request.Method} {context.Request.Path}",
                ExceptionMessage = exception.Message,
                StackTrace = exception.StackTrace ?? string.Empty
            };

            await logger.LogExceptionAsync(exceptionDto);
        }

        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An error occurred.");
    });
});

// Configure swagger
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Logging middleware after exception handler
app.UseMiddleware<Library.API.Middleware.LoggingMiddleware>();
app.MapControllers();

var mongoContext = app.Services.GetRequiredService<MongoContext>();
// mongoContext.CreateCollectionsIfNotExist(); // temporary, one-time creation of collections

// Run seeders at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    await LibrarySeeder.SeedAllAsync(db);
}

app.Run();