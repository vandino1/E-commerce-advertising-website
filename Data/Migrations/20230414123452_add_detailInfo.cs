using Microsoft.EntityFrameworkCore.Migrations;

namespace ADVERTISEMENT.Data.Migrations
{
    public partial class add_detailInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "detailInfo",
                table: "Advertisement",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "detailInfo",
                table: "Advertisement");
        }
    }
}
