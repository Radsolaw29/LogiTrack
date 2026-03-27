using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiTrack.Migrations
{
    /// <inheritdoc />
    public partial class TruckUserIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Trucks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_CreatedById",
                table: "Trucks",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_Users_CreatedById",
                table: "Trucks",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_Users_CreatedById",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_CreatedById",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Trucks");
        }
    }
}
