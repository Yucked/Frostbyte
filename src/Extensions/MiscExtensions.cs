using System.Drawing;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Colorful;
using Microsoft.AspNetCore.Http;
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

		public static bool IsValidRequest(this HttpContext context, out ulong userId) {
			if (!context.Request.Headers.TryGetValue("User-Id", out var id)) {
				context.Response.StatusCode = 403;
				userId = default;
				return false;
			}

			if (ulong.TryParse(id, out userId)) {
				return true;
			}

			context.Response.StatusCode = 403;
			return false;
		}
	}
}