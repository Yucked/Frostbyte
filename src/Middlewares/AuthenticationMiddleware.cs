using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rhapsody.Entities;

namespace Rhapsody.Middlewares {
	public readonly struct AuthenticationMiddleware {
		private readonly RequestDelegate _requestDelegate;
		private readonly ILogger _logger;
		private readonly ApplicationOptions _applicationOptions;

		public AuthenticationMiddleware(RequestDelegate requestDelegate, IConfiguration configuration,
			ILogger<AuthenticationMiddleware> logger) {
			_requestDelegate = requestDelegate;
			_applicationOptions = configuration.Get<ApplicationOptions>();
			_logger = logger;
		}

		public async Task Invoke(HttpContext context) {
			if (_applicationOptions.BlacklistedEndpoints.Contains(context.Request.Path)) {
				await _requestDelegate(context);
				return;
			}

			var headers = context.Request.Headers;
			var remoteEndpoint = $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}";

			if (!headers.TryGetValue("Authorization", out var authorization)) {
				_logger.LogError($"{remoteEndpoint} didn't include authorization headers.");
				context.Response.StatusCode = 401;
				await context.Response.CompleteAsync();
				return;
			}

			if (authorization != _applicationOptions.Authorization) {
				_logger.LogError($"{remoteEndpoint} provided wrong authorization value.");
				context.Response.StatusCode = 401;
				await context.Response.CompleteAsync();
				return;
			}

			await _requestDelegate(context);
		}
	}
}