using Microsoft.AspNetCore.Mvc;

namespace NotificationSenderApi.Controllers
{
    [ApiController]
    [Route("health")]
    [Produces("application/json")]
    public sealed class HealthController : ControllerBase
    {
        [HttpGet("live", Name = "Liveness")]
        [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
        public ActionResult<HealthResponse> Live()
        {
            return Ok(new HealthResponse("Healthy"));
        }
    }

    public sealed record HealthResponse(string Status);
}
