using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AndWeHaveAPlan.Mimic.AspExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace AndWeHaveAPlan.Mimic.AspPlayground
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<HttpRealWorker>();
            services.AddScopedMimic<IClient, HttpRealWorker>();
            services.AddScopedMimic<IClient, HttpRealWorker>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "AndWeHaveAPlan.Mimic.AspPlayground", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AndWeHaveAPlan.Mimic.AspPlayground v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }

    public interface IClient
    {
        Task Foo(string input, string i2, string i3);
        Task<string> Baz(string input, string i2, string i3);
        string Bar(string input, string i2, string i3);
        int BarI(string input, string i2, string i3);
    }


    public class HttpRealWorker : IMimicWorker
    {
        public async Task<T> Mock<T>(string mockMethodName, MockParameter[] args)
        {
            Console.WriteLine("Common worker: " + mockMethodName);

            foreach (var arg in args)
            {
                Console.WriteLine(arg.ToString());
            }

            return default(T);
        }
    }
}