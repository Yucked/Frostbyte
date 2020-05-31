using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rhapsody.Objects {
	public sealed class ApplicationOptions {
		public EndpointOptions Endpoint { get; set; }
		public AuthenticationOptions Authentication { get; set; }
		public LoggingOptions Logging { get; set; }
		public IDictionary<string, bool> Providers { get; set; }

		[JsonIgnore]
		public string Url
			=> $"{Endpoint.Host}:{Endpoint.Port}";

		public const string FILE_NAME = "configuration.json";
	}
}