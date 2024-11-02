using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using respNewsV8.Models;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// IConfiguration eriþimi için
var configuration = builder.Configuration;


//JWT kimlik doðrulama yapýlandýrmasý
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
                                            {
                                                ValidateIssuer = true,
                                                ValidateAudience = true,
                                                ValidateLifetime = true,
                                                ValidateIssuerSigningKey = true,
                                                ValidIssuer = configuration["Jwt:Issuer"],
                                                ValidAudience = configuration["Jwt:Audience"],
                                                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"])),
                                                RoleClaimType = ClaimTypes.Role
                                            };
    });

// CORS yapýlandýrmasý
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("https://your-frontend-domain.com")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Yetkilendirme ve diðer servisler
builder.Services.AddAuthorization();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.JsonSerializerOptions.WriteIndented = true;
});
//builder.Services.AddDbContext<RespNewContext>();
builder.Services.AddDbContext<RespNewContext>();
var app = builder.Build();

// Uygulama yapýlandýrmasý
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



public class GPT4Service
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GPT4Service(string apiKey)
    {
        _httpClient = new HttpClient();
        _apiKey = apiKey;
    }

    public async Task<string> GenerateContentAsync(string title)
    {
        var requestBody = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new { role = "user", content = $"Please provide content for the following title: '{title}'." }
            },
            max_tokens = 500
        };

        var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseBody);
            return result.choices[0].message.content;
        }
        else
        {
            throw new Exception($"Error: {response.StatusCode} - {response.ReasonPhrase}");
        }
    }
}