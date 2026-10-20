using Microsoft.AspNetCore.Mvc;
using Data;

namespace NotificationSenderApi.Controllers
{
    [ApiController]
    [Route("health")]
    [Produces("application/json")]
    public sealed class HealthController : ControllerBase
    {
        private readonly NotificationDbContext _dbContext;
        public HealthController(NotificationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("live", Name = "Liveness")]
        [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
        public ActionResult<HealthResponse> Live()
        {
            return Ok(new HealthResponse("Healthy"));
        }

        [HttpGet("ready", Name = "Readiness")]
        [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<HealthResponse>> Ready(CancellationToken cancellationToken)
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new HealthResponse("Not Ready"));
            }

            return Ok(new HealthResponse("Ready"));
        }

    }

    public sealed record HealthResponse(string Status);
}
