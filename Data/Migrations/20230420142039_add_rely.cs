using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADVERTISEMENT.Data.Migrations
{
    public partial class add_rely : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Location",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "starRating",
                table: "Comment",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Rely",
                columns: table => new
                {
                    replyId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(nullable: true),
                    createDate = table.Column<DateTime>(nullable: false),
                    mainContent = table.Column<string>(nullable: true),
                    commentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rely", x => x.replyId);
                    table.ForeignKey(
                        name: "FK_Rely_Comment_commentId",
                        column: x => x.commentId,
                        principalTable: "Comment",
                        principalColumn: "commentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rely_commentId",
                table: "Rely",
                column: "commentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rely");

            migrationBuilder.DropColumn(
                name: "starRating",
                table: "Comment");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Location",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
