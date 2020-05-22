using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Rhapsody.Controllers {
	[Route("api/[controller]"), ApiController, Produces("application/json")]
	public sealed class SearchController : ControllerBase {
		[HttpGet("/youtube")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public Task<ActionResult<object>> GetYouTubeAsync(string query) {
			return SearchAsync(default, default);
		}

		[HttpGet("/soundcloud")]
		public Task GetSoundCloudAsync(string query) {
			return SearchAsync(default, default);
		}

		private async Task<ActionResult<object>> SearchAsync(object providerType, string query) {
			return default;
		}
	}
}