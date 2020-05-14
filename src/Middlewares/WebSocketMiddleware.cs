using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Rhapsody.Middlewares {
	public readonly struct WebSocketMiddleware {
		private readonly RequestDelegate _requestDelegate;
		private readonly ILogger<WebSocketMiddleware> _logger;

		public WebSocketMiddleware(RequestDelegate requestDelegate, ILogger<WebSocketMiddleware> logger) {
			_requestDelegate = requestDelegate;
			_logger = logger;
		}

		public async Task Invoke(HttpContext context) {
			if (!context.WebSockets.IsWebSocketRequest) {
				await _requestDelegate(context);
				return;
			}

			var remoteEndpoint = $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}";
			var webSocket = await context.WebSockets.AcceptWebSocketAsync();

			await ReceiveAsync(webSocket);
		}

		private async Task ReceiveAsync(WebSocket webSocket) {
			try {
				var buffer = new byte[256];

				do {
					var receiveResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
					if (!receiveResult.EndOfMessage) {
						continue;
					}

					_logger.LogInformation(Encoding.UTF8.GetString(buffer));
				} while (webSocket.State == WebSocketState.Open);
			}
			catch (Exception exception) {
				_logger.LogCritical(exception, exception.StackTrace);
			}
		}
	}
}