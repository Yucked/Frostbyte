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
using Rhapsody.Extensions;
using Rhapsody.Internals.Attributes;
using Rhapsody.Internals.Logging;
using Rhapsody.Options;

namespace Rhapsody {
	public readonly struct Program {
		public static async Task Main() {
			try {
				MiscExtensions.SetupApplicationInformation();

				var configuration = OptionsManager.IsCreated
					? OptionsManager.Load()
					: OptionsManager.Create();

				await Host.CreateDefaultBuilder()
				   .ConfigureAppConfiguration(x => {
						x.SetBasePath(Directory.GetCurrentDirectory());
						x.AddJsonFile(OptionsManager.FILE_NAME, false, true);
					})
				   .ConfigureWebHostDefaults(webBuilder => {
						var endpoint = configuration.Endpoint;
						webBuilder.UseUrls($"http://{endpoint.Host}:{endpoint.Port}");
						webBuilder.UseStartup<Startup>();
					})
				   .ConfigureLogging(logging => {
						var loggingOptions = configuration.Logging;
						logging.SetMinimumLevel(loggingOptions.DefaultLevel);
						logging.ClearProviders();
						logging.AddProvider(new LoggerProvider(loggingOptions));
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
						collection.Configure<OptionsManager>(context.Configuration);
						
						collection.AddScoped<ProviderFilterAttribute>();
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