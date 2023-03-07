using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class _005_MigrationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_SubitterUserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SubitterUserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SubitterUserId",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SubmitterUserId",
                table: "Tickets",
                column: "SubmitterUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_SubmitterUserId",
                table: "Tickets",
                column: "SubmitterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_SubmitterUserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SubmitterUserId",
                table: "Tickets");

            migrationBuilder.AddColumn<string>(
                name: "SubitterUserId",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SubitterUserId",
                table: "Tickets",
                column: "SubitterUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_SubitterUserId",
                table: "Tickets",
                column: "SubitterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
