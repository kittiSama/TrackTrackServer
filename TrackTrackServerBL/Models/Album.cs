using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackTrackServerBL.Models
{
    public class Album
    {
        public string ImageUrl {  get; set; }

        public long AlbumID {  get; set; }

        public string AlbumTitle { get; set; }

        public string ArtistName {  get; set; }
    }
}
