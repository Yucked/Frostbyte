using System.Collections.Concurrent;
using Concept.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Concept.Logger
{
    public readonly struct ModifiedProvider : ILoggerProvider
    {
        private readonly IConfigurationSection _configuration;
        private readonly ConcurrentDictionary<string, ModifiedLogger> _loggers;
        private readonly LogJob _logger;

        public ModifiedProvider(IConfiguration configuration, LogJob logJob)
        {
            _configuration = configuration.GetSection("LogLevel");
            _loggers = new ConcurrentDictionary<string, ModifiedLogger>();
            _logger = logJob;
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (_loggers.TryGetValue(categoryName, out var logger))
                return logger;

            var category = _configuration.GetSection(categoryName.Split('.')[0]);
            logger = new ModifiedLogger(categoryName, category, _logger);
            _loggers.TryAdd(categoryName, logger);
            return logger;
        }

        public void Dispose()
            => _loggers.Clear();
    }
}