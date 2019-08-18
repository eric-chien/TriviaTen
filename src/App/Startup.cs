using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;

namespace App
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private static string TriviaTenPolicyName = "Trivia-Ten-CORS-Policy";
        private static string SsmPath = "triviaten";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Register swagger generator
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "TriviaTen API", Version = "v1" });
            });

            //Allow client-side-app to access resources
            services.AddCors(options =>
            {
                options.AddPolicy(TriviaTenPolicyName,
                    builder =>
                    {
                        builder.WithOrigins(Configuration[ConfigurationKeys.UiAppUrl])
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            //Compression middleware to compress responses. https://docs.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-2.2
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression();

            //Set default authorization policy for all controllers to require authentication by default
            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Latest);

            //add api versioning
            services.AddApiVersioning();

            //register aws services
            var awsOptions = Configuration.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);

            //register application services
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();

            //Enable middleware to serve swagger-ui
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "TriviaTen Api V1");
            });

            //Enable response compression
            app.UseResponseCompression();

            //Enable authentication
            app.UseAuthentication();

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to configure the app.
        public static void ConfigureAppConfiguration (WebHostBuilderContext context, IConfigurationBuilder builder)
        {
            var baseConfiguration = builder.Build();

            var awsOptions = baseConfiguration.GetAWSOptions();

            var settingsPrefix = context.HostingEnvironment.IsDevelopment() ? $"/development/{SsmPath}" : $"/production/{SsmPath}";

            //Add all triviaten ssm parameters to configuration
            builder.AddSystemsManager(settingsPrefix, awsOptions, TimeSpan.FromDays(1));
        }
    }
}
