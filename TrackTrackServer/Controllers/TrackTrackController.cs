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
        #region Intiation
        TrackTrackDbContext context;
        DiscogsService discogs;
        Random rnd;

        const int MAXIDVALUE = 1000000;

        public TrackTrackController(TrackTrackDbContext context)
        {
            this.context = context;
            this.discogs = new DiscogsService();
            this.rnd = new Random();
        }
        #endregion

        #region Misc
        [Route("Hello")]
        [HttpGet]
        public async Task<ActionResult> Hello()
        {
            return Ok("hi");
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
        #endregion

        #region Discogs
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
        #endregion

        #region Utilities
        public long GenerateUniqueId(string type)
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
        #endregion


        //im actually not sure all of these work so make sure to play with them and make sure they work

        #region Makenew

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
                    context.SavedAlbums.Add(new SavedAlbum() { AlbumId = albumID, CollectionId = collectionID, UserId = userID, Id = GenerateUniqueId("savedAlbum"), Date = DateTime.Now });
                    await context.SaveChangesAsync();
                    return (Ok("successfully saved " + albumID + " to your collection " + collectionID));
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
                    context.Collections.Add(new Collection { Name = name, OwnerId = userID, Id = id });
                    await context.SaveChangesAsync();
                    return (Ok("successfully added " + name + " to your collections with id = " + id));
                }
            }
            catch (Exception ex) { return BadRequest(ex); };

        }

        #endregion

        #region Updates

        [Route("UpdateUser")]
        [HttpGet]
        public async Task<ActionResult> UpdateUser(long id, string name, string password, string email, string bio)
        {
            try
            {
                var user = context.Users.Where(x => x.Id == id).FirstOrDefault();
                if(user == null) return NotFound("no such user id");
                user.Name = name;
                user.Password = password;
                user.Email = email;
                user.Bio = bio;
                await context.SaveChangesAsync();
                return Ok("successfully updated user " + id);
            }
            catch (Exception ex) { return  BadRequest(ex.Message); }
        }

        [Route("RenameCollection")]
        [HttpGet]
        public async Task<ActionResult> RenameCollection(long id, string name)
        {
            try
            {
                var coll = context.Collections.Where(x => x.Id == id).FirstOrDefault();
                if (coll == null) return NotFound("no such collection id");
                coll.Name = name;
                await context.SaveChangesAsync();
                return Ok("successfully changed collection's name");
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [Route("UpdateRating")]
        [HttpGet]
        public async Task<ActionResult> UpdateRating(long id, long rating)
        {
            try
            {
                var alb = context.SavedAlbums.Where(x => x.Id == id).FirstOrDefault();
                if (alb == null) return NotFound("no such savedAlbum id");
                alb.Rating = rating;
                await context.SaveChangesAsync();
                return Ok("successfully changed "+alb.AlbumId+"'s rating");
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        #endregion

        #region Deletion
        [Route("RemoveUser")]
        [HttpGet]
        public async Task<ActionResult> RemoveUser(long id)
        {
            try
            {
                context.SavedAlbums.RemoveRange(context.SavedAlbums.Where(x => x.UserId == id));
                context.Collections.RemoveRange(context.Collections.Where(x => x.OwnerId == id));

                context.Users.Remove(context.Users.Where(x => x.Id == id).First());
                await context.SaveChangesAsync();
                return Ok("successfully removed user " + id+" and all their related rows");
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [Route("RemoveCollection")]
        [HttpGet]
        public async Task<ActionResult> RemoveCollection(long id)
        {
            try
            {
                context.SavedAlbums.RemoveRange(context.SavedAlbums.Where(x => x.CollectionId == id));

                context.Collections.Remove(context.Collections.Where(x => x.Id == id).First());
                await context.SaveChangesAsync();
                return Ok("successfully removed collection " + id + " and all albums saved in it");
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [Route("RemoveAlbumFromCollection")]
        [HttpGet]
        public async Task<ActionResult> RemoveAlbumFromCollection(long id)
        {
            try
            {
                context.SavedAlbums.Remove(context.SavedAlbums.Where(x => x.Id == id).First());
                await context.SaveChangesAsync();
                return Ok("successfully removed album " + id + " from its collection");
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        #endregion


        //sends all related collections for user id
        [Route("GetUserCollections")]
        [HttpGet]
        public async Task<ActionResult> GetUserCollections(long id) { return null; }



        //sends all related albums saved for user id
        [Route("GetUserSavedAlbums")]
        [HttpGet]
        public async Task<ActionResult> GetUserSavedAlbums(long id) { return null; }
    }
}
