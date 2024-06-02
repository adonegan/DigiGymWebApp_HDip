using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigiGymWebApp_HDip.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "AspNetUsers");
        }
    }
}
