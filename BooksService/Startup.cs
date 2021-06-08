using System;

using AuthorsService.Grpc;

using BooksService.ApiClients;
using BooksService.Controllers;
using BooksService.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace BooksService
{
    public class Startup
    {
        private const string AuthorsRestApiConfigKey = "AuthorsRestApiUrl";
        private const string AuthorsGrpcApiConfigKey = "AuthorsGrpcApiUrl";

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<BookContext>(builder => builder.UseInMemoryDatabase("BooksDb"),
                ServiceLifetime.Singleton);
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "BooksService", Version = "v1"});
            });
            services.AddGrpc();

            var authorsRestApiUrl = this.Configuration.GetValue<string>(Startup.AuthorsRestApiConfigKey);
            services.AddHttpClient(AuthorsApiClient.Name, client => client.BaseAddress = new Uri(authorsRestApiUrl));
            services.AddSingleton<AuthorsApiClient>();

            var authorsGrpcApiUrl = this.Configuration.GetValue<string>(Startup.AuthorsGrpcApiConfigKey);
            services.AddGrpcClient<AuthorsServiceProto.AuthorsServiceProtoClient>(options =>
                options.Address = new Uri(authorsGrpcApiUrl));
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<BooksGrpcService>();
            });
        }
    }
}