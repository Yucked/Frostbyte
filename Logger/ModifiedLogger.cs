using System;
using Concept.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Concept.Logger
{
    public sealed class ModifiedLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly IConfigurationSection _section;
        private readonly LogJob _logger;

        public ModifiedLogger(string categoryName, IConfigurationSection section, LogJob logger)
        {
            _categoryName = categoryName;
            _section = section;
            _logger = logger;
        }

        public IDisposable BeginScope<TState>(TState state)
            => default;

        public bool IsEnabled(LogLevel logLevel)
        {
            // TODO: Check logging properly.
            var level = (LogLevel)Enum.Parse(typeof(LogLevel), _section.Value);
            return level <= logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            try
            {
                //IFeatureCollection disposing here, but with that try catch the log continues normal
                var message = formatter(state, exception);
                if (string.IsNullOrWhiteSpace(message))
                    return;

                _logger.WriteLog.Invoke(message, _categoryName, logLevel);
            }
            catch { }
        }
    }
}