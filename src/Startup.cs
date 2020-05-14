using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rhapsody.ConnectionHandlers;
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
			//app.UseMiddleware<WebSocketMiddleware>();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
				endpoints.MapConnectionHandler<WebSocketHandler>("/ws");
			});
		}
	}
}