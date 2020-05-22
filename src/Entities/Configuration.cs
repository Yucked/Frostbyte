using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Rhapsody.Extensions;

namespace Rhapsody.Entities {
	public sealed class Configuration {
		public string Host { get; set; }
		public ushort Port { get; set; }
		public string Authorization { get; set; }

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public LogLevel LogLevel { get; set; }

		public IDictionary<string, LogLevel> LogFilters { get; set; }
		public List<string> AuthEndpoints { get; set; }


		public const string FILE_NAME = "options.json";

		[JsonIgnore]
		public static bool IsCreated
			=> File.Exists(FILE_NAME);

		public static Configuration Create() {
			var options = new Configuration {
				Host = "127.0.0.1",
				Port = 9000,
				Authorization = nameof(Rhapsody),
				LogLevel = LogLevel.Trace,
				LogFilters = new Dictionary<string, LogLevel> {
					{
						"System.*", LogLevel.Trace
					}
				},
				AuthEndpoints = new List<string> {
					"/api/search",
					"/ws"
				}
			};

			var serialize = options.Serialize();
			File.WriteAllBytes(FILE_NAME, serialize);

			return options;
		}

		public static Configuration Load() {
			var bytes = File.ReadAllBytes(FILE_NAME);
			return bytes.Deserialize<Configuration>();
		}
	}
}