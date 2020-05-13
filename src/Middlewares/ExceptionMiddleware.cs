using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Rhapsody.Middlewares {
    public readonly struct ExceptionMiddleware {
        private readonly RequestDelegate _requestDelegate;
        private readonly ILogger _logger;

        public ExceptionMiddleware(RequestDelegate requestDelegate, ILogger<ExceptionMiddleware> logger) {
            _requestDelegate = requestDelegate;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context) {
            try {
                await _requestDelegate(context);
            }
            catch (Exception ex) {
                _logger.LogError(ex, ex.StackTrace);
                context.Response.StatusCode = 500;
            }
        }
    }
}