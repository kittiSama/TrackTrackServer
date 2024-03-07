using System;
using System.Collections.Generic;

namespace TrackTrackServerBL.Models;

public partial class AlbumStyle
{
    public long Id { get; set; }

    public long AlbumId { get; set; }

    public string Style { get; set; } = null!;

    public virtual AlbumDatum Album { get; set; } = null!;
}
