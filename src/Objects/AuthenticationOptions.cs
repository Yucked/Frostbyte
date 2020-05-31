using System.Collections.Generic;

namespace Rhapsody.Objects {
	public sealed class AuthenticationOptions {
		public string Password { get; set; }
		public List<string> Endpoints { get; set; }
	}
}