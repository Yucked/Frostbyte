using System.Collections.Generic;
using System.Drawing;
using Microsoft.Extensions.Logging;

namespace Rhapsody.Extensions {
	public static class LoggingExtensions {
		public static Color GetLogLevelColor(this LogLevel logLevel) {
			return logLevel switch {
				LogLevel.Trace       => Color.LightBlue,
				LogLevel.Debug       => Color.PaleVioletRed,
				LogLevel.Information => Color.GreenYellow,
				LogLevel.Warning     => Color.Coral,
				LogLevel.Error       => Color.Crimson,
				LogLevel.Critical    => Color.Red,
				LogLevel.None        => Color.Coral,
				_                    => Color.White
			};
		}

		public static string GetShortLogLevel(this LogLevel logLevel) {
			return logLevel switch {
				LogLevel.Trace       => "TRCE",
				LogLevel.Debug       => "DBUG",
				LogLevel.Information => "INFO",
				LogLevel.Warning     => "WARN",
				LogLevel.Error       => "EROR",
				LogLevel.Critical    => "CRIT",
				LogLevel.None        => "NONE",
				_                    => "UKON"
			};
		}

		public static bool TryGetFilter(this IDictionary<string, LogLevel> filters, ref string category,
			out LogLevel logLevel) {
			foreach (var (filter, level) in filters) {
				if (!filter.IsMatchFilter(ref category)) {
					continue;
				}

				logLevel = level;
				break;
			}

			logLevel = LogLevel.Trace;
			return true;
		}
	}
}