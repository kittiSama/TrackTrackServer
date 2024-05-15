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

        [Route("SignOut")]
        [HttpGet]
        public async Task<ActionResult> Signout()
        {
            try
            {
                HttpContext.Session.SetObject("user", null);
                return (Ok());
            }
            catch { return BadRequest(); }

        }

        #endregion

        #region Discogs

        [Route("GetClosestAlbumsForApp")]
        [HttpGet] //gets the top 5 results when searching q, returns just their title and id
        public async Task<ActionResult<AlbumAndHeart[]>> GetClosestAlbumsForApp(string q, string SType, string country = "")
        {
            try
            {
                var res = JObject.Parse(await discogs.GetClosestAlbums(q,SType,country));
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
        public async Task<bool> UpdateUser(User user)
        {
            try
            {
                var foundUser = context.Users.Where(x => x.Id == user.Id).FirstOrDefault();
                if (user == null) return false;
                Utils.ValidateUser(user);
                foundUser.Name = user.Name;
                foundUser.Password = user.Password;
                foundUser.Email = user.Email;
                foundUser.Bio = user.Bio;
                await context.SaveChangesAsync();

                HttpContext.Session.SetObject("user", foundUser);

                return true;
            }
            catch (BadDataException ex)
            {
                return false;
            }
            catch (Exception ex) { return false; }
        }
        #endregion

        #region Graphs
        [Route("GetArtistChartValues")]
        [HttpGet]
        public async Task<ActionResult<List<StringAndValue>>> GetArtistChartValues(long id = -1)//not void, should send each artist and how many saved albums the curr user has of them
        {
            try
            {
                long currUserId;
                if (id == -1) { currUserId = HttpContext.Session.GetObject<User>("user").Id; }
                else { currUserId = id; }
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
        public async Task<ActionResult<List<StringAndValue>>> GetGenreChartValues(long id = -1)//not void, should send each artist and how many saved albums the curr user has of them
        {
            try
            {
                long currUserId;
                if (id == -1) { currUserId = HttpContext.Session.GetObject<User>("user").Id; }
                else { currUserId = id; }

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
        public async Task<ActionResult<List<StringAndValue>>> GetStyleChartValues(long id = -1)//not void, should send each artist and how many saved albums the curr user has of them
        {
            try
            {

                long currUserId;
                if (id == -1) { currUserId = HttpContext.Session.GetObject<User>("user").Id; }
                else { currUserId = id; }

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
        public async Task<ActionResult<List<StringAndValue>>> GetYearChartValues(long id = -1)//not void, should send each artist and how many saved albums the curr user has of them
        {
            try
            {

                long currUserId;
                if (id == -1) { currUserId = HttpContext.Session.GetObject<User>("user").Id; }
                else { currUserId = id; }

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
