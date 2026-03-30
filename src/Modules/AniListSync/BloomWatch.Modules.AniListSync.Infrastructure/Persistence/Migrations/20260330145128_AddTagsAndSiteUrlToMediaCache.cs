using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloomWatch.Modules.AniListSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsAndSiteUrlToMediaCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "site_url",
                schema: "anilist_sync",
                table: "media_cache",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tags",
                schema: "anilist_sync",
                table: "media_cache",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "site_url",
                schema: "anilist_sync",
                table: "media_cache");

            migrationBuilder.DropColumn(
                name: "tags",
                schema: "anilist_sync",
                table: "media_cache");
        }
    }
}
