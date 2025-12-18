
using Library.Infrastructure.Logging.Interfaces;
using Library.Infrastructure.Logging.Services;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Library.Infrastructure.Mongo;
using Library.Domain.Repositories;
using Library.Services.Interfaces;
using Library.Services.Services;
using Library.Domain.Data;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

//Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var mongoContext = app.Services.GetRequiredService<MongoContext>();
//mongoContext.CreateCollectionsIfNotExist(); //temporary, one-time creation of collections

app.Run();
