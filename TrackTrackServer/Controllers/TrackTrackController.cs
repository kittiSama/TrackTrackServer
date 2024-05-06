using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TrackTrackServer.Services;
using TrackTrackServerBL.Models;
using TrackTrackServer.AdditionalModels;
using TrackTrackServer.Utilities;
using TrackTrackServer.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
                    HttpContext.Session.SetObject("user", found);
                    return (Ok(found)); 
                }
                else { return (NotFound()); }
            }
            catch { return BadRequest(); }

        }

        #endregion

        #region Discogs

        [Route("GetClosestAlbumsForApp")]
        [HttpGet] //gets the top 5 results when searching q, returns just their title and id
        public async Task<ActionResult<AlbumAndHeart[]>> GetClosestAlbumsForApp(string q, string SType)
        {
            try
            {
                var res = JObject.Parse(await discogs.GetClosestAlbums(q,SType));
                var output = new AlbumAndHeart[5];
                User user = HttpContext.Session.GetObject<User>("user");
                var usersfavscollection = context.Collections.Where(y => y.OwnerId == user.Id && y.Name == "favorites").First();
                List<SavedAlbum> usersfavs = context.SavedAlbums.Where(x => x.UserId == user.Id && x.CollectionId == usersfavscollection.Id).ToList();
                bool copy = false;
                int bonus = 0;
                for (int i = 0; i < 5; i++)
                {
                    var titleandartist = res["results"][i+bonus]["title"].ToString();
                    var TAA = titleandartist.Split('-');
                    if (TAA[1] == null) TAA[1] = "null";
                    copy = false; 
                    foreach(var prev in output)
                    {
                        if (prev != null)
                        {
                            if (prev.album.AlbumTitle == TAA[1].Trim() && prev.album.ArtistName == TAA[0].Trim())
                            {
                                if (bonus + i < 45)
                                {
                                    copy = true;
                                    i--;
                                    bonus++;
                                    break;

                                }
                            }
                        }
                    }
                    if (!copy)
                    {
                        output[i] = new AlbumAndHeart();
                        var currResult = res["results"][i + bonus];

                        output[i].album = new Album()
                        {
                            AlbumTitle = TAA[1].Trim(),
                            AlbumID = (long)currResult["id"],
                            ImageUrl = currResult["thumb"].ToString(),
                            ArtistName = TAA[0].Trim(),
                        };
                        if (currResult["year"]==null) output[i].album.Year = "0";
                        else output[i].album.Year = currResult["year"].ToString();
                        if (currResult["genre"].IsNullOrEmpty()) output[i].album.Genre = "No Genre";
                        else output[i].album.Genre = currResult["genre"][0].ToString();
                        if (currResult["style"].IsNullOrEmpty()) output[i].album.Style = "No Style";
                        else output[i].album.Style = currResult["style"][0].ToString();

                        if (usersfavs.Where(x => x.AlbumId == (long)res["results"][i + bonus]["id"]).Any())
                        {
                            output[i].image = "heart_icon_happy.png";
                        }
                        else
                        {
                            output[i].image = "heart_icon.png";
                        }
                    }
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
        public async Task<ActionResult<List<SavedAlbum>>> GetAlbumsInCollection(long id)
        {
            try
            {
                var result = context.SavedAlbums.Where(x => x.CollectionId == id).ToList();
                if (result == null) return NotFound("collection " + id + " either has no albums savd in it, or doesn't exist");
                return (Ok(result));
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [Route("GetAlbumsInCollectionByName")]
        [HttpGet] //gets all albums in a collection
        public async Task<ActionResult<List<SavedAlbum>>> GetAlbumsInCollectionByName(long userId, string collectionName)
        {
            try
            {
                var collection = context.Collections.Where(x => x.OwnerId == userId && x.Name == collectionName).ToList();
                if (collection == null) return NotFound("collection " + collectionName + " either has no albums savd in it, or doesn't exist");
                return (Ok(collection));
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
        public async Task<ActionResult> SaveAlbumByName([FromBody] SaveAlbumByNameDTO dto)//make dto for this shit
        {
            try
            {
                var collection = context.Collections.Where(x => x.OwnerId == HttpContext.Session.GetObject<User>("user").Id && x.Name == dto.collectionName).First();
                dto.savedAlbum.Collection = collection;
                var found = context.SavedAlbums.Where(x => x.UserId == dto.savedAlbum.User.Id && x.AlbumId == dto.savedAlbum.AlbumId && x.CollectionId == dto.savedAlbum.Collection.Id).FirstOrDefault();
                if (found!=null)
                {
                    return Conflict("exists");
                }
                else
                {
                    if (!context.AlbumData.Where(x => x.Id == dto.savedAlbum.AlbumId).Any())
                    {


                        string albumData = await discogs.GetAlbumInfo(dto.savedAlbum.AlbumId);
                        var dataJson = JObject.Parse(albumData);
                        AlbumDatum albumDatum = new AlbumDatum()
                        {
                            Id = (long)dataJson["id"],
                            ArtistId = (long)dataJson["artists"][0]["id"],
                            ArtistName = dataJson["artists"][0]["name"].ToString(),
                            Country = dataJson["country"].ToString(),
                            Year = (long)dataJson["year"],
                        };
                        dto.savedAlbum.Album = albumDatum;
                        context.AlbumData.Add(albumDatum);

                        await context.SaveChangesAsync();

                        foreach (string genre in dataJson["genres"].ToList())
                        {
                            context.AlbumGenres.Add(new()
                            {
                                Id = Utils.GenerateUniqueId("AlbumGenre", rnd, context),
                                AlbumId = dto.savedAlbum.AlbumId,
                                Genre = genre
                            });
                        }
                        await context.SaveChangesAsync();

                        foreach (string style in dataJson["styles"].ToList())
                        {
                            context.AlbumStyles.Add(new()
                            {
                                Id = Utils.GenerateUniqueId("AlbumStyle", rnd, context),
                                AlbumId = dto.savedAlbum.AlbumId,
                                Style = style
                            });
                        }

                    }

                    dto.savedAlbum.Date = DateTime.Now;
                    dto.savedAlbum.User = HttpContext.Session.GetObject<User>("user");
                    dto.savedAlbum.UserId = dto.savedAlbum.User.Id;
                    dto.savedAlbum.Id = Utils.GenerateUniqueId("savedAlbum", rnd, context);
                    if (dto.savedAlbum.Rating == null) dto.savedAlbum.Rating = 0;
                    context.Users.Attach(dto.savedAlbum.User);


                    dto.savedAlbum.Album.Id = dto.savedAlbum.AlbumId;
                    context.AlbumData.Attach(dto.savedAlbum.Album);

                    context.SavedAlbums.Add(dto.savedAlbum);
                    await context.SaveChangesAsync();

                    return (Ok("successfully saved " + dto.savedAlbum.AlbumId + " to your collection " + dto.savedAlbum.CollectionId));
                }
            }
            catch (Exception ex) { return BadRequest(ex); };
        }
        [Route("DeleteAlbumByName")]
        [HttpPost] //saves an album in a specified user's collection
        public async Task<ActionResult> DeleteAlbumByName([FromBody] SaveAlbumByNameDTO dto)//make dto for this shit
        {
            try
            {
                var collection = context.Collections.Where(x => x.OwnerId == HttpContext.Session.GetObject<User>("user").Id && x.Name == dto.collectionName).First();
                dto.savedAlbum.Collection = collection;
                var found = context.SavedAlbums.Where(x => x.UserId == dto.savedAlbum.User.Id && x.AlbumId == dto.savedAlbum.AlbumId && x.CollectionId == dto.savedAlbum.Collection.Id).FirstOrDefault();
                if (found != null)
                {
                    context.SavedAlbums.Remove(found);
                    return (Ok("deleted"));
                }
                else
                {
                    return (Conflict("doesn't exist"));
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

        #region Graphs
        [Route("GetArtistChartValues")]
        [HttpGet]
        public async Task<ActionResult<List<StringAndValue>>> GetArtistChartValues()//not void, should send each artist and how many saved albums the curr user has of them
        {
            try
            {

                var currUserId = HttpContext.Session.GetObject<User>("user").Id;
                var userSaved = context.SavedAlbums.Include(x => x.Album).Where(x => x.UserId == currUserId).ToList();
                var distinctArtists = userSaved.DistinctBy(x => x.Album.ArtistName);
                //StringAndValue[] toReturn = new StringAndValue[distinctArtists.Count()];
                List<StringAndValue> toReturn = new List<StringAndValue>();
                int i = 0;
                foreach (var item in distinctArtists)
                {
                    toReturn.Add(new StringAndValue(item.Album.ArtistName, userSaved.Where(x => x.Album.ArtistName == item.Album.ArtistName).Count()));
                    i++;
                }
                toReturn.Sort(new StringAndValueComparer());
                return (Ok(toReturn));
            }
            catch
            {
                return (BadRequest());
            }

        }

        [Route("GetGenreChartValues")]
        [HttpGet]
        public async Task<ActionResult<List<StringAndValue>>> GetGenreChartValues()//not void, should send each artist and how many saved albums the curr user has of them
        {
            try
            {

                var currUserId = HttpContext.Session.GetObject<User>("user").Id;
                var userSaved = context.SavedAlbums.Include(x => x.Album).Include(x=>x.Album.AlbumGenres).Where(x => x.UserId == currUserId).ToList();
                List<StringAndValue> toReturn = new List<StringAndValue>();
                foreach (var item in userSaved)
                {
                    foreach (var genre in item.Album.AlbumGenres)
                    {
                        bool found = false;
                        foreach(var savedGenre in toReturn)
                        {
                            if(savedGenre.String == genre.Genre)
                            {
                                savedGenre.Value++;
                                found = true;
                                break;
                            }
                        }
                        if (!found) toReturn.Add(new(genre.Genre, 1));
                    }
                }
                toReturn.Sort(new StringAndValueComparer());
                return (Ok(toReturn));
            }
            catch
            {
                return (BadRequest());
            }

        }

        [Route("GetStyleChartValues")]
        [HttpGet]
        public async Task<ActionResult<List<StringAndValue>>> GetStyleChartValues()//not void, should send each artist and how many saved albums the curr user has of them
        {
            try
            {

                var currUserId = HttpContext.Session.GetObject<User>("user").Id;
                var userSaved = context.SavedAlbums.Include(x => x.Album).Include(x => x.Album.AlbumStyles).Where(x => x.UserId == currUserId).ToList();
                List<StringAndValue> toReturn = new List<StringAndValue>();
                foreach (var item in userSaved)
                {
                    foreach (var style in item.Album.AlbumStyles)
                    {
                        bool found = false;
                        foreach (var savedStyle in toReturn)
                        {
                            if (savedStyle.String == style.Style)
                            {
                                savedStyle.Value++;
                                found = true;
                                break;
                            }
                        }
                        if (!found) toReturn.Add(new(style.Style, 1));
                    }
                }
                toReturn.Sort(new StringAndValueComparer());
                return (Ok(toReturn));
            }
            catch
            {
                return (BadRequest());
            }

        }

        [Route("GetYearChartValues")]
        [HttpGet]
        public async Task<ActionResult<List<StringAndValue>>> GetYearChartValues()//not void, should send each artist and how many saved albums the curr user has of them
        {
            try
            {

                var currUserId = HttpContext.Session.GetObject<User>("user").Id;
                var userSaved = context.SavedAlbums.Include(x => x.Album).Include(x => x.Album.AlbumStyles).Where(x => x.UserId == currUserId).ToList();
                List<StringAndValue> toReturn = new List<StringAndValue>();
                for(int i = 1960; i<=2024; i++)
                {
                    toReturn.Add(new StringAndValue(i.ToString(), 0));
                }
                foreach (var item in userSaved)
                {
                    bool found = false;
                    foreach(var year in toReturn)
                    {
                        if(year.String == item.Album.Year.ToString())
                        {
                            year.Value++;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        toReturn.Insert(0, new StringAndValue(item.Album.Year.ToString(), 1));
                    }

                }
                return (Ok(toReturn));
            }
            catch
            {
                return (BadRequest());
            }

        }

        #endregion
    }
}
