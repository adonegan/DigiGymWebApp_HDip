using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigiGymWebApp_HDip.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFKandNavigationProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeightEntries_ProfileEntries_ProfileID",
                table: "WeightEntries");

            migrationBuilder.DropIndex(
                name: "IX_WeightEntries_ProfileID",
                table: "WeightEntries");

            migrationBuilder.DropColumn(
                name: "ProfileID",
                table: "WeightEntries");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "WeightEntries",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeightEntries_Id",
                table: "WeightEntries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WeightEntries_AspNetUsers_Id",
                table: "WeightEntries",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeightEntries_AspNetUsers_Id",
                table: "WeightEntries");

            migrationBuilder.DropIndex(
                name: "IX_WeightEntries_Id",
                table: "WeightEntries");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "WeightEntries");

            migrationBuilder.AddColumn<int>(
                name: "ProfileID",
                table: "WeightEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WeightEntries_ProfileID",
                table: "WeightEntries",
                column: "ProfileID");

            migrationBuilder.AddForeignKey(
                name: "FK_WeightEntries_ProfileEntries_ProfileID",
                table: "WeightEntries",
                column: "ProfileID",
                principalTable: "ProfileEntries",
                principalColumn: "ProfileID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
