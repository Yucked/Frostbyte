using System;
using System.IO;
using System.Threading.Tasks;
using Dysc;
using Microsoft.AspNetCore.Builder;
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

				var configuration = Configuration.IsCreated
					? Configuration.Load()
					: Configuration.Create();

				await Host.CreateDefaultBuilder()
				   .ConfigureAppConfiguration(x => {
						x.SetBasePath(Directory.GetCurrentDirectory());
						x.AddJsonFile(Configuration.FILE_NAME, false, true);
					})
				   .ConfigureWebHostDefaults(webBuilder => {
						webBuilder.UseUrls($"http://{configuration.Host}:{configuration.Port}");
						webBuilder.UseStartup<Startup>();
					})
				   .ConfigureLogging(logging => {
						logging.SetMinimumLevel(configuration.LogLevel);
						logging.ClearProviders();
						logging.AddProvider(new LoggerProvider(configuration));
					})
				   .ConfigureServices((context, collection) => {
						collection.AddConnections();
						collection.AddControllers();
						collection.AddHttpClient();
						collection.AddMemoryCache(cacheOptions => {
							cacheOptions.ExpirationScanFrequency = TimeSpan.FromSeconds(30);
							cacheOptions.CompactionPercentage = 0.5;
						});
						collection.AddResponseCaching(cachingOptions => { cachingOptions.SizeLimit = 5; });
						collection.AddResponseCompression();
						collection.Configure<Configuration>(context.Configuration);
						collection.AddSingleton<DyscClient>();
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