using App;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CognitoExtensions
    {
        public static IServiceCollection AddCognitoAuthentication(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
        {
            if (isDevelopment)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(options =>
                {
                    options.Authority = configuration[ConfigurationKeys.CognitoAuthorityUrl];
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new IdentityModel.Tokens.TokenValidationParameters()
                    {
                        NameClaimType = ClaimTypes.NameIdentifier,
                        ValidateAudience = false //TODO ... Configure cognito access token to include audience
                    };
                });
            }
            else
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(options =>
                {
                    options.Authority = configuration[ConfigurationKeys.CognitoAuthorityUrl];
                    options.TokenValidationParameters = new IdentityModel.Tokens.TokenValidationParameters()
                    {
                        NameClaimType = ClaimTypes.NameIdentifier,
                        ValidateAudience = false //TODO ... Configure cognito access token to include audience
                    };
                });
            }
            
            return services;
        }
    }
}
