using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Rhapsody.WS {
	public sealed class WebSocketConnection {
		public string Endpoint { get; }
		public ulong UserId { get; }
		public WebSocket WebSocket { get; }

		private readonly ILogger _logger;

		public WebSocketConnection(WebSocket webSocket, string endpoint, ulong userId, ILogger logger) {
			WebSocket = webSocket;
			Endpoint = endpoint;
			UserId = userId;
			_logger = logger;
		}

		public async ValueTask OnConnectedAsync() {
			_logger.LogInformation($"WebSocket connection opened from {Endpoint}.");
		}

		public async ValueTask OnDisconnectedAsync() {
			_logger.LogError($"WebSocket connection dropped by {Endpoint}.");
		}

		public async ValueTask OnMessageAsync(PipeReader pipeReader) {
			
		}
	}
}