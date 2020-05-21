using System.ComponentModel.DataAnnotations;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Rhapsody {
	public struct GuildPlayer {
		[Required, MaxLength(19)]
		[JsonPropertyName("user_id")]
		public ulong UserId { get; private set; }

		[Required]
		[JsonPropertyName("session_id")]
		public string SessionId { get; private set; }

		[Required]
		[JsonPropertyName("token")]
		public string Token { get; private set; }

		[Required]
		[JsonPropertyName("endpoint")]
		public string Endpoint { get; private set; }

		[Required]
		[JsonPropertyName("client_endpoint")]
		public string ClientEndpoint { get; private set; }

		[JsonIgnore]
		public WebSocket Socket { get; private set; }

		private readonly ILogger _logger;

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