using System.ComponentModel.DataAnnotations;

namespace Rhapsody.Payloads.Inbound {
	public sealed class ConnectPayload : BasePayload {
		[Required]
		public ulong UserId { get; }
		
		[Required]
		public string SessionId { get; }
		
		[Required]
		public string Token { get; }

		[Required]
		public string Endpoint { get; }
	}
}