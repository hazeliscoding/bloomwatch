using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialWatchSpacesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "watch_spaces");

            migrationBuilder.CreateTable(
                name: "watch_spaces",
                schema: "watch_spaces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_watch_spaces", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invitations",
                schema: "watch_spaces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    watch_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    accepted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.id);
                    table.ForeignKey(
                        name: "FK_invitations_watch_spaces_watch_space_id",
                        column: x => x.watch_space_id,
                        principalSchema: "watch_spaces",
                        principalTable: "watch_spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "watch_space_members",
                schema: "watch_spaces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    watch_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    joined_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_watch_space_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_watch_space_members_watch_spaces_watch_space_id",
                        column: x => x.watch_space_id,
                        principalSchema: "watch_spaces",
                        principalTable: "watch_spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_invitations_token",
                schema: "watch_spaces",
                table: "invitations",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invitations_watch_space_id",
                schema: "watch_spaces",
                table: "invitations",
                column: "watch_space_id");

            migrationBuilder.CreateIndex(
                name: "ix_watch_space_members_space_user",
                schema: "watch_spaces",
                table: "watch_space_members",
                columns: new[] { "watch_space_id", "user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitations",
                schema: "watch_spaces");

            migrationBuilder.DropTable(
                name: "watch_space_members",
                schema: "watch_spaces");

            migrationBuilder.DropTable(
                name: "watch_spaces",
                schema: "watch_spaces");
        }
    }
}
