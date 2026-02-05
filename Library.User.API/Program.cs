using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Library.Common.StringConstants;
using Library.User.Services.Interfaces;
using Library.User.Domain.Repositories.UserRepo;
using Library.User.Domain.Data;
using Library.User.Entities;
using Library.User.Services.Services;
using Library.User.Seeder;

var builder = WebApplication.CreateBuilder(args);

//ApplicationDbContext extends IdentityDbContext<ApplicationUser, ApplicationRole, int>
//This wires EF Core + Identity together so you get AspNetUsers, AspNetRoles, etc.

var connectionString = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
    (builder.Configuration.GetConnectionString("DefaultConnection"))
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
builder.Services.AddScoped<IUserTypeService, UserTypeService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Optional: custom password hasher
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

// Authentication + Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // "Bearer"
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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

// Exception handler (only in non-dev)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An error occurred in UserAPI.");
        });
    });
}

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
