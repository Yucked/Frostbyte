using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dysc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rhapsody.Extensions;
using Rhapsody.Internals.Attributes;
using Rhapsody.Internals.Logging;
using Rhapsody.Objects;

namespace Rhapsody {
	public readonly struct Program {
		public static async Task Main() {
			try {
				MiscExtensions.SetupApplicationInformation();
				var applicationOptions = MiscExtensions.VerifyOptions();

				await Host.CreateDefaultBuilder()
				   .ConfigureAppConfiguration(builder => {
						var sources = builder.Sources.OfType<JsonConfigurationSource>().ToArray();
						foreach (var source in sources) {
							builder.Sources.Remove(source);
						}

						builder.SetBasePath(Directory.GetCurrentDirectory());
						builder.AddJsonFile(ApplicationOptions.FILE_NAME, false, true);
					})
				   .ConfigureWebHostDefaults(webBuilder => {
						webBuilder.UseUrls($"http://{applicationOptions.Url}");
						webBuilder.UseStartup<Startup>();
					})
				   .ConfigureLogging(logging => {
						logging.SetMinimumLevel(applicationOptions.Logging.DefaultLevel);
						logging.ClearProviders();
						logging.AddProvider(new LoggerProvider(applicationOptions.Logging));
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
						collection.Configure<ApplicationOptions>(context.Configuration);

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