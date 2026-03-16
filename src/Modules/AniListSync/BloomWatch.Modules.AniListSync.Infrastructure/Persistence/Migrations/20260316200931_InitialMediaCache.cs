using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloomWatch.Modules.AniListSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMediaCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "anilist_sync");

            migrationBuilder.CreateTable(
                name: "media_cache",
                schema: "anilist_sync",
                columns: table => new
                {
                    anilist_media_id = table.Column<int>(type: "integer", nullable: false),
                    title_romaji = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    title_english = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    title_native = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cover_image_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    episodes = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    format = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    season = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    season_year = table.Column<int>(type: "integer", nullable: true),
                    genres = table.Column<IReadOnlyList<string>>(type: "jsonb", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    average_score = table.Column<int>(type: "integer", nullable: true),
                    popularity = table.Column<int>(type: "integer", nullable: true),
                    cached_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_cache", x => x.anilist_media_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_cache",
                schema: "anilist_sync");
        }
    }
}
