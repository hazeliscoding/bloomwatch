using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropWatchSessionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "watch_sessions",
                schema: "anime_tracking");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "watch_sessions",
                schema: "anime_tracking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    end_episode = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    session_date_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    start_episode = table.Column<int>(type: "integer", nullable: false),
                    watch_space_anime_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_watch_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_watch_sessions_watch_space_anime_watch_space_anime_id",
                        column: x => x.watch_space_anime_id,
                        principalSchema: "anime_tracking",
                        principalTable: "watch_space_anime",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_watch_sessions_anime",
                schema: "anime_tracking",
                table: "watch_sessions",
                column: "watch_space_anime_id");
        }
    }
}
