using Library.Common.StringConstants;
using Library.UI.Models.String_constant;
using Library.UI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================== SERVICES ==================

builder.Services.AddControllersWithViews();

builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session lifetime
    options.Cookie.HttpOnly = true;                 // secure cookie
    options.Cookie.IsEssential = true;              // bypass consent
});

builder.Services.AddHttpClient("Library.UserApi")
    .ConfigureHttpClient((sp, client) =>
    {
        var apiSettings = sp
            .GetRequiredService<IOptions<ApiSettings>>()
            .Value;

        client.BaseAddress = new Uri(apiSettings.BaseUrl);
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IApiClient, ApiClient>();

// ================== AUTH ==================

// Configure authentication with both Cookies and JWT
builder.Services.AddAuthentication(options =>
{
    // Default to cookies for MVC controllers
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
})
.AddJwtBearer(options =>
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

// ================== AUTHORIZATION ==================

builder.Services.AddAuthorization();

var authBuilder = builder.Services.AddAuthorizationBuilder();

foreach (var perm in PermissionNames.All)
{
    authBuilder.AddPolicy(perm, policy => policy.RequireClaim("Permission", perm));
}

var app = builder.Build();

// ================== PIPELINE ==================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCookiePolicy();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// ================== ROUTING ==================

app.MapStaticAssets();

// Default route (non-area)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
