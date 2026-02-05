using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaxillaDentalStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class modifyingIndexesForPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_PackageId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "Index_Product_IsActive",
                table: "products");

            migrationBuilder.DropIndex(
                name: "Index_Order_Date",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "Index_User_Name",
                table: "Users",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_PackageId_CreatedAt",
                table: "Reviews",
                columns: new[] { "PackageId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "Index_Product_IsActive",
                table: "products",
                columns: new[] { "IsActive", "Price" });

            migrationBuilder.CreateIndex(
                name: "Index_User_Id_Order_Date",
                table: "Orders",
                columns: new[] { "UserId", "OrderDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Index_User_Name",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_PackageId_CreatedAt",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "Index_Product_IsActive",
                table: "products");

            migrationBuilder.DropIndex(
                name: "Index_User_Id_Order_Date",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_PackageId",
                table: "Reviews",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "Index_Product_IsActive",
                table: "products",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "Index_Order_Date",
                table: "Orders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");
        }
    }
}
