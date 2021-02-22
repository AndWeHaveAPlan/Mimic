using System;
using AndWeHaveAPlan.Mimic.AspExample.JsonRpcControllers;
using AndWeHaveAPlan.Mimic.AspExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace AndWeHaveAPlan.Mimic.AspExample
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
            services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });

            // Old implementation usage
            // services.AddScoped<IUsefulService, UsefulService>();

            // UsefulStuff migrated to separate service (UsefulStuffController in this example)
            // now we can mimic this interface and pass all IUsefulStuff calls over http
            services.AddHttpClient<JsonRpcRealWorker>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:3000");
            });
            services.AddScopedMimic<IUsefulStuff, JsonRpcRealWorker>();

            
            services.AddControllers();
            services.AddJsonRpc();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "AndWeHaveAPlan.Mimic.AspExample", Version = "v1"});
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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AndWeHaveAPlan.Mimic.AspExample v1"));
            }

            
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            app.UseJsonRpc(builder =>
            {
                builder.AddController<RemoteUsefulStuffServiceController>();
            });


            
        }
    }
}