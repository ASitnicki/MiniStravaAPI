using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniStrava.Models.Requests;
using MiniStrava.Services;

namespace MiniStrava.Controllers
{
    [ApiController]
    [Route("api/activities")]
    [Authorize]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityService _svc;

        public ActivitiesController(IActivityService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<IActionResult> GetMine()
            => Ok(await _svc.GetMineAsync());

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool includeTrackPoints = false)
            => Ok(await _svc.GetMineByIdAsync(id, includeTrackPoints));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateActivityRequest req)
        {
            var created = await _svc.CreateAsync(req);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityRequest req)
            => Ok(await _svc.UpdateAsync(id, req));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _svc.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id:guid}/trackpoints")]
        public async Task<IActionResult> AddTrackPoints(Guid id, [FromBody] AddTrackPointsRequest req)
        {
            var added = await _svc.AddTrackPointsAsync(id, req);
            return Ok(new { success = true, added });
        }
    }
}
