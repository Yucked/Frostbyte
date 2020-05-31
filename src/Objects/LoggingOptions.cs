using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Rhapsody.Objects {
	public struct LoggingOptions {
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public LogLevel DefaultLevel { get; set; }

		public IDictionary<string, LogLevel> Filters { get; set; }
	}
}