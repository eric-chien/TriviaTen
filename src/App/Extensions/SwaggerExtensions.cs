using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "TriviaTen API", Version = "v1" });

                var apiKeyScheme = new ApiKeyScheme
                {
                    In = "header",
                    Description = "Enter authorization key in the format: 'Bearer {ACCESS TOKEN}'",
                    Name = "Authorization",
                    Type = "apiKey"
                };

                options.AddSecurityDefinition("Bearer", apiKeyScheme);

                options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", Enumerable.Empty<string>() }
                });
            });

            return services;
        }
    }
}
