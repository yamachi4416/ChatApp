using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ChatApp.Migrations
{
    public partial class ChangeChatRoomKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatRoomAvatars",
                table: "ChatRoomAvatars");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserAvatars",
                type: "text",
                maxLength: 38,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 38);

            migrationBuilder.AddColumn<Guid>(
                name: "ChatRoomAvatarId",
                table: "ChatRooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChatRoomAvatars",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatRoomAvatars",
                table: "ChatRoomAvatars",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomAvatars_ChatRoomId",
                table: "ChatRoomAvatars",
                column: "ChatRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomAvatars_ChatRooms_ChatRoomId",
                table: "ChatRoomAvatars",
                column: "ChatRoomId",
                principalTable: "ChatRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomAvatars_ChatRooms_ChatRoomId",
                table: "ChatRoomAvatars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatRoomAvatars",
                table: "ChatRoomAvatars");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomAvatars_ChatRoomId",
                table: "ChatRoomAvatars");

            migrationBuilder.DropColumn(
                name: "ChatRoomAvatarId",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChatRoomAvatars");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserAvatars",
                maxLength: 38,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 38);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatRoomAvatars",
                table: "ChatRoomAvatars",
                column: "ChatRoomId");
        }
    }
}
