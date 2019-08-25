using Microsoft.EntityFrameworkCore.Migrations;

namespace LmsApp.API.Migrations
{
    public partial class AddedInterestsUserProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Interests",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Interests",
                table: "Users");
        }
    }
}
