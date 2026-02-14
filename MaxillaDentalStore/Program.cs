using MaxillaDentalStore.Common.Abstractions;
using MaxillaDentalStore.Common.Authentication;
using MaxillaDentalStore.Common.Helpers;
using MaxillaDentalStore.Data;
using MaxillaDentalStore.Repositories.Implementations;
using MaxillaDentalStore.Repositories.Interfaces;
using MaxillaDentalStore.Repository.Implementations;
using MaxillaDentalStore.Repository.Interfaces;
using MaxillaDentalStore.Services.Implementations;
using MaxillaDentalStore.Services.Interfaces;
using MaxillaDentalStore.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace MaxillaDentalStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============ 1. Configuration ============
            // Bind JwtOptions
            // Bind JwtOptions with secure defaults
            builder.Services.Configure<JwtOptions>(options =>
            {
                builder.Configuration.GetSection("JwtOptions").Bind(options);
                
                if (string.IsNullOrEmpty(options.SigningKey))
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        options.SigningKey = "ThisIsADefaultSecretKeyForDevelopmentOnly123!";
                    }
                }
            });

            // ============ 2. Database Context ============
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ============ 3. Auth & Security Services ============
            builder.Services.AddScoped<IJwtProvider, JwtProvider>();
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IDateTimeProvider, DateTimeHelper>(); // Registered DateTimeHelper

            // ============ 4. Authentication Setup ============
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();
                    
                    // Security Key Validation
                    var signingKey = jwtOptions?.SigningKey;
                    if (string.IsNullOrEmpty(signingKey))
                    {
                        if (builder.Environment.IsDevelopment())
                        {
                            signingKey = "ThisIsADefaultSecretKeyForDevelopmentOnly123!"; // Default for Dev
                            Console.WriteLine("WARNING: Using default Security Key for Development.");
                        }
                        else
                        {
                            throw new InvalidOperationException("JwtOptions:SigningKey is missing in appsettings.json. Cannot start application securely.");
                        }
                    }
                    else if (signingKey.Length < 32)
                    {
                         if (!builder.Environment.IsDevelopment())
                            throw new InvalidOperationException("JwtOptions:SigningKey is too short. It must be at least 32 characters long for HMAC-SHA256.");
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions?.Issuer,
                        ValidAudience = jwtOptions?.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
                    };
                });

            // ============ 5. Repositories ============
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IPackageRepository, PackageRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

            // ============ 6. UnitOfWork ============
            builder.Services.AddScoped<IUnitOfWork, MaxillaDentalStore.UnitOfWork.UnitOfWork>();

            // ============ 7. Domain Services ============
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IPackageService, PackageService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            // ============ 8. Other Services ============
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            
            // Response Caching for performance
            builder.Services.AddResponseCaching();
            
            // Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("CustomerOrAdmin", policy => policy.RequireRole("Customer", "Admin"));
            });
            
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Maxilla Dental Store API", Version = "v1" });

                // Add Security Definition for JWT Bearer
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // Add Security Requirement
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            var app = builder.Build();

            // ============ 9. Middleware Pipeline ============
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Response Caching (before Authorization)
            app.UseResponseCaching();

            // Important: Auth before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
