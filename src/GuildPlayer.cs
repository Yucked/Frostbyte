using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rhapsody.Payloads.Inbound;

namespace Rhapsody {
	public struct GuildPlayer {
		[JsonPropertyName("user_id")]
		public ulong UserId { get; }

		[JsonPropertyName("session_id")]
		public string SessionId { get; }

		[JsonPropertyName("token")]
		public string Token { get; }

		[JsonPropertyName("endpoint")]
		public string Endpoint { get; }

		[JsonPropertyName("client_endpoint")]
		public string Client { get; }

		[JsonIgnore]
		public WebSocket Socket { get; private set; }

		private readonly ILogger _logger;

		public GuildPlayer(ConnectPayload connectPayload, string clientEndpoint, ILogger logger) {
			UserId = connectPayload.UserId;
			SessionId = connectPayload.SessionId;
			Token = connectPayload.Token;
			Endpoint = connectPayload.Endpoint;
			Client = clientEndpoint;
			_logger = logger;
			Socket = default;
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