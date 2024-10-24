using AutoMapper;
using L_L.API.Handler;
using L_L.Business.Mappers;
using L_L.Business.Middlewares;
using L_L.Business.Services;
using L_L.Business.Ultils;
using L_L.Data.Base;
using L_L.Data.Entities;
using L_L.Data.SeedData;
using L_L.Data.UnitOfWorks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace L_L.API.Extensions
{

    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ExceptionMiddleware>();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            //Add Mapper
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new ProfilesMapper());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            //Set time
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // jwt
            var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

            // zalo pay
            var zaloPaySetting = configuration.GetSection(nameof(ZaloPaySetting)).Get<ZaloPaySetting>();
            services.Configure<ZaloPaySetting>(configuration.GetSection(nameof(ZaloPaySetting)));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<ZaloPaySetting>>().Value);

            // payOs
            var payOsSetting = configuration.GetSection(nameof(PayOSSetting)).Get<PayOSSetting>();
            services.Configure<PayOSSetting>(configuration.GetSection(nameof(PayOSSetting)));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<PayOSSetting>>().Value);

            // mail
            services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));

            // cloud
            services.Configure<CloundSettings>(configuration.GetSection(nameof(CloundSettings)));

            /*            services.AddAuthorization();*/

            // set policy prn
            services.AddAuthorization(options =>
            {
                // Define policy role for Admin
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                // Define policy claim for Admin
                options.AddPolicy("CustomerOnly", policy => policy.RequireClaim("IsCustomer", "Customer"));
                // handler policy
                options.AddPolicy("AdminHandler", policy =>
                    policy.Requirements.Add(new AdminRequirement("Admin")));
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                    {
                        options.Authority = "https://dev-urvottxjuerzz313.us.auth0.com/";
                        options.Audience = "L&L";
                    }
                });

            services.AddDbContext<AppDbContext>(opt =>
            {
                var connectionString = configuration.GetConnectionString("PgDbConnection");
                Console.WriteLine($"PgDbConnection DbConnect: {connectionString}");
                opt.UseNpgsql(connectionString);
            });

            /*Config repository*/
            services.AddScoped(typeof(IRepository<,>), typeof(GenericRepository<,>));

            /*Register UnitOfWorks*/
            services.AddScoped<UnitOfWorks>();

            /*Init Data*/
            services.AddScoped<DatabaseInitialiser>();

            /*Config Service*/
            services.AddScoped<UserService>();
            services.AddScoped<AuthService>();
            services.AddScoped<MailService>();
            services.AddScoped<PackageTypeService>();
            services.AddScoped<VehicleTypeService>();
            services.AddScoped<OrderService>();
            services.AddScoped<OrderDetailService>();
            services.AddScoped<CloudService>();
            services.AddScoped<ProductService>();
            services.AddScoped<DeliveryInfoService>();
            services.AddScoped<TruckSevice>();
            services.AddScoped<IdentityCardService>();
            services.AddScoped<LicenseDriverService>();
            services.AddScoped<GuessService>();
            services.AddScoped<TransactionService>();

            services.AddSingleton<IAuthorizationHandler, AdminHandler>();
            return services;
        }
    };
}
