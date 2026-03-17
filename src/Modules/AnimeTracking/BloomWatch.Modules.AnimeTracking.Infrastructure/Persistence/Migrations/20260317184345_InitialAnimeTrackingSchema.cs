using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAnimeTrackingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "anime_tracking");

            migrationBuilder.CreateTable(
                name: "watch_space_anime",
                schema: "anime_tracking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    watch_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anilist_media_id = table.Column<int>(type: "integer", nullable: false),
                    preferred_title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    episode_count_snapshot = table.Column<int>(type: "integer", nullable: true),
                    cover_image_url_snapshot = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    format = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    season = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    season_year = table.Column<int>(type: "integer", nullable: true),
                    shared_status = table.Column<string>(type: "text", nullable: false),
                    shared_episodes_watched = table.Column<int>(type: "integer", nullable: false),
                    mood = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    vibe = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pitch = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    added_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    added_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_watch_space_anime", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "participant_entries",
                schema: "anime_tracking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    watch_space_anime_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    individual_status = table.Column<string>(type: "text", nullable: false),
                    episodes_watched = table.Column<int>(type: "integer", nullable: false),
                    rating_score = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    rating_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    last_updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participant_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_participant_entries_watch_space_anime_watch_space_anime_id",
                        column: x => x.watch_space_anime_id,
                        principalSchema: "anime_tracking",
                        principalTable: "watch_space_anime",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_participant_entries_anime_user",
                schema: "anime_tracking",
                table: "participant_entries",
                columns: new[] { "watch_space_anime_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_watch_space_anime_space_media",
                schema: "anime_tracking",
                table: "watch_space_anime",
                columns: new[] { "watch_space_id", "anilist_media_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "participant_entries",
                schema: "anime_tracking");

            migrationBuilder.DropTable(
                name: "watch_space_anime",
                schema: "anime_tracking");
        }
    }
}
