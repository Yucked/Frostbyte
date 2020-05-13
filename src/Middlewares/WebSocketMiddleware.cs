using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Rhapsody.Controllers;

namespace Rhapsody.Middlewares {
	public readonly struct WebSocketMiddleware {
		private readonly RequestDelegate _requestDelegate;
		private readonly ILogger<WebSocketMiddleware> _logger;

		public WebSocketMiddleware(RequestDelegate requestDelegate, ILogger<WebSocketMiddleware> logger) {
			_requestDelegate = requestDelegate;
			_logger = logger;
		}

		public async Task Invoke(HttpContext context, WebSocketController controller) {
			if (!context.WebSockets.IsWebSocketRequest) {
				await _requestDelegate(context);
				return;
			}

			if (!IsValidRequest(context, out var userId)) {
				return;
			}

			var remoteEndpoint = $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}";
			var webSocket = await context.WebSockets.AcceptWebSocketAsync();
		}

		private async Task ReceiveAsync(WebSocketController controller) {
			try {
			}
			catch (Exception exception) {
				_logger.LogCritical(exception, exception.StackTrace);
			}
		}

		private static bool IsValidRequest(HttpContext context, out ulong userId) {
			if (!context.Request.Headers.TryGetValue("User-Id", out var id)) {
				context.Response.StatusCode = 403;
				userId = default;
				return false;
			}

			if (ulong.TryParse(id, out userId))
				return true;

			context.Response.StatusCode = 403;
			return false;
		}
	}
}