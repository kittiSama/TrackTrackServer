using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TrackTrackServer.Services;
using TrackTrackServerBL.Models;
using TrackTrackServer.Utilities;

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

        public TrackTrackController(TrackTrackDbContext context)
        {
            this.context = context;
            this.discogs = new DiscogsService();
            this.rnd = new Random();
        }
        #endregion

        #region Misc
        [Route("Hello")]
        [HttpGet] //returns hi
        public async Task<ActionResult> Hello()
        {
            return Ok("hi");
        }

        [Route("Login")]
        [HttpPost]
        public async Task<ActionResult<User>> Login(User user)
        {
            try
            {
                User found = context.Users.Where(u => u.Name == user.Name && u.Password == user.Password).FirstOrDefault();
                if (found != null)
                {
                    HttpContext.Session.SetObject("user", user);
                    return (Ok(found)); 
                }
                else { return (NotFound()); }
            }
            catch { return BadRequest(); }

        }

        #endregion

        #region Discogs
        [Route("GetClosestAlbums")]
        [HttpGet] //gets the top 5 results when searching q, returns all of their information
        public async Task<ActionResult> GetClosestAlbums(string q)
        {
            try
            {

                return (Ok(await discogs.GetClosestAlbums(q)));
            }
            catch (Exception ex) { return BadRequest(ex); }

        }

        [Route("GetClosestAlbumsShort")]
        [HttpGet] //gets the top 5 results when searching q, returns just their title and id
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

        [Route("GetClosestAlbumsForApp")]
        [HttpGet] //gets the top 5 results when searching q, returns just their title and id
        public async Task<ActionResult<Album[]>> GetClosestAlbumsForApp(string q)
        {
            try
            {
                var res = JObject.Parse(await discogs.GetClosestAlbums(q));
                var output = new Album[5];

                for (int i = 0; i < 5; i++)
                {
                    var titleandartist = res["results"][i]["title"].ToString();
                    var TAA = titleandartist.Split('-');
                    output[i] = new Album()
                    {
                        AlbumTitle = TAA[1].Trim(),
                        AlbumID = (long)res["results"][i]["id"],
                        ImageUrl = res["results"][i]["thumb"].ToString(),
                        ArtistName = TAA[0].Trim()
                    };
                }
                return (Ok(output));
            }
            catch (Exception ex) { return BadRequest(ex); }

        }
        #endregion

        #region Getters
        [Route("GetUsers")]
        [HttpGet] //get user object from any given of their given info (should be unique to that user)
        public async Task<ActionResult<User>> GetUsers(string param, string value)
        {
            User toReturn;
            try
            {
                switch (param)
                {
                    case ("id"):
                        toReturn = (context.Users.Where(u => u.Id.ToString() == value).FirstOrDefault());
                        break;
                    case ("name"):
                        toReturn = (context.Users.Where(u => u.Name == value).FirstOrDefault());
                        break;
                    case ("password"):
                        toReturn = (context.Users.Where(u => u.Password == value).FirstOrDefault());
                        break;
                    case ("email"):
                        toReturn = (context.Users.Where(u => u.Email == value).FirstOrDefault());
                        break;
                    default:
                        return BadRequest("No such user parameter");
                }
                if (toReturn == null) return NotFound("no user matching param: " + param + " and value: " + value);
                return (toReturn);
            }
            catch (Exception ex) { return BadRequest(ex); }
        }

        [Route("GetUserCollections")]
        [HttpGet] //gets all the user's collections
        public async Task<ActionResult> GetUserCollections(long id)
        {
            try
            {
                var result = context.Collections.Where(x => x.OwnerId == id);
                if (result == null) return NotFound("user " + id + " either has no collections, or doesn't exist");
                return (Ok(result));
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [Route("GetUserSavedAlbums")]
        [HttpGet] //gets all the albums in all the user's collections
        public async Task<ActionResult> GetUserSavedAlbums(long id)
        {
            try
            {
                var result = context.SavedAlbums.Where(x => x.UserId == id);
                if (result == null) return NotFound("user " + id + " either has no saved albums, or doesn't exist");
                return (Ok(result));
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [Route("GetAlbumsInCollection")]
        [HttpGet] //gets all albums in a collection
        public async Task<ActionResult> GetAlbumsInCollection(long id)
        {
            try
            {
                var result = context.SavedAlbums.Where(x => x.CollectionId == id);
                if (result == null) return NotFound("collection " + id + " either has no albums savd in it, or doesn't exist");
                return (Ok(result));
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        #endregion

        #region Create
        [Route("AddUser")]
        [HttpPost] //adds user with the required params, an empty bio and a random unique id. also creates a collection named favorites for them
        public async Task<ActionResult> AddUser(User user)
        {
            try
            {
                Utils.ValidateUser(user);
                var id = Utils.GenerateUniqueId("user", rnd, context);
                user.Id = id;
               
                context.Users.Add(user);
                await context.SaveChangesAsync();
                await CreateCollection(new Collection() { Name="favorites", OwnerId=id});
                HttpContext.Session.SetObject("user", user); 
                return Ok("successfully added " + user.Name + " to the users, id = " + id);
            }
            catch (BadDataException ex)
            {
                return(Problem(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Route("SaveAlbum")]
        [HttpPost] //saves an album in a specified user's collection
        public async Task<ActionResult> SaveAlbum(SavedAlbum save)
        {
            //Maya <3 Ohad
            try
            {
                if (context.SavedAlbums.Where(x => x.UserId == save.UserId && x.AlbumId == save.AlbumId && x.CollectionId == save.CollectionId).Any())
                {
                    return Conflict("that album is already saved in that collection");
                }
                else
                {
                    save.Date = DateTime.Now;
                    save.Id = Utils.GenerateUniqueId("savedAlbum", rnd, context);
                    if(save.Rating==null) save.Rating = 0;
                    context.SavedAlbums.Add(save);
                    await context.SaveChangesAsync();
                    return (Ok("successfully saved " + save.AlbumId + " to your collection " + save.CollectionId));
                }
            }
            catch (Exception ex) { return BadRequest(ex); };
        }

        [Route("SaveAlbumByName")]
        [HttpPost] //saves an album in a specified user's collection
        public async Task<ActionResult> SaveAlbumByName(SavedAlbum save, string collectionName)//make dto for this shit
        {
            try
            {
                var collectionID = context.Collections.Where(x => x.OwnerId == save.UserId && x.Name == collectionName).First().Id;
                save.CollectionId = collectionID;
                if (context.SavedAlbums.Where(x => x.UserId == save.UserId && x.AlbumId == save.AlbumId && x.CollectionId == save.CollectionId).Any())
                {
                    return Conflict("that album is already saved in that collection");
                }
                else
                {
                    save.Date = DateTime.Now;
                    save.Id = Utils.GenerateUniqueId("savedAlbum", rnd, context);
                    if (save.Rating == null) save.Rating = 0;
                    context.SavedAlbums.Add(save);
                    await context.SaveChangesAsync();
                    return (Ok("successfully saved " + save.AlbumId + " to your collection " + save.CollectionId));
                }
            }
            catch (Exception ex) { return BadRequest(ex); };
        }

        [Route("CreateCollection")]
        [HttpPost] // creates a new collection for a user
        public async Task<ActionResult> CreateCollection(Collection coll)
        {
            try
            {
                
                if (context.Collections.Where(x => x.Name == coll.Name && x.OwnerId == coll.OwnerId).Any())
                {
                    return Conflict("there is already a collection named " + coll.Name + " for user " + coll.OwnerId);
                }
                else
                {
                    var id = Utils.GenerateUniqueId("collection", rnd, context);
                    coll.Id = id;
                    context.Collections.Add(coll);
                    await context.SaveChangesAsync();
                    return (Ok("successfully added " + coll.Name + " to your collections with id = " + id));
                }
            }
            catch (Exception ex) { return BadRequest(ex); };

        }

        #endregion

        #region Updates
        [Route("UpdateUser")]
        [HttpPost] //updates a user based on their id (it remains constant), gets all their new information and saves it
        public async Task<ActionResult> UpdateUser(long id, string name, string password, string email, string bio)
        {
            try
            {
                var user = context.Users.Where(x => x.Id == id).FirstOrDefault();
                if(user == null) return NotFound("no such user id");
                Utils.ValidateUser(user);
                user.Name = name;
                user.Password = password;
                user.Email = email;
                user.Bio = bio;
                await context.SaveChangesAsync();
                return Ok("successfully updated user " + id);
            }
            catch (BadDataException ex)
            {
                return (Problem(ex.Message));
            }
            catch (Exception ex) { return  BadRequest(ex.Message); }
        }

        [Route("RenameCollection")]
        [HttpPost] //changes a collection's name
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
        [HttpPost] //changes the rating given to an album in a collection
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
        [HttpDelete] //removes a user, all their collections, and all their saved albums
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
        [HttpDelete] //removes a collection and all albums saved in it
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
        [HttpDelete] //removes a specific album from a specific collection
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

    }
}
