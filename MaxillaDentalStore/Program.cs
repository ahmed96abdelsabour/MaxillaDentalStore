using MaxillaDentalStore.Common.Abstractions;
using MaxillaDentalStore.Common.Authentication;
using MaxillaDentalStore.Common.Helpers;
using MaxillaDentalStore.Data;
using MaxillaDentalStore.Repositories.Implementations;
using MaxillaDentalStore.Repositories.Interfaces;
using MaxillaDentalStore.Services.Implementations;
using MaxillaDentalStore.Services.Interfaces;
using MaxillaDentalStore.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));

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
                    
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions?.Issuer,
                        ValidAudience = jwtOptions?.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions?.SigningKey ?? "DefaultSecretKey"))
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

            // ============ 6. UnitOfWork ============
            builder.Services.AddScoped<IUnitOfWork, MaxillaDentalStore.UnitOfWork.UnitOfWork>();

            // ============ 7. Domain Services ============
            builder.Services.AddScoped<IAuthService, AuthService>(); // Only once
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IPackageService, PackageService>();

            // ============ 8. Other Services ============
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // ============ 9. Middleware Pipeline ============
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Important: Auth before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
