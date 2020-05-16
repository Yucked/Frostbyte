using System;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Logging;
using Rhapsody.Extensions;

namespace Rhapsody.WS {
	public sealed class WebSocketHandler : ConnectionHandler {
		private readonly ILogger _logger;
		private readonly ILoggerFactory _loggerFactory;
		private readonly PipeWriter _pipeWriter;
		private readonly PipeReader _pipeReader;
		private const int BUFFER_SIZE = 256;

		private readonly ConcurrentDictionary<string, WebSocketConnection> _connections;

		public WebSocketHandler(ILogger<WebSocketHandler> logger, ILoggerFactory loggerFactory) {
			_logger = logger;
			_loggerFactory = loggerFactory;
			_connections = new ConcurrentDictionary<string, WebSocketConnection>();
			
			var pipe = new Pipe();
			_pipeWriter = pipe.Writer;
			_pipeReader = pipe.Reader;
		}

		public override async Task OnConnectedAsync(ConnectionContext connection) {
			var httpContext = connection.GetHttpContext();
			var endpoint = $"{connection.RemoteEndPoint}";

			if (!httpContext.WebSockets.IsWebSocketRequest) {
				await httpContext.Response.WriteAsync("Only WebSocket requests are allowed at this endpoint.");
				await httpContext.Response.CompleteAsync();
				return;
			}

			if (!httpContext.IsValidRequest(out var userId)) {
				await httpContext.Response.CompleteAsync();
				return;
			}

			await httpContext.WebSockets.AcceptWebSocketAsync()
			   .ContinueWith(async task => {
					var webSocket = await task;
					var socketConnection =
						new WebSocketConnection(webSocket, endpoint, userId, _loggerFactory.CreateLogger<WebSocketConnection>());

					_connections.TryAdd(endpoint, socketConnection);
					await socketConnection.OnConnectedAsync();
					await HandleConnectionAsync(socketConnection);
				});
		}

		private async Task HandleConnectionAsync(WebSocketConnection webSocketConnection) {
			try {
				var webSocket = webSocketConnection.WebSocket;
				do {
					var memory = _pipeWriter.GetMemory(BUFFER_SIZE);
					var receiveResult = await webSocket.ReceiveAsync(memory, CancellationToken.None);
					if (!receiveResult.EndOfMessage) {
						_pipeWriter.Advance(receiveResult.Count);
						continue;
					}

					await _pipeWriter.FlushAsync();
					var readResult = await _pipeReader.ReadAsync();
					await webSocketConnection.OnMessageAsync();
				} while (webSocket.State == WebSocketState.Open);
			}
			catch (Exception exception) {
				_logger.LogCritical(exception, exception.StackTrace);
				
				await _pipeWriter.CompleteAsync(exception);
				await webSocketConnection.OnDisconnectedAsync();

				_connections.TryRemove(webSocketConnection.Endpoint, out _);
			}
		}
	}
}