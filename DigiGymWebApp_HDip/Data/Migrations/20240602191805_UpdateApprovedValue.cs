using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigiGymWebApp_HDip.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApprovedValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IsApproved",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
