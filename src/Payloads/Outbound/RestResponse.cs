using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Rhapsody.Payloads.Outbound {
	public sealed class RestResponse : IActionResult {
		public bool IsSuccess { get; }
		public string Message { get; }
		public object Data { get; }

		public RestResponse(bool isSuccess, string message) {
			IsSuccess = isSuccess;
			Message = message;
		}

		public RestResponse(object data) {
			IsSuccess = true;
			Data = data;
		}

		public static IActionResult Error(string message)
			=> new RestResponse(false, message);

		public static IActionResult Ok(object data)
			=> new RestResponse(data);

		public static IActionResult Ok(string message)
			=> new RestResponse(true, message);

		public async Task ExecuteResultAsync(ActionContext context) {
			var jsonResult = new JsonResult(this) {
				ContentType = "application/json",
				StatusCode = IsSuccess ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest,
				Value = this
			};

			await jsonResult.ExecuteResultAsync(context);
		}
	}
}