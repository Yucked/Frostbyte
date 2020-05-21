using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rhapsody.Controllers;
using Rhapsody.Middlewares;

namespace Rhapsody {
	public sealed class Startup {
		public void ConfigureServices(IServiceCollection services) {
		}

		public void Configure(IApplicationBuilder app) {
			app.UseRouting();
			app.UseWebSockets();
			app.UseFileServer();

			app.UseMiddleware<ExceptionMiddleware>();
			app.UseMiddleware<AuthenticationMiddleware>();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
				endpoints.MapConnectionHandler<WebSocketHandler>("/ws/{guildId}");
			});
		}
	}
}