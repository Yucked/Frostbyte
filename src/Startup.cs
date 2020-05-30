using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rhapsody.Controllers;
using Rhapsody.Internals.Middlewares;

namespace Rhapsody {
	public sealed class Startup {
		public void ConfigureServices(IServiceCollection services) {
		}

		public void Configure(IApplicationBuilder app) {
			app.UseFileServer();
			app.UseWebSockets();
			app.UseRouting();

			app.UseResponseCaching();
			app.UseResponseCompression();

			app.UseMiddleware<ExceptionMiddleware>();
			app.UseMiddleware<AuthenticationMiddleware>();
			
			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
				endpoints.MapConnectionHandler<WebSocketHandler>("/player/{guildId}");
			});
		}
	}
}