
using Library.UserAPI.Data;
using Library.UserAPI.Interfaces;
using Library.UserAPI.Models;
using Library.UserAPI.Repositories.UserRepo;
using Library.UserAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Library.UserAPI.Seeder;

var builder = WebApplication.CreateBuilder(args);

//ApplicationDbContext extends IdentityDbContext<ApplicationUser, ApplicationRole, int>
//This wires EF Core + Identity together so you get AspNetUsers, AspNetRoles, etc.

var connectionString = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection"))
{
    Encrypt = false,
    TrustServerCertificate = true
}.ConnectionString;

//Update-Database -Connection "Server=(localdb)\ProjectModels;Database=UserDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True"

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Identity with ApplicationUser + ApplicationRole
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add controllers + JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Swagger setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User API", Version = "v1" });
    c.UseInlineDefinitionsForEnums();
});


// Register repositories and services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Optional: custom password hasher
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

// Authentication + Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "LocalJWT";
    options.DefaultChallengeScheme = "LocalJWT";
})
.AddJwtBearer("LocalJWT", options =>
{
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
        )
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // Skip the default behavior
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\":\"User not authorized\"}");
        }
    };
});


// Authorization policies
var authBuilder = builder.Services.AddAuthorizationBuilder();

//Automatically add one policy per permission from seeder
foreach (var perm in RolePermissionSeeder.Permissions)
{
    authBuilder.AddPolicy(perm, policy => policy.RequireClaim("Permission", perm));
}


var app = builder.Build();

//Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var db = services.GetRequiredService<ApplicationDbContext>();

    await RolePermissionSeeder.SeedAsync(roleManager, db);
    await UserSeeder.SeedAsync(userManager);
}

// Exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An error occurred in UserAPI.");
    });
});

// Swagger in dev
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();