using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddBorrowedByUserIdToBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BorrowedByUserId",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_BorrowedByUserId",
                table: "Books",
                column: "BorrowedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Users_BorrowedByUserId",
                table: "Books",
                column: "BorrowedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Users_BorrowedByUserId",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_BorrowedByUserId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "BorrowedByUserId",
                table: "Books");
        }
    }
}
