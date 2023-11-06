using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrackTrackServerBL.Models;

namespace TrackTrackServer.Controllers
{
    [Route("TrackTrack")]
    [ApiController]
    public class TrackTrackController : ControllerBase
    {
        TrackTrackDbContext context;
        public TrackTrackController(TrackTrackDbContext context)
        {
            this.context = context;
        }

        [Route("Hello")]
        [HttpGet]
        public async Task<ActionResult> Hello()
        {
            return Ok("hi");
        }
    }
}
