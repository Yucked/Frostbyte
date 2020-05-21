using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rhapsody.Payloads;
using Rhapsody.Payloads.Inbound;

namespace Rhapsody.Controllers {
	[Route("api/[controller]"), ApiController]
	public sealed class PlayerController : ControllerBase {
		private readonly IMemoryCache _memoryCache;
		private readonly ILoggerFactory _loggerFactory;

		public PlayerController(IMemoryCache memoryCache, ILoggerFactory loggerFactory) {
			_memoryCache = memoryCache;
			_loggerFactory = loggerFactory;
		}

		[HttpGet("{guildId}")]
		public IActionResult Get(ulong guildId) {
			if (!_memoryCache.TryGetValue(guildId, out GuildPlayer player)) {
				return NotFound();
			}

			return Ok(player);
		}

		[HttpDelete("{guildId}")]
		public IActionResult Delete(ulong guildId) {
			if (!_memoryCache.TryGetValue(guildId, out _)) {
				return NotFound();
			}

			_memoryCache.Remove(guildId);
			return Ok();
		}

		[HttpPost("{guildId}")]
		public IActionResult HandlePayload(ulong guildId, BasePayload payload) {
			if (!ModelState.IsValid) {
				return BadRequest(ModelState);
			}

			if (payload.Op != PayloadOp.Connect && !_memoryCache.TryGetValue(guildId, out _)) {
				return NotFound();
			}

			switch (payload.Op) {
				case PayloadOp.Connect:
					var logger = _loggerFactory.CreateLogger<GuildPlayer>();
					var guildPlayer = new GuildPlayer(payload as ConnectPayload, "", logger);

					_memoryCache.Set(guildId, guildPlayer);
					break;
			}

			return Ok();
		}
	}
}