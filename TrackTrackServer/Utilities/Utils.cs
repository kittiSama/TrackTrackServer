using TrackTrackServerBL.Models;

namespace TrackTrackServer.Utilities
{
    public class Utils
    {
        static int MAXIDVALUE = 1000000;

        public static long GenerateUniqueId(string type, Random rnd, TrackTrackDbContext context)
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
                case ("AlbumGenre"):

                    while (context.AlbumGenres.Where(u => u.Id == i).FirstOrDefault() != null)
                    {
                        i = rnd.Next(MAXIDVALUE);
                    }
                    break;
                case ("AlbumStyle"):

                    while (context.AlbumStyles.Where(u => u.Id == i).FirstOrDefault() != null)
                    {
                        i = rnd.Next(MAXIDVALUE);
                    }
                    break;
                default: throw (new Exception("no such type"));
            }
            return i;
        }

        public static void ValidateUser(User user)
        {
            if (user.Name.Length < 4) throw new BadDataException("name must be at least 4 characters");
            if (!user.Email.Contains("@")) throw new BadDataException("email must be valid");
            if (user.Password.Length < 6) throw new BadDataException("password must be at least 6 characters");
        }
    }
}
