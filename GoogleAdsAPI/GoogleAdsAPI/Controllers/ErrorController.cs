using Digital.Domain.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoogleAdsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)] // Ask to Hasan.
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("/error")]
        public IActionResult Error()
        {
            Exception? ex = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
            _logger.LogError($"ERROR: {ex?.ToFullBlownString()}");

            return Problem(title: ex?.Message);
        }
    }
}
