using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rhapsody.Middlewares;

namespace Rhapsody {
	public sealed class Startup {
		public void ConfigureServices(IServiceCollection services) {
		}

		public void Configure(IApplicationBuilder app, IHostApplicationLifetime hostApplicationLifetime) {
			hostApplicationLifetime.ApplicationStarted.Register(OnStartup);
			hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
			hostApplicationLifetime.ApplicationStopped.Register(OnStopped);

			app.UseRouting();
			app.UseWebSockets();

			app.UseMiddleware<ExceptionMiddleware>();
			app.UseMiddleware<AuthenticationMiddleware>();
			app.UseMiddleware<WebSocketMiddleware>();

			app.UseEndpoints(endpoints => endpoints.MapControllers());
		}


		private void OnStartup() {
		}

		private void OnStopping() {
		}

		private void OnStopped() {
		}
	}
}