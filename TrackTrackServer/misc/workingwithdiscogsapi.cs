/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace trying_to_work_with_discogs_api
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var serv = new WebService();
            serv.init();
            await serv.GetHello();
            while (true) ;
        }
    }


    public class WebService
    {
        private HttpClient client;
        string token = "RSuitAoMoFINnkFeZKWBvlHhnmQmTrdfUnwrdTFC";
        const string URL = @"https://api.discogs.com/";
        JsonSerializer js = new JsonSerializer();


        public void init()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "TrackTrack/0.1");
            client.DefaultRequestHeaders.Add("Authorization", "Discogs token=" + token);

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
    }

}
*/