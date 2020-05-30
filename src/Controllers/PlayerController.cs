using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rhapsody.Payloads.Inbound;
using Rhapsody.Payloads.Outbound;

namespace Rhapsody.Controllers {
	[Route("api/[controller]"), ApiController, Produces("application/json")]
	public sealed class PlayerController : ControllerBase {
		private readonly IMemoryCache _memoryCache;
		private readonly ILoggerFactory _loggerFactory;

		public PlayerController(IMemoryCache memoryCache, ILoggerFactory loggerFactory) {
			_memoryCache = memoryCache;
			_loggerFactory = loggerFactory;
		}

		[HttpGet("{guildId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public IActionResult Get(ulong guildId) {
			return !_memoryCache.TryGetValue(guildId, out GuildPlayer player)
				? RestResponse.Error($"Couldn't find player with {guildId} id.")
				: RestResponse.Ok(player);
		}

		[HttpDelete("{guildId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public IActionResult Delete(ulong guildId) {
			if (!_memoryCache.TryGetValue(guildId, out _)) {
				return RestResponse.Error($"Couldn't find player with {guildId} id.");
			}

			_memoryCache.Remove(guildId);
			return RestResponse.Ok($"Player with {guildId} id successfully removed.");
		}

		[HttpPost("{guildId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public IActionResult HandlePayload(ulong guildId, object payload) {
			if (payload is ConnectPayload connectPayload) {
				return Ok(connectPayload);
			}

			return BadRequest(RestResponse.Error("Failed"));
		}
	}
}