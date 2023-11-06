using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrackTrackServerBL.Models;

namespace TrackTrackServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackTrackController : ControllerBase
    {
        TrackTrackDbContext context;
        public TrackTrackController(TrackTrackDbContext context)
        {
            this.context = context;
        }
    }
}
