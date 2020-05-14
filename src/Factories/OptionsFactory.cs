using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Rhapsody.Entities;
using Rhapsody.Extensions;

namespace Rhapsody.Factories {
	public sealed class OptionsFactory {
		public const string FILE_NAME = "options.json";

		public static bool IsCreated
			=> File.Exists(FILE_NAME);

		public static ApplicationOptions Create() {
			var options = new ApplicationOptions {
				Host = "127.0.0.1",
				Port = 9000,
				Authorization = nameof(Rhapsody),
				LogLevel = LogLevel.Trace,
				LogFilters = new Dictionary<string, LogLevel> {
					{
						"System.*", LogLevel.Trace
					}
				},
				AuthEndpoints = new List<string>{
					"/api/search",
					"/ws"
				}
			};

			var serialize = options.Serialize();
			File.WriteAllBytes(FILE_NAME, serialize);

			return options;
		}

		public static ApplicationOptions Load() {
			var bytes = File.ReadAllBytes(FILE_NAME);
			return bytes.Deserialize<ApplicationOptions>();
		}
	}
}