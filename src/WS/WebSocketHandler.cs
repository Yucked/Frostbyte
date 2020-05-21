using System;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rhapsody.Extensions;

namespace Rhapsody.WS {
	[Route("/ws/{guildId}")]
	public sealed class WebSocketHandler : ConnectionHandler {
		private readonly ILogger _logger;
		private readonly ILoggerFactory _loggerFactory;
		private readonly IMemoryCache _memoryCache;
		private readonly Pipe _pipe;
		private const int BUFFER_SIZE = 256;

		private readonly ConcurrentDictionary<string, WebSocketConnection> _connections;

		public WebSocketHandler(ILogger<WebSocketHandler> logger, ILoggerFactory loggerFactory, IMemoryCache memoryCache) {
			_logger = logger;
			_loggerFactory = loggerFactory;
			_memoryCache = memoryCache;
			_connections = new ConcurrentDictionary<string, WebSocketConnection>();

			_pipe = new Pipe();
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
			var writer = _pipe.Writer;
			var webSocket = webSocketConnection.WebSocket;

			try {
				do {
					var memory = writer.GetMemory(BUFFER_SIZE);
					var receiveResult = await webSocket.ReceiveAsync(memory, CancellationToken.None);
					if (!receiveResult.EndOfMessage) {
						writer.Advance(receiveResult.Count);
						continue;
					}

					await writer.FlushAsync();
					await webSocketConnection.OnMessageAsync(_pipe.Reader);
				} while (webSocket.State == WebSocketState.Open);
			}
			catch (Exception exception) {
				_logger.LogCritical(exception, exception.StackTrace);

				await writer.CompleteAsync(exception);
				await webSocketConnection.OnDisconnectedAsync();

				_connections.TryRemove(webSocketConnection.Endpoint, out _);
			}
		}
	}
}