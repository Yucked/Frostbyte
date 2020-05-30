using System;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rhapsody.Extensions;

namespace Rhapsody.Controllers {
	public sealed class WebSocketHandler : ConnectionHandler {
		private readonly ILogger _logger;
		private readonly IMemoryCache _memoryCache;
		private readonly Pipe _pipe;
		private const int BUFFER_SIZE = 256;

		public WebSocketHandler(ILogger<WebSocketHandler> logger, IMemoryCache memoryCache) {
			_logger = logger;
			_memoryCache = memoryCache;
			_pipe = new Pipe();
		}

		public override async Task OnConnectedAsync(ConnectionContext connection) {
			var httpContext = connection.GetHttpContext();

			if (!httpContext.WebSockets.IsWebSocketRequest) {
				await httpContext.Response.WriteAsync("Only WebSocket requests are allowed at this endpoint.");
				await httpContext.Response.CompleteAsync();
				return;
			}

			if (!httpContext.IsValidRoute(out var guildId)) {
				await httpContext.Response.CompleteAsync();
				return;
			}

			if (!_memoryCache.TryGetValue(guildId, out GuildPlayer guildPlayer)) {
				await httpContext.Response.WriteAsync($"You must send a ConnectPayload to /api/player/{guildId}.");
				await httpContext.Response.CompleteAsync();
				return;
			}

			await httpContext.WebSockets.AcceptWebSocketAsync()
			   .ContinueWith(async task => {
					var webSocket = await task;
					await guildPlayer.OnConnectedAsync(webSocket);
					await HandleConnectionAsync(guildPlayer);
				});
		}

		private async Task HandleConnectionAsync(GuildPlayer guildPlayer) {
			var writer = _pipe.Writer;
			var webSocket = guildPlayer.Socket;

			try {
				do {
					var memory = writer.GetMemory(BUFFER_SIZE);
					var receiveResult = await webSocket.ReceiveAsync(memory, CancellationToken.None);
					if (!receiveResult.EndOfMessage) {
						writer.Advance(receiveResult.Count);
						continue;
					}

					await writer.FlushAsync();
					await guildPlayer.OnMessageAsync(_pipe.Reader);
				} while (webSocket.State == WebSocketState.Open);
			}
			catch (Exception exception) {
				_logger.LogCritical(exception, exception.StackTrace);

				await writer.CompleteAsync(exception);
				await guildPlayer.OnDisconnectedAsync(exception);
			}
		}
	}
}