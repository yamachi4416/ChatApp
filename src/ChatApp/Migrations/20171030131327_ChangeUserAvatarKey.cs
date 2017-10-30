using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ChatApp.Migrations
{
    public partial class ChangeUserAvatarKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAvatars",
                table: "UserAvatars");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "UserAvatars",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserAvatarId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAvatars",
                table: "UserAvatars",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserAvatars_UserId",
                table: "UserAvatars",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAvatars_AspNetUsers_UserId",
                table: "UserAvatars",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAvatars_AspNetUsers_UserId",
                table: "UserAvatars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAvatars",
                table: "UserAvatars");

            migrationBuilder.DropIndex(
                name: "IX_UserAvatars_UserId",
                table: "UserAvatars");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserAvatars");

            migrationBuilder.DropColumn(
                name: "UserAvatarId",
                table: "AspNetUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAvatars",
                table: "UserAvatars",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName");
        }
    }
}
