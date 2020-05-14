using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Logging;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Http;

namespace Rhapsody.ConnectionHandlers {
	public sealed class WebSocketHandler : ConnectionHandler {
		private readonly ILogger _logger;
		private readonly PipeWriter _pipeWriter;
		private readonly PipeReader _pipeReader;
		private const int BUFFER_SIZE = 256;

		public WebSocketHandler(ILogger<WebSocketHandler> logger) {
			_logger = logger;
			var pipe = new Pipe();
			_pipeWriter = pipe.Writer;
			_pipeReader = pipe.Reader;
		}

		public override async Task OnConnectedAsync(ConnectionContext connection) {
			var context = connection.GetHttpContext();
			var endpoint = $"{connection.RemoteEndPoint}";

			if (!context.WebSockets.IsWebSocketRequest) {
				_logger.LogError($"{endpoint} request was invalid.");
				connection.Abort();
				return;
			}

			await HandleConnectionAsync(context);
		}

		private async Task HandleConnectionAsync(HttpContext httpContext) {
			try {
				var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
				do {
					var memory = _pipeWriter.GetMemory(BUFFER_SIZE);
					var receiveResult = await webSocket.ReceiveAsync(memory, CancellationToken.None);
					if (!receiveResult.EndOfMessage) {
						_pipeWriter.Advance(receiveResult.Count);
						continue;
					}

					await _pipeWriter.FlushAsync();
				} while (webSocket.State == WebSocketState.Open);
			}
			catch (Exception exception) {
				_logger.LogCritical(exception, exception.StackTrace);
				await _pipeWriter.CompleteAsync(exception);
			}
		}
	}
}