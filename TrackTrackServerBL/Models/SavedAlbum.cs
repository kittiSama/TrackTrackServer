using System;
using System.Collections.Generic;

namespace TrackTrackServerBL.Models;

public partial class SavedAlbum
{
    public long Id { get; set; }

    public long AlbumId { get; set; }

    public long UserId { get; set; }

    public long CollectionId { get; set; }

    public DateTime Date { get; set; }

    public long? Rating { get; set; }

    
    public virtual AlbumDatum Album { get; set; } = null!;

    public virtual Collection Collection { get; set; } = null!;

    public virtual User User { get; set; } = null!;

}
