using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaxillaDentalStore.Data.Migrations
{
    public partial class AddReviewIdToNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReviewId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ReviewId",
                table: "Notifications",
                column: "ReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Reviews_ReviewId",
                table: "Notifications",
                column: "ReviewId",
                principalTable: "Reviews",
                principalColumn: "ReviewId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Reviews_ReviewId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ReviewId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReviewId",
                table: "Notifications");
        }
    }
}
