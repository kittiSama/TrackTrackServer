using System.Text.Json;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TrackTrackServer.Services
{

    public class DiscogsService :HttpClient
    {
        private HttpClient client;
        const string token = "RSuitAoMoFINnkFeZKWBvlHhnmQmTrdfUnwrdTFC";
        const string URL = @"https://api.discogs.com/";

        public DiscogsService()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "TrackTrack/0.1");
            client.DefaultRequestHeaders.Add("Authorization", "Discogs token=" + token);
            Console.WriteLine("aa");
        }
        
        public async Task<string> GetHello()
        {
            try
            {

                var response = await client.GetAsync(URL + "database/search?q=Nirvana&per_page=1&type=release");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.Out.WriteLine("good");
                    Console.Out.WriteLine(await response.Content.ReadAsStringAsync());
                    var lib = JObject.Parse(await response.Content.ReadAsStringAsync());
                    Console.Out.WriteLine("\n\n");
                    Console.Out.WriteLine(lib["results"][0]["id"]);

                    var release = await client.GetAsync(URL + "releases/" + lib["results"][0]["id"]);
                    Console.WriteLine(await release.Content.ReadAsStringAsync());
                    var lib2 = JObject.Parse(await release.Content.ReadAsStringAsync());
                    await Console.Out.WriteLineAsync("release year: " + lib2["year"] + " genres: " + lib2["genres"][0] + " styles: " + lib2["styles"][0] + " & " + lib2["styles"][1]);

                    return await response.Content.ReadAsStringAsync();
                }
                Console.Out.WriteLine(await response.Content.ReadAsStringAsync());
                Console.Out.WriteLine(response.StatusCode);
                return "Something is Wrong";
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return "ooops";
        }
        public async Task<string> GetClosestAlbums(string q)
        {
            try
            {

                var response = await client.GetAsync(URL + "database/search?q="+q+"&per_page=50&type=release");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return "Something went wrong " + response.StatusCode;
            }
            catch (Exception ex) { return ex.Message; }
            
        }

        public async Task<string> GetAlbumInfo(long id)
        {
            try
            {
                string s = URL + "releases/" + id;
                var response = await client.GetAsync(URL + "releases/" + id);
                if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return "Something went wrong " + response.StatusCode;
            }
            catch (Exception ex) { return ex.Message; }
        
        }
    }

}
