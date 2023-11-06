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

        [Route("AddUser")]
        [HttpGet]
        public async Task<ActionResult> AddUser(string name, string password, string email)
        {
            try
            {
                context.Users.Add(new User { Name = name, Password = password, Email = email, Bio = "ararara", Id = 1 });
                context.SaveChangesAsync();
                return Ok("successfully added " + name + "to the users");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //this doesnt work
        [Route("GetUsers")]
        [HttpGet]
        public async Task<ActionResult<User>> GetUsers()
        {
            try
            {
                return Ok(context.Users.Where(u => u.Id == 1).FirstOrDefault());
            }
            catch (Exception ex) { return BadRequest(ex); }
        }

    }
}
