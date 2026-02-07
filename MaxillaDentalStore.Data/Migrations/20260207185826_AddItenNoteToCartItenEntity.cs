using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaxillaDentalStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddItenNoteToCartItenEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemNotes",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemNotes",
                table: "CartItems");
        }
    }
}
