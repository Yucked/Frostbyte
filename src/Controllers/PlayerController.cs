using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Rhapsody.Controllers.Entities;

namespace Rhapsody.Controllers {
	[Route("api/[controller]"), ApiController]
	public sealed class PlayerController : ControllerBase {
		private readonly IMemoryCache _memoryCache;

		public PlayerController(IMemoryCache memoryCache) {
			_memoryCache = memoryCache;
		}

		[HttpGet("{guildId}")]
		public IActionResult Get(ulong guildId) {
			if (!_memoryCache.TryGetValue(guildId, out PlayerEntity player)) {
				return NotFound();
			}

			return Ok(player);
		}

		[HttpPost("{guildId}")]
		public IActionResult Create(ulong guildId, PlayerEntity player) {
			if (_memoryCache.TryGetValue(guildId, out _)) {
				return Conflict();
			}

			_memoryCache.Set(guildId, player);
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
		public IActionResult HandlePayload(ulong guildId, PayloadEntity payload) {
			if (!_memoryCache.TryGetValue(guildId, out _)) {
				return NotFound();
			}

			return Ok();
		}
	}
}