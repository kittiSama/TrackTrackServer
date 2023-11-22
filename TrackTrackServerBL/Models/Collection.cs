using System;
using System.Collections.Generic;

namespace TrackTrackServerBL.Models;

public partial class Collection
{
    public long Id { get; set; }

    public long OwnerId { get; set; }

    public long Name { get; set; }

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<SavedAlbum> SavedAlbums { get; set; } = new List<SavedAlbum>();
}
