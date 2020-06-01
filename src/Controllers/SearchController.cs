using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dysc;
using Dysc.Providers;
using Dysc.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rhapsody.Extensions;
using Rhapsody.Internals.Attributes;
using Rhapsody.Payloads.Outbound;

namespace Rhapsody.Controllers {
	[Route("api/[controller]"), ApiController, Produces("application/json")]
	[ServiceFilter(typeof(ProviderFilterAttribute))]
	public sealed class SearchController : ControllerBase {
		private readonly DyscClient _dyscClient;
		private readonly IMemoryCache _memoryCache;
		private readonly ILogger _logger;

		public SearchController(DyscClient dyscClient, ILogger<SearchController> logger, IMemoryCache memoryCache) {
			_dyscClient = dyscClient;
			_logger = logger;
			_memoryCache = memoryCache;
		}


		[HttpGet("youtube")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ValueTask<IActionResult> GetYouTubeAsync(string query) {
			return SearchAsync(ProviderType.YouTube, query);
		}

		[HttpGet("soundcloud")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ValueTask<IActionResult> GetSoundCloudAsync(string query) {
			return SearchAsync(ProviderType.SoundCloud, query);
		}

		[HttpGet("bandcamp")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ValueTask<IActionResult> GetBandCampAsync(string query) {
			return SearchAsync(ProviderType.BandCamp, query);
		}

		[HttpGet("hearthisat")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ValueTask<IActionResult> GetHearThisAtAsync(string query) {
			return SearchAsync(ProviderType.HearThisAt, query);
		}

		[HttpGet("http")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ValueTask<IActionResult> GetHttpAsync(string query) {
			return !Uri.IsWellFormedUriString(query, UriKind.Absolute)
				? new ValueTask<IActionResult>(RestResponse.Error("Query must be an absolute URI string."))
				: SearchAsync(ProviderType.Http, query);
		}

		private async ValueTask<IActionResult> SearchAsync(ProviderType providerType, string query) {
			if (string.IsNullOrWhiteSpace(query)) {
				return RestResponse.Error("Query must not be empty.");
			}

			try {
				var provider = _dyscClient.GetProvider(providerType);
				if (TrySearchCache(providerType, query, out var searchResponse)) {
				}
				else {
					searchResponse = await provider.SearchAsync(query);
					_memoryCache.Set(providerType, new[] {
						searchResponse
					});
				}

				return RestResponse.Ok(searchResponse);
			}
			catch (Exception exception) {
				_logger.LogCritical(exception, exception.StackTrace);
				return RestResponse.Error(exception.Message);
			}
		}

		private bool TrySearchCache(ProviderType providerType, string query, out SearchResponse searchResponse) {
			searchResponse = default;
			if (!_memoryCache.TryGetValue(providerType, out ICollection<SearchResponse> searchResponses)) {
				return false;
			}

			foreach (var response in searchResponses) {
				if (response.Query.IsFuzzyMatch(query)) {
					searchResponse = response;
					return true;
				}

				var sum = response.Tracks.Sum(info => {
					if (info.Url.IsFuzzyMatch(query)) {
						return 1;
					}

					if (info.Title.IsFuzzyMatch(query)) {
						return 1;
					}

					return $"{info.Author} {info.Title}".IsFuzzyMatch(query) ? 1 : 0;
				});

				if (sum / response.Tracks.Count <= 85) {
					continue;
				}

				searchResponse = response;
				return true;
			}
			
			return false;
		}
	}
}