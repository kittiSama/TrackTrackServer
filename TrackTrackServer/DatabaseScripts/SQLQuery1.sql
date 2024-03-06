use master
create database TrackTrackDB
go

use TrackTrackDB
go
CREATE TABLE "AlbumGenres"(
    "ID" BIGINT NOT NULL,
    "AlbumID" BIGINT NOT NULL,
    "Genre" NVARCHAR(255) NOT NULL
);
ALTER TABLE
    "AlbumGenres" ADD CONSTRAINT "albumgenres_id_primary" PRIMARY KEY("ID");
CREATE TABLE "Collection"(
    "ID" BIGINT NOT NULL,
    "OwnerID" BIGINT NOT NULL,
    "Name" NVARCHAR(255) NOT NULL
);
ALTER TABLE
    "Collection" ADD CONSTRAINT "collection_id_primary" PRIMARY KEY("ID");
CREATE TABLE "User"(
    "ID" BIGINT NOT NULL,
    "Name" NVARCHAR(255) NOT NULL,
    "Password" NVARCHAR(255) NOT NULL,
    "Email" NVARCHAR(255) NOT NULL,
    "Bio" NVARCHAR(255) NOT NULL
);
ALTER TABLE
    "User" ADD CONSTRAINT "user_id_primary" PRIMARY KEY("ID");
CREATE TABLE "AlbumStyles"(
    "ID" BIGINT NOT NULL,
    "AlbumID" BIGINT NOT NULL,
    "Style" NVARCHAR(255) NOT NULL
);
ALTER TABLE
    "AlbumStyles" ADD CONSTRAINT "albumstyles_id_primary" PRIMARY KEY("ID");
CREATE TABLE "savedAlbums"(
    "ID" BIGINT NOT NULL,
    "AlbumID" BIGINT NOT NULL,
    "UserID" BIGINT NOT NULL,
    "CollectionID" BIGINT NOT NULL,
    "Date" DATETIME NOT NULL,
    "Rating" BIGINT NULL
);
ALTER TABLE
    "savedAlbums" ADD CONSTRAINT "savedalbums_id_primary" PRIMARY KEY("ID");
CREATE TABLE "AlbumData"(
    "ID" BIGINT NOT NULL,
    "Year" BIGINT NOT NULL,
    "ArtistID" BIGINT NOT NULL,
    "ArtistName" NVARCHAR(255) NOT NULL,
    "Country" NVARCHAR(255) NOT NULL
);
ALTER TABLE
    "AlbumData" ADD CONSTRAINT "albumdata_id_primary" PRIMARY KEY("ID");
ALTER TABLE
    "savedAlbums" ADD CONSTRAINT "savedalbums_albumid_foreign" FOREIGN KEY("AlbumID") REFERENCES "AlbumData"("ID");
ALTER TABLE
    "savedAlbums" ADD CONSTRAINT "savedalbums_collectionid_foreign" FOREIGN KEY("CollectionID") REFERENCES "Collection"("ID");
ALTER TABLE
    "savedAlbums" ADD CONSTRAINT "savedalbums_userid_foreign" FOREIGN KEY("UserID") REFERENCES "User"("ID");
ALTER TABLE
    "Collection" ADD CONSTRAINT "collection_ownerid_foreign" FOREIGN KEY("OwnerID") REFERENCES "User"("ID");
ALTER TABLE
    "AlbumGenres" ADD CONSTRAINT "albumgenres_albumid_foreign" FOREIGN KEY("AlbumID") REFERENCES "AlbumData"("ID");
ALTER TABLE
    "AlbumStyles" ADD CONSTRAINT "albumstyles_albumid_foreign" FOREIGN KEY("AlbumID") REFERENCES "AlbumData"("ID");