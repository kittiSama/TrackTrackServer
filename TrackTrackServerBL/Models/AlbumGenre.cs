using System;
using System.Collections.Generic;

namespace TrackTrackServerBL.Models;

public partial class AlbumGenre
{
    public long Id { get; set; }

    public long AlbumId { get; set; }

    public string Genre { get; set; } = null!;

    public virtual AlbumDatum Album { get; set; } = null!;
}
