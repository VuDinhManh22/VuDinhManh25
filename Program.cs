using System;
using Microsoft.EntityFrameworkCore;
using Training.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Training.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore.Diagnostics; // Thêm cho ConfigureWarnings
using System.Linq; // Thêm cho .Any()
using BCrypt.Net;
using Training.Service;
using Training.Services; // Thêm cho BCrypt.Net.BCrypt.HashPassword trong Seed Data

var builder = WebApplication.CreateBuilder(args);

// --- TẤT CẢ CÁC ĐĂNG KÝ DỊCH VỤ PHẢI NẰM Ở ĐÂY ---

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Training API", Version = "v1" });

    // Thêm Security Definition cho JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Thêm Security Requirement để áp dụng Bearer cho tất cả các endpoint mặc định
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Đăng ký AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// Cấu hình DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))); // Giữ lại cấu hình này

// Cấu hình JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build(); // Dòng này phải ở SAU TẤT CẢ các đăng ký dịch vụ

// --- CÁC CẤU HÌNH MIDDLEWARE (PIPEPLINE) NẰM Ở ĐÂY ---

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Thêm Authentication và Authorization Middleware TRƯỚC MapControllers
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed Data logic - ĐẶT ĐOẠN NÀY SAU app.MapControllers() và TRƯỚC app.Run()
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Đảm bảo database đã được tạo và các migration đã được áp dụng
        context.Database.Migrate();

        // Seed users if none exist
        if (!context.Users.Any())
        {
            context.Users.Add(new User { Name = "Alice", Email = "alice@example.com", Password = BCrypt.Net.BCrypt.HashPassword("passwordAlice"), Role = "Admin" });
            context.Users.Add(new User { Name = "Bob", Email = "bob@example.com", Password = BCrypt.Net.BCrypt.HashPassword("passwordBob"), Role = "User" });
            await context.SaveChangesAsync();
            Console.WriteLine("Seeded initial users.");
        }

        // Seed products if none exist
        if (!context.Products.Any())
        {
            context.Products.Add(new Product { Id = 101, Name = "Laptop", Price = 1200.00M, Description = "Powerful laptop for work and gaming" });
            context.Products.Add(new Product { Id = 102, Name = "Mouse", Price = 25.50M, Description = "Wireless ergonomic mouse" });
            await context.SaveChangesAsync();
            Console.WriteLine("Seeded initial products.");
        }
    }
    catch (Exception ex)
    {
        // Log lỗi nếu có vấn đề khi seed database
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
        // Nếu bạn đã cấu hình ILogger, có thể sử dụng:
        // var logger = services.GetRequiredService<ILogger<Program>>();
        // logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();