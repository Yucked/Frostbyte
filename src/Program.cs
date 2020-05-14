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
using Rhapsody.Factories;
using Rhapsody.Logging;

namespace Rhapsody {
	public readonly struct Program {
		public static async Task Main() {
			try {
				MiscExtensions.SetupApplicationInformation();

				var options = OptionsFactory.IsCreated
					? OptionsFactory.Load()
					: OptionsFactory.Create();

				await Host.CreateDefaultBuilder()
				   .ConfigureAppConfiguration(x => {
						x.SetBasePath(Directory.GetCurrentDirectory());
						x.AddJsonFile(OptionsFactory.FILE_NAME, false, true);
					})
				   .ConfigureWebHostDefaults(webBuilder => {
						webBuilder.UseStartup<Startup>();
						webBuilder.UseUrls($"http://{options.Host}:{options.Port}");
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
						collection.Configure<ApplicationOptions>(context.Configuration);
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