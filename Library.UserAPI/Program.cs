using Library.Domain.Data;
using Library.Domain.Repositories;
using Library.Services.Interfaces;
using Library.Services.Services;
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
using System.Text.Json.Serialization;

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
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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

// Optional: custom password hasher
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

// Authentication + Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
});

// 🔐 Add authorization policies here
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"))
    .AddPolicy("RequireManageUserTypes", policy =>
        policy.RequireClaim("Permission", "ManageUserTypes"))
    .AddPolicy("AdminWithPermission", policy =>
        policy.RequireRole("Admin")
              .RequireClaim("Permission", "ManageUserTypes"));

var app = builder.Build();

// --- Runtime seeding block ---
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    // Ensure Admin role exists
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new ApplicationRole { Name = "Admin", NormalizedName = "ADMIN" });
    }

    // Ensure Normal role exists
    if (!await roleManager.RoleExistsAsync("Normal"))
    {
        await roleManager.CreateAsync(new ApplicationRole { Name = "Normal", NormalizedName = "NORMAL" });
    }

    // Ensure Admin user exists
    var adminUser = await userManager.FindByNameAsync("admin");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@library.local",
            EmailConfirmed = true,
            CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsArchived = false,
            SecurityStamp = Guid.NewGuid().ToString("D")
        };
        await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
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
