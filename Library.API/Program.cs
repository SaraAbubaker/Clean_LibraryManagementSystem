
using Library.Domain.Data;
using Library.Domain.Repositories;
using Library.Infrastructure.Logging.DTOs;
using Library.Infrastructure.Logging.Interfaces;
using Library.Infrastructure.Logging.Models;
using Library.Infrastructure.Logging.Services;
using Library.Infrastructure.Mongo;
using Library.Infrastructure.RabbitMQ.Configuation;
using Library.Infrastructure.RabbitMQ.Consuming;
using Library.Infrastructure.RabbitMQ.Publishing;
using Library.Infrastructure.RabbitMQ.Services;
using Library.Services.Interfaces;
using Library.Services.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using RabbitMQ.Client;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//MongoDB Guid serialization
BsonSerializer.RegisterSerializer(
    new GuidSerializer(GuidRepresentation.Standard)
);

//Add Dbcontext
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

//Register Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Library API", Version = "v1" });

    //Swagger use enum names in dropdowns instead of numbers
    c.UseInlineDefinitionsForEnums();
});

//Library Services
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserTypeService, UserTypeService>();

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


// RabbitMQ settings binding + validation
builder.Services.AddOptions<RabbitMqSettings>()
    .Bind(builder.Configuration.GetSection("RabbitMqSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Register RabbitMQ IConnection singleton
builder.Services.AddSingleton<IConnection>(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqSettings>>().Value;

    var factory = new ConnectionFactory
    {
        HostName = settings.HostName ?? throw new InvalidOperationException("RabbitMq HostName is missing"),
        UserName = settings.UserName ?? throw new InvalidOperationException("RabbitMq UserName is missing"),
        Password = settings.Password ?? throw new InvalidOperationException("RabbitMq Password is missing"),
        Port = settings.Port
    };

    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// Publisher/consumer/logger services
builder.Services.AddSingleton<LogPublisher>();
builder.Services.AddSingleton<LogConsumer>();
builder.Services.AddSingleton<RabbitMqLoggerService>();

// Hosted service to start consumers automatically
builder.Services.AddHostedService<LogConsumerHostedService>();


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

            var exceptionDto = new ExceptionLogDto
            {
                Guid = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
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


//Configure swagger
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Logging middleware after exception handler
app.UseMiddleware<Library.API.Middleware.LoggingMiddleware>();
app.MapControllers();

var mongoContext = app.Services.GetRequiredService<MongoContext>();
//mongoContext.CreateCollectionsIfNotExist(); //temporary, one-time creation of collections

app.Run();
