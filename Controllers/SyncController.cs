using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniStrava.Models.Requests;
using MiniStrava.Services;

namespace MiniStrava.Controllers
{
    [ApiController]
    [Route("api/sync")]
    [Authorize]
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _svc;

        public SyncController(ISyncService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<IActionResult> GetMine()
            => Ok(await _svc.GetMineAsync());

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartSyncSessionRequest req)
            => Ok(await _svc.StartAsync(req));

        [HttpPost("{sessionId:guid}/finish")]
        public async Task<IActionResult> Finish(Guid sessionId, [FromBody] FinishSyncSessionRequest req)
            => Ok(await _svc.FinishAsync(sessionId, req));
    }
}
