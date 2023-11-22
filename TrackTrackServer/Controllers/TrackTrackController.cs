using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TrackTrackServer.Services;
using TrackTrackServerBL.Models;

namespace TrackTrackServer.Controllers
{
    [Route("TrackTrack")]
    [ApiController]
    public class TrackTrackController : ControllerBase
    {
        TrackTrackDbContext context;
        DiscogsService discogs;
        Random rnd;
        public TrackTrackController(TrackTrackDbContext context)
        {
            this.context = context;
            this.discogs = new DiscogsService();
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
                var id = GenerateUniqueId();
                context.Users.Add(new User { Name = name, Password = password, Email = email, Bio = "ararara", Id = id });
                await context.SaveChangesAsync();
                return Ok("successfully added " + name + " to the users, id = "+id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        //this doesnt work
        private long GenerateUniqueId()
        {
            long i = rnd.Next(1000000);
            while(context.Users.Where(u => u.Id == i).FirstOrDefault() != null)
            {
                i = rnd.Next(1000000);
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
                        return Ok(context.Users.Where(u => u.Name == value).FirstOrDefault());
                    case ("password"):
                        return Ok(context.Users.Where(u => u.Password == value).FirstOrDefault());
                    case ("email"):
                        return Ok(context.Users.Where(u => u.Email == value).FirstOrDefault());
                    default:
                        return BadRequest("No such user parameter");
                }
            }
            catch (Exception ex) { return BadRequest(ex); }
        }
        
        [Route("GetClosestAlbums")]
        [HttpGet]
        public async Task<ActionResult> GetClosestAlbums(string q)
        {
            try
            {
                
                return (Ok(await discogs.GetClosestAlbums(q)));
            }
            catch (Exception ex) { return BadRequest(ex); }

        }

        [Route("GetClosestAlbumsShort")]
        [HttpGet]
        public async Task<ActionResult> GetClosestAlbumsShort(string q)
        {
            try
            {
                var res = JObject.Parse(await discogs.GetClosestAlbums(q));
                
                return (Ok(res["results"][0]["title"] + " - " + res["results"][0]["id"] +  "\n" +
                    res["results"][1]["title"] + " - " + res["results"][1]["id"] +         "\n" +
                    res["results"][2]["title"] + " - " + res["results"][2]["id"] +         "\n" +
                    res["results"][3]["title"] + " - " + res["results"][3]["id"] +         "\n" +
                    res["results"][4]["title"] + " - " + res["results"][4]["id"]));
            }
            catch (Exception ex) { return BadRequest(ex); }

        }

        [Route("SaveAlbum")]
        [HttpGet]
        public async Task<ActionResult> SaveAlbum(int userID, int albumID, int collectionID)
        {
            return null;
        }


    }
}
