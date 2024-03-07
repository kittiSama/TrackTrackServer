using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TrackTrackServerBL.Models;

public partial class TrackTrackDbContext : DbContext
{
    public TrackTrackDbContext()
    {
    }

    public TrackTrackDbContext(DbContextOptions<TrackTrackDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AlbumDatum> AlbumData { get; set; }

    public virtual DbSet<AlbumGenre> AlbumGenres { get; set; }

    public virtual DbSet<AlbumStyle> AlbumStyles { get; set; }

    public virtual DbSet<Collection> Collections { get; set; }

    public virtual DbSet<SavedAlbum> SavedAlbums { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\sqlexpress;Database=TrackTrackDB;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlbumDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("albumdata_id_primary");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ArtistId).HasColumnName("ArtistID");
            entity.Property(e => e.ArtistName).HasMaxLength(255);
            entity.Property(e => e.Country).HasMaxLength(255);
        });

        modelBuilder.Entity<AlbumGenre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("albumgenres_id_primary");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.AlbumId).HasColumnName("AlbumID");
            entity.Property(e => e.Genre).HasMaxLength(255);

            entity.HasOne(d => d.Album).WithMany(p => p.AlbumGenres)
                .HasForeignKey(d => d.AlbumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("albumgenres_albumid_foreign");
        });

        modelBuilder.Entity<AlbumStyle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("albumstyles_id_primary");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.AlbumId).HasColumnName("AlbumID");
            entity.Property(e => e.Style).HasMaxLength(255);

            entity.HasOne(d => d.Album).WithMany(p => p.AlbumStyles)
                .HasForeignKey(d => d.AlbumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("albumstyles_albumid_foreign");
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("collection_id_primary");

            entity.ToTable("Collection");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.OwnerId).HasColumnName("OwnerID");

            entity.HasOne(d => d.Owner).WithMany(p => p.Collections)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("collection_ownerid_foreign");
        });

        modelBuilder.Entity<SavedAlbum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("savedalbums_id_primary");

            entity.ToTable("savedAlbums");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.AlbumId).HasColumnName("AlbumID");
            entity.Property(e => e.CollectionId).HasColumnName("CollectionID");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Album).WithMany(p => p.SavedAlbums)
                .HasForeignKey(d => d.AlbumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("savedalbums_albumid_foreign");

            entity.HasOne(d => d.Collection).WithMany(p => p.SavedAlbums)
                .HasForeignKey(d => d.CollectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("savedalbums_collectionid_foreign");

            entity.HasOne(d => d.User).WithMany(p => p.SavedAlbums)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("savedalbums_userid_foreign");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_id_primary");

            entity.ToTable("User");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Bio).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
