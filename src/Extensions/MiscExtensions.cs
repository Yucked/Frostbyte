using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Colorful;
using Dysc.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Rhapsody.Objects;
using Console = Colorful.Console;

namespace Rhapsody.Extensions {
	public static class MiscExtensions {
		public static void SetupApplicationInformation() {
			var versionAttribute = typeof(Startup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

			Console.SetWindowSize(120, 25);
			Console.Title = $"{nameof(Rhapsody)} v{versionAttribute?.InformationalVersion}";

			const string TITLE =
				@"       _         _           _       _         _        _         _            _  
     _/\\___   _/\\___    __/\\__  _/\\___    /\\__  __/\\___  __/\\___   _   /\\ 
    (_   _  ))(_ __ __)) (_  ____)(_   _ _)) /    \\(_     _))(_  ____)) /\\ / // 
     /  |))//  /  |_| \\  /  _ \\  /  |))\\ _\  \_// /  _  \\  /   _ \\  \ \/ //  
    /:.    \\ /:.  _   \\/:./_\ \\/:. ___//// \:.\  /:.(_)) \\/:. |_\ \\ _\:.//   
    \___|  // \___| |  //\  _   //\_ \\    \\__  /  \  _____//\  _____//(_  _))   
         \//         \//  \// \//   \//       \\/    \//       \//        \//     
";
			Console.WriteWithGradient(TITLE, Color.DarkOrange, Color.SlateBlue);
			Console.ReplaceAllColorsWithDefaults();

			const string LOG_MESSAGE =
				"->  Version: {0}\n->  Framework: {1}\n->  OS Arch: {2}\n->  Process Arch: {3}\n->  OS: {4}";

			var formatters = new[] {
				new Formatter(versionAttribute?.InformationalVersion.Trim(), Color.Gold),
				new Formatter(RuntimeInformation.FrameworkDescription, Color.Aqua),
				new Formatter(RuntimeInformation.OSArchitecture, Color.Gold),
				new Formatter(RuntimeInformation.ProcessArchitecture, Color.LawnGreen),
				new Formatter(RuntimeInformation.OSDescription, Color.HotPink)
			};

			Console.WriteLineFormatted(LOG_MESSAGE, Color.White, formatters);
			Console.WriteLine(new string('-', 100), Color.Gray);
		}

		public static Task SendAsync<T>(this WebSocket webSocket, T data) {
			return webSocket.SendAsync(data.Serialize(), WebSocketMessageType.Binary, true, CancellationToken.None);
		}

		public static bool IsValidRoute(this HttpContext httpContext, out ulong guildId) {
			if (!httpContext.Request.RouteValues.TryGetValue("guildId", out var obj)) {
				httpContext.Response.StatusCode = 403;
				guildId = default;
				return false;
			}

			if (ulong.TryParse($"{obj}", out guildId)) {
				return true;
			}

			httpContext.Response.StatusCode = 403;
			return false;
		}

		public static bool TryMatchAny<T>(this T value, params T[] against) where T : struct {
			return against.Contains(value);
		}

		public static ApplicationOptions VerifyOptions() {
			ApplicationOptions applicationOptions;
			if (File.Exists(ApplicationOptions.FILE_NAME)) {
				var bytes = File.ReadAllBytes(ApplicationOptions.FILE_NAME);
				applicationOptions = bytes.Deserialize<ApplicationOptions>();
			}
			else {
				applicationOptions = new ApplicationOptions {
					Endpoint = new EndpointOptions {
						Host = "*",
						Port = 2020,
						FallbackRandom = false
					},
					Authentication = new AuthenticationOptions {
						Password = nameof(Rhapsody),
						Endpoints = new List<string> {
							"/api/search",
							"/ws"
						}
					},
					Logging = new LoggingOptions {
						Filters = new Dictionary<string, LogLevel> {
							{
								"System.*", LogLevel.Warning
							}
						},
						DefaultLevel = LogLevel.Trace
					},
					Providers = Enum.GetNames(typeof(ProviderType))
					   .ToDictionary(x => x, x => true)
				};
				var serialize = applicationOptions.Serialize();
				File.WriteAllBytes(ApplicationOptions.FILE_NAME, serialize);
			}

			return applicationOptions;
		}
	}
}