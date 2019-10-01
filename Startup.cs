using System;
using Concept.Caches;
using Concept.Controllers;
using Concept.Middlewares;
using Concept.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Theory;

namespace Concept
{
    public sealed class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationOptions _options;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _options = _configuration.Get<ApplicationOptions>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ClientsCache>();
            services.AddSingleton<Theoretical>();
            services.AddSingleton<WebSocketController>();
            services.Configure<ApplicationOptions>(_configuration);
            services.AddControllers();
            services.AddAuthentication("HeaderAuth")
                .UseHeaderAuthentication(options => options.Authorization = _options.Authorization);

            if (_options.CacheOptions.IsEnabled)
                services.AddSingleton<ResponsesCache>();
        }

        public void Configure(IApplicationBuilder app, IServiceProvider provider)
        {
            app.UseWebSockets();
            app.UseRouting();

            app.UseMiddleware(typeof(ExceptionMiddleware));
            app.UseMiddleware(typeof(WebSocketMiddleware), _options.Authorization);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            if (_options.CacheOptions.IsEnabled && _options.CacheOptions.ExpiresAfter > 0)
                _ = provider.GetRequiredService<ResponsesCache>()
                    .AutoPurgeAsync();
        }
    }
}