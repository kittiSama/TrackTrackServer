using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackTrackServerBL.Models
{
    public class SaveAlbumByNameDTO
    {
        public SavedAlbum savedAlbum { get; set; } = null!;
        public string collectionName { get; set; } = null!;
    }
}
