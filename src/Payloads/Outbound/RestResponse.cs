using Microsoft.AspNetCore.Mvc;

namespace Rhapsody.Payloads.Outbound {
	public sealed class RestResponse {
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

		public static ActionResult<RestResponse> Error(string message)
			=> new BadRequestObjectResult(new RestResponse(false, message));

		public static ActionResult<RestResponse> Ok(object data)
			=> new OkObjectResult(new RestResponse(data));

		public static ActionResult<RestResponse> Ok(string message)
			=> new OkObjectResult(new RestResponse(true, message));
	}
}