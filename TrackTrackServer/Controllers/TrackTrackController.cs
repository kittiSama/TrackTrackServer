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
        Random rnd;
        public TrackTrackController(TrackTrackDbContext context)
        {
            this.context = context;
            this.rnd = new Random();
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
                context.Users.Add(new User { Name = name, Password = password, Email = email, Bio = "ararara", Id = GenerateUniqueId() });
                await context.SaveChangesAsync();
                return Ok("successfully added " + name + "to the users");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        //this doesnt work
        private long GenerateUniqueId()
        {
            long i = (long)rnd.Next(10000);
            while(context.Users.Where(u => u.Id == i).First() != null)
            {
                i = rnd.Next(10000);
            }
            return i;

        }

        [Route("GetUsers")]
        [HttpGet]
        public async Task<ActionResult<User>> GetUsers(string param, string value)
        {
            try
            {
                switch (param)
                {
                    case ("id"):
                        return Ok(context.Users.Where(u => u.Id.ToString() == value).FirstOrDefault());
                    case ("name"):
                        return Ok(context.Users.Where(u => u.Name.ToString() == value).FirstOrDefault());
                    case ("password"):
                        return Ok(context.Users.Where(u => u.Password.ToString() == value).FirstOrDefault());
                    case ("email"):
                        return Ok(context.Users.Where(u => u.Email.ToString() == value).FirstOrDefault());
                    default:
                        return BadRequest("No such user parameter");
                }
            }
            catch (Exception ex) { return BadRequest(ex); }
        }

    }
}
