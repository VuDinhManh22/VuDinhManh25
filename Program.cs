using System;
using Microsoft.EntityFrameworkCore;
using Training.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Training.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore.Diagnostics; 
using System.Linq; 
using BCrypt.Net;
using Training.Service;
using Training.Services;
using Serilog;
using Serilog.Events;
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Training API", Version = "v1" });

   
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    
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

builder.Services.AddScoped<IAuthService, AuthService>();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))); 


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

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() 
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) 
    .Enrich.FromLogContext()
    .WriteTo.Console() 
    .WriteTo.File(
        "logs/log.txt",
        rollingInterval: RollingInterval.Day, 
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}") // Định dạng output
    .CreateLogger();
    
var app = builder.Build(); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        context.Database.Migrate();

        
        if (!context.Users.Any())
        {
            context.Users.Add(new User { Name = "Alice", Email = "alice@example.com", Password = BCrypt.Net.BCrypt.HashPassword("passwordAlice"), Role = "Admin" });
            context.Users.Add(new User { Name = "Bob", Email = "bob@example.com", Password = BCrypt.Net.BCrypt.HashPassword("passwordBob"), Role = "User" });
            await context.SaveChangesAsync();
            Console.WriteLine("Seeded initial users.");
        }

        
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
        
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
        
    }
}

app.Run();