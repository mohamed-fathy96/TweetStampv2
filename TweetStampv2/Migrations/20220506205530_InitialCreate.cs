using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TweetStampv2.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tweets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserScreenName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserFullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserProfileImgUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmbbededTweetHTML = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaUrl1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaUrl2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaUrl3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaUrl4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidationDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeStampInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TsByteArr = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hash = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tweets", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tweets");
        }
    }
}
