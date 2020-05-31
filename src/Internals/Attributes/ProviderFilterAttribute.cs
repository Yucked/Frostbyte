using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Rhapsody.Objects;
using Rhapsody.Payloads.Outbound;

namespace Rhapsody.Internals.Attributes {
	public sealed class ProviderFilterAttribute : ActionFilterAttribute {
		private readonly IDictionary<string, bool> _providers;

		public ProviderFilterAttribute(IConfiguration configuration) {
			_providers = configuration.Get<ApplicationOptions>().Providers;
		}

		public override void OnActionExecuting(ActionExecutingContext context) {
			if (context.Controller.GetType() != typeof(Controllers.SearchController)) {
				return;
			}

			var requestPath = context.HttpContext.Request.Path;
			if (!requestPath.HasValue) {
				context.Result = RestResponse.Error("Request path value wasn't specified.");
				return;
			}

			var provider = requestPath.Value[12..];
			if (!_providers.TryGetValue(provider, out var isEnabled)) {
				context.Result = RestResponse.Error("Invalid provider. Is the provider added in configuration?");
				return;
			}

			if (isEnabled) {
				return;
			}

			context.Result = RestResponse.Error($"{provider} is disabled in configuration.");
		}
	}
}