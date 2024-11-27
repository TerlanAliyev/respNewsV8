using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using respNewsV8.Models;
using respNewsV8.Services;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Hizmetlerin eklenmesi

// Unsplash servisi yapýlandýrmasý
builder.Services.AddHttpClient<UnsplashService>();
builder.Services.Configure<UnsplashOptions>(builder.Configuration.GetSection("Unsplash"));

// Rate limiting yapýlandýrmasý
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("FixedWindow", _options =>
    {
        _options.PermitLimit = 10;
        _options.Window = TimeSpan.FromMinutes(1);
    });
});

// JWT kimlik doðrulama yapýlandýrmasý
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
            RoleClaimType = ClaimTypes.Role
        };
    });

// CORS yapýlandýrmasý
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
        builder.WithOrigins("https://your-frontend-domain.com")
            .AllowAnyHeader()
            .AllowAnyMethod());
    options.AddDefaultPolicy(builder =>
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Yetkilendirme
builder.Services.AddAuthorization();

// Controller'lar için JSON yapýlandýrmasý (Newtonsoft.Json)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // Opsiyonel: Camel case kullanýmý
    });



// DbContext ekleme
builder.Services.AddDbContext<RespNewContext>();

// Uygulama oluþturuluyor
var app = builder.Build();

// Rate Limiter'ý uygula
app.UseRateLimiter();

// Hata ayýklama ortamý
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Gerekli middleware'ler
app.UseRouting();
app.UseCors();

// Kimlik doðrulama ve yetkilendirme
app.UseAuthentication();
app.UseAuthorization();

// API controller'larýný haritalama
app.MapControllers();

// Uygulama çalýþtýrýlýyor
app.Run();
