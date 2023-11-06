using System;
using System.Collections.Generic;

namespace TrackTrackServerBL.Models;

public partial class SavedAlbum
{
    public long Id { get; set; }

    public string AlbumId { get; set; } = null!;

    public long UserId { get; set; }

    public DateTime Date { get; set; }

    public long Rating { get; set; }

    public virtual User User { get; set; } = null!;
}
