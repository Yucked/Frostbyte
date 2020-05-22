using System;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rhapsody.Payloads.Inbound;

namespace Rhapsody.Entities {
	public struct GuildPlayer {
		[JsonPropertyName("user_id")]
		public ulong UserId { get; }

		[JsonPropertyName("session_id")]
		public string SessionId { get; }

		[JsonPropertyName("token")]
		public string Token { get; }

		[JsonPropertyName("endpoint")]
		public string Endpoint { get; }

		[JsonPropertyName("remote_endpoint")]
		public string RemoteEndpoint { get; }

		[JsonIgnore]
		public WebSocket Socket { get; private set; }

		private readonly ILogger _logger;

		public GuildPlayer(ConnectPayload connectPayload, string remoteEndpoint, ILogger logger) {
			UserId = connectPayload.UserId;
			SessionId = connectPayload.SessionId;
			Token = connectPayload.Token;
			Endpoint = connectPayload.Endpoint;
			RemoteEndpoint = remoteEndpoint;
			_logger = logger;
			Socket = default;
		}

		public async ValueTask OnConnectedAsync(WebSocket webSocket) {
			Socket = webSocket;
			_logger.LogInformation($"WebSocket connection opened from {RemoteEndpoint}.");
		}

		public async ValueTask OnDisconnectedAsync(Exception exception = default) {
			_logger.LogError($"WebSocket connection dropped by {RemoteEndpoint}.");
			if (Guard.IsSafeMatch(Socket.State, WebSocketState.Connecting, WebSocketState.Open)) {
				await Socket.CloseAsync(WebSocketCloseStatus.InternalServerError, exception?.Message, CancellationToken.None);
			}

			Socket.Dispose();
		}

		public async ValueTask OnMessageAsync(PipeReader pipeReader) {
		}
	}
}