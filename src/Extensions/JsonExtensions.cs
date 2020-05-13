using System.Text.Json;

namespace Rhapsody.Extensions {
	public static class JsonExtensions {
		public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		public static T Deserialize<T>(this byte[] bytes) {
			return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
		}

		public static byte[] Serialize<T>(this T value) {
			return JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
		}
	}
}