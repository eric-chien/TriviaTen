using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using App.Managers.Users;
using Microsoft.IdentityModel.Logging;

namespace App
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }
        public bool IsDevelopment => HostingEnvironment.IsDevelopment();
        private static string SsmPath = "triviaten";

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;
            //Register swagger generator
            services.AddSwagger();

            //Allow client-side-app to access resources
            services.AddCorsPolicy(Configuration);

            //Compression middleware to compress responses. https://docs.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-2.2
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression();

            //Add cognito authentication
            services.AddCognitoAuthentication(Configuration, IsDevelopment);

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
            services.AddMvcCore().AddApiExplorer()
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            //register aws services
            var awsOptions = Configuration.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonCognitoIdentityProvider>();
            services.AddAWSService<IAmazonDynamoDB>();

            //register application services
            services.AddSingleton<IUserManager, UserManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();

            //Enable middleware to serve swagger-ui
            app.UseSwaggerUI(options => {
                options.SwaggerEndpoint($"/swagger/v1/swagger.json", "TriviaTen Api V1");
            });

            //Allow client to access resources
            app.UseCors(Constants.Cors.TriviaTenPolicyName);

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
