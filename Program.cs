using DrawingApp.Data; // Veritabanı context'i
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controller'ları ekle
builder.Services.AddControllers();

// DbContext ayarı - SQLite veritabanı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=drawings.db")); // Veritabanı dosyası

// JWT ayarlarını appsettings.json'dan okuyalım
var jwtSettings = builder.Configuration.GetSection("Jwt");
#pragma warning disable CS8604 // Olası null başvuru bağımsız değişkeni.
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);
#pragma warning restore CS8604 // Olası null başvuru bağımsız değişkeni.

// Authentication servisini ekle (JWT Bearer)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Geliştirme için, prod'da true olmalı
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});

// Swagger (API arayüzü) ve CORS ayarları
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS izinleri - frontend erişimi için gerekli
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// CORS middleware'i çağrısı
app.UseCors();

// Geliştirme ortamında Swagger'ı aç
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Authentication ve Authorization middleware'leri
app.UseAuthentication();
app.UseAuthorization();

// Controller yönlendirme
app.MapControllers();

app.Run();
