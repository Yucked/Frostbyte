using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Colorful;
using Microsoft.Extensions.Logging;
using Rhapsody.Extensions;
using Rhapsody.Objects;

namespace Rhapsody.Internals.Logging {
	public struct LoggerProvider : ILoggerProvider {
		private const string MESSAGE_FORMAT = "[{0}] [{1}] [{2}]\n{3}";
		private readonly LogLevel _defaultLevel;
		private readonly IDictionary<string, LogLevel> _filters;
		private readonly ConcurrentQueue<Formatter[]> _queue;
		private readonly ConcurrentDictionary<string, ILogger> _loggers;
		private bool _isDisposed;

		public LoggerProvider(LoggingOptions loggingOptions) {
			_defaultLevel = loggingOptions.DefaultLevel;
			_filters = loggingOptions.Filters;
			_loggers = new ConcurrentDictionary<string, ILogger>();
			_queue = new ConcurrentQueue<Formatter[]>();
			_isDisposed = false;

			_ = RunAsync();
		}

		public readonly ILogger CreateLogger(string categoryName) {
			if (_filters.TryGetFilter(ref categoryName, out var logLevel)) {
			}

			if (_loggers.TryGetValue(categoryName, out var logger)) {
				return logger;
			}


			logger = new ColorfulLogger(categoryName, _defaultLevel, this);
			_loggers.TryAdd(categoryName, logger);
			return logger;
		}

		public void Dispose() {
			_isDisposed = true;
			_loggers.Clear();
		}

		public readonly void Enqueue(Formatter[] formatters) {
			_queue.Enqueue(formatters);
		}

		private readonly async Task RunAsync() {
			while (!_isDisposed) {
				if (!_queue.TryDequeue(out var formatters)) {
					await Task.Delay(5);
					continue;
				}

				Console.WriteLineFormatted(MESSAGE_FORMAT, Color.White, formatters);
				await Task.Delay(5);
			}
		}
	}
}