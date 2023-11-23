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
                var id = GenerateUniqueId("user");
                context.Users.Add(new User { Name = name, Password = password, Email = email, Bio = "ararara", Id = id });
                await context.SaveChangesAsync();
                await CreateCollection(id, "favorites");
                return Ok("successfully added " + name + " to the users, id = " + id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        const int MAXIDVALUE = 1000000;
        private long GenerateUniqueId(string type)
        {
            long i = rnd.Next(MAXIDVALUE);
            switch (type)
            {
                case ("user"):
                
                    while (context.Users.Where(u => u.Id == i).FirstOrDefault() != null)
                    {
                        i = rnd.Next(MAXIDVALUE);
                    }
                    break;
                case ("collection"):

                    while (context.Collections.Where(u => u.Id == i).FirstOrDefault() != null)
                    {
                        i = rnd.Next(MAXIDVALUE);
                    }
                    break;
                case ("savedAlbum"):

                    while (context.SavedAlbums.Where(u => u.Id == i).FirstOrDefault() != null)
                    {
                        i = rnd.Next(MAXIDVALUE);
                    }
                    break;
                default: throw (new Exception("no such type"));
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

                return (Ok(res["results"][0]["title"] + " - " + res["results"][0]["id"] + "\n" +
                    res["results"][1]["title"] + " - " + res["results"][1]["id"] + "\n" +
                    res["results"][2]["title"] + " - " + res["results"][2]["id"] + "\n" +
                    res["results"][3]["title"] + " - " + res["results"][3]["id"] + "\n" +
                    res["results"][4]["title"] + " - " + res["results"][4]["id"]));
            }
            catch (Exception ex) { return BadRequest(ex); }

        }

        [Route("SaveAlbum")]
        [HttpGet]
        public async Task<ActionResult> SaveAlbum(long userID, long albumID, long collectionID)
        {
            try
            {
                if (context.SavedAlbums.Where(x => x.UserId == userID && x.AlbumId == albumID && x.CollectionId == collectionID).Any())
                {
                    return Conflict("that album is already saved in that collection");
                }
                else
                {
                    context.SavedAlbums.Add(new SavedAlbum() { AlbumId=albumID, CollectionId=collectionID, UserId=userID, Id=GenerateUniqueId("savedAlbum"), Date=DateTime.Now});
                    await context.SaveChangesAsync();
                    return (Ok("successfully saved "+albumID+" to your collection "+collectionID));
                }
            }
            catch (Exception ex) { return BadRequest(ex); };
        }

        [Route("CreateCollection")]
        [HttpGet]
        public async Task<ActionResult> CreateCollection(long userID, string name)
        {
            try
            {
                if (context.Collections.Where(x => x.Name == name && x.OwnerId == userID).Any())
                {
                    return Conflict("there is already a collection named " + name + " for user " + userID);
                }
                else
                {
                    var id = GenerateUniqueId("collection");
                    context.Collections.Add(new Collection { Name = name, OwnerId = userID, Id=id});
                    await context.SaveChangesAsync();
                    return (Ok("successfully added " + name + " to your collections with id = "+id));
                }
            }
            catch (Exception ex) { return BadRequest(ex); };

        }
    }
}
