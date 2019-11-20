using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Concept.Controllers;
using Concept.Entities;
using Concept.Entities.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vysn.Voice.Enums;

namespace Concept.Jobs
{
    public sealed class MetricsJob : BaseJob
    {
        private DateTimeOffset _lastTimestamp;
        private TimeSpan _lastTotalProcessorTime = TimeSpan.Zero;
        private readonly Process _process;
        private readonly WebSocketController _controller;

        public MetricsJob(IConfiguration config, WebSocketController controller, ILogger<MetricsJob> logger)
            : base(logger)
        {
            var delay = config.Get<ApplicationOptions>().CacheOptions.MetricsDelayMs;
            _controller = controller;
            _process = Process.GetCurrentProcess();

        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_controller.Cache.Count <= 0)
                {
                    await Task.Delay(Delay)
                        .ConfigureAwait(false);

                    continue;
                }

                var metrics = BuildMetrics();
                await _controller.SendToAllAsync(metrics)
                    .ConfigureAwait(false);

                await Task.Delay(Delay)
                    .ConfigureAwait(false);
            }
        }

        private ApplicationMetric BuildMetrics()
        {
            var totalCpuTimeUsed = _process.TotalProcessorTime.TotalMilliseconds -
                                   _lastTotalProcessorTime.TotalMilliseconds;

            _lastTotalProcessorTime = _process.TotalProcessorTime;

            var cpuTimeElapsed = (DateTimeOffset.UtcNow - _lastTimestamp).TotalMilliseconds *
                                 Environment.ProcessorCount;

            _lastTimestamp = DateTimeOffset.UtcNow;
            var totalCpuUsed = totalCpuTimeUsed * 100 / cpuTimeElapsed;


            var clients = _controller.Cache.Clients.Values;
            return new ApplicationMetric
            {
                Uptime = (DateTime.Now - _process.StartTime).Ticks,
                ConnectedClients = _controller.Cache.Count,
                ConnectedVoiceClients = clients.Sum(x => x.GatewayClients.Count),
                Usage = totalCpuUsed,
                ActiveVoiceClients = clients
                    .Sum(x => x.GatewayClients.Values
                        .Count(s => s.State == ConnectionState.Ready))
            };
        }
    }
}