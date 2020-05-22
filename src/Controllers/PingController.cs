using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Rhapsody.Controllers {
	[Route("api/[controller]"), ApiController, Produces("application/json")]
	public sealed class PingController : ControllerBase {
		private readonly IMemoryCache _memoryCache;
		private const string CACHE_KEY = "_Time";

		public PingController(IMemoryCache memoryCache) {
			_memoryCache = memoryCache;
		}

		[HttpGet]
		public IActionResult Ping() {
			if (_memoryCache.TryGetValue(CACHE_KEY, out DateTimeOffset old)) {
				return Ok(old);
			}

			old = DateTimeOffset.Now;
			_memoryCache.Set(CACHE_KEY, old);
			return Ok(old);
		}
	}
}