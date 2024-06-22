using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigiGymWebApp_HDip.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateWeightEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "ProfileEntries");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "ProfileEntries");

            migrationBuilder.CreateTable(
                name: "WeightEntries",
                columns: table => new
                {
                    WeightID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProfileID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeightEntries", x => x.WeightID);
                    table.ForeignKey(
                        name: "FK_WeightEntries_ProfileEntries_ProfileID",
                        column: x => x.ProfileID,
                        principalTable: "ProfileEntries",
                        principalColumn: "ProfileID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeightEntries_ProfileID",
                table: "WeightEntries",
                column: "ProfileID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeightEntries");

            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "ProfileEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "ProfileEntries",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
