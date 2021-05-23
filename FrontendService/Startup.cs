using System;

using FrontendService.ApiClients;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace FrontendService
{
    public class Startup
    {
        private const string BooksApiUrlConfigKey = "BooksApiUrl";
        private const string AuthorsApiConfigKey = "AuthorsApiUrl";

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BooksService", Version = "v1" });
            });

            var booksApiUrl = this.Configuration.GetValue<string>(Startup.BooksApiUrlConfigKey);
            services.AddHttpClient(BooksApiClient.Name, client => client.BaseAddress = new Uri(booksApiUrl));

            var authorsApiUrl = this.Configuration.GetValue<string>(Startup.AuthorsApiConfigKey);
            services.AddHttpClient(AuthorsApiClient.Name, client => client.BaseAddress = new Uri(authorsApiUrl));

            services.AddLogging();
            services.AddSingleton<AuthorsApiClient>();
            services.AddSingleton<BooksApiClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BooksService v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}