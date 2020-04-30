using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace mock_sns_core2.Data.Migrations
{
    public partial class fixPhotoColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Photo",
                table: "AspNetUsers",
                maxLength: 2097152,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Photo",
                table: "AspNetUsers",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldMaxLength: 2097152,
                oldNullable: true);
        }
    }
}
