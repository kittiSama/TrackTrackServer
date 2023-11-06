Use master
Create Database TrackTrackDB
Go

Use TrackTrackDB
Go

CREATE TABLE "User"(
    "ID" BIGINT NOT NULL,
    "Name" NVARCHAR(255) NOT NULL,
    "Password" NVARCHAR(255) NOT NULL,
    "Email" NVARCHAR(255) NOT NULL,
    "Bio" NVARCHAR(255) NOT NULL
);
ALTER TABLE
    "User" ADD CONSTRAINT "user_id_primary" PRIMARY KEY("ID");
CREATE TABLE "savedAlbums"(
    "ID" BIGINT NOT NULL,
    "AlbumID" NCHAR(255) NOT NULL,
    "UserID" BIGINT NOT NULL,
    "Date" DATETIME NOT NULL,
    "Rating" BIGINT NOT NULL
);
ALTER TABLE
    "savedAlbums" ADD CONSTRAINT "savedalbums_id_primary" PRIMARY KEY("ID");
ALTER TABLE
    "savedAlbums" ADD CONSTRAINT "savedalbums_userid_foreign" FOREIGN KEY("UserID") REFERENCES "User"("ID");