using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiTrack.Migrations
{
    /// <inheritdoc />
    public partial class DriverUserIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Drivers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CreatedById",
                table: "Drivers",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Users_CreatedById",
                table: "Drivers",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Users_CreatedById",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_CreatedById",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Drivers");
        }
    }
}
