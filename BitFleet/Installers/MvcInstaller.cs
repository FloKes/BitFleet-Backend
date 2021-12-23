using System.Text;
using BitFleet.Options;
using BitFleet.Services;
using BitFleet.Services.Interfaces;
using BitFleet.Validators.ValidationFilters;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BitFleet.Installers
{
    public class MvcInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var jwtOptions = new JwtOptions();
            configuration.Bind(nameof(jwtOptions), jwtOptions);
            services.AddSingleton(jwtOptions);

            services.AddScoped<IIdentityService, IdentityService>();

            services.AddMvc(options =>
                {
                    options.EnableEndpointRouting = false;
                    options.Filters.Add<ValidationFilter>();
                })
                .AddFluentValidation(mvcConfiguration =>
                    mvcConfiguration.RegisterValidatorsFromAssemblyContaining<Startup>())
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.Secret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = false,
                ValidateLifetime = true
            };

            services.AddSingleton(tokenValidationParameters);

            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.SaveToken = true;
                    x.TokenValidationParameters = tokenValidationParameters;
                });

            services.AddSingleton<IUriService>(provider =>
                {
                    var accessor = provider.GetRequiredService<IHttpContextAccessor>();
                    var request = accessor.HttpContext.Request;
                    var absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), "/");
                    return new UriService(absoluteUri);
                }
            );

            services.AddScoped<ICarService, CarService>();

            services.AddScoped<IRideService, RideService>();

            services.AddScoped<IMalfunctionService, MalfunctionService>();

            services.AddScoped<IVehicleServiceService, VehicleServiceService>();
        }
    }
}