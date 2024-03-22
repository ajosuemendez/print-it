using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrintIt.Core;
using Microsoft.OpenApi.Models;


namespace PrintIt.ServiceHost
{
    [ExcludeFromCodeCoverage]
    internal sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddPrintIt();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PrintIt API", Version = "v1" });
            });

            services.AddRouting();
            services.AddControllers();
            services.AddCors();
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseCors(builder => builder.AllowAnyOrigin());
            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PrintIt API v1");
                c.RoutePrefix = "doc"; // Endpoint for swagger
            });
            
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
