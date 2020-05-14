using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Rhapsody.Entities {
	public sealed class ApplicationOptions {
		public string Host { get; set; }
		public ushort Port { get; set; }
		public string Authorization { get; set; }

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public LogLevel LogLevel { get; set; }

		public IDictionary<string, LogLevel> LogFilters { get; set; }
		public List<string> AuthEndpoints { get; set; }
	}
}