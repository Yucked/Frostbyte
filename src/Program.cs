using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rhapsody.Entities;
using Rhapsody.Extensions;
using Rhapsody.Logging;

namespace Rhapsody {
	public readonly struct Program {
		public static async Task Main() {
			try {
				MiscExtensions.SetupApplicationInformation();

				var options = Configuration.IsCreated
					? Configuration.Load()
					: Configuration.Create();

				await Host.CreateDefaultBuilder()
				   .ConfigureAppConfiguration(x => {
						x.SetBasePath(Directory.GetCurrentDirectory());
						x.AddJsonFile(Configuration.FILE_NAME, false, true);
					})
				   .ConfigureWebHostDefaults(webBuilder => {
						webBuilder.UseUrls($"http://{options.Host}:{options.Port}");
						webBuilder.UseKestrel();
						webBuilder.UseStartup<Startup>();
					})
				   .ConfigureLogging(logging => {
						logging.SetMinimumLevel(options.LogLevel);
						logging.ClearProviders();
						logging.AddProvider(new LoggerProvider(options.LogLevel));
					})
				   .ConfigureServices((context, collection) => {
						collection.AddConnections();
						collection.AddControllers();
						collection.AddHttpClient();
						collection.AddMemoryCache(cacheOptions => {
							cacheOptions.ExpirationScanFrequency = TimeSpan.FromSeconds(30);
							cacheOptions.CompactionPercentage = 0.5;
						});
						collection.Configure<Configuration>(context.Configuration);
					})
				   .RunConsoleAsync();
			}
			catch (Exception exception) {
				Console.WriteLine(exception);
				Console.ReadKey();
			}
		}
	}
}