using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Rhapsody.Controllers {
	[Route("api/[controller]"), ApiController]
	public sealed class SearchController : ControllerBase {
		[HttpGet("/youtube")]
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