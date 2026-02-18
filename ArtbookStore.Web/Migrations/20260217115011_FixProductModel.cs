using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtbookStore.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixProductModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Name", table: "Products");

            migrationBuilder.RenameColumn(name: "ProductId", table: "Products", newName: "Id");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
            );

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: ""
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CreatedAt", table: "Products");

            migrationBuilder.DropColumn(name: "StockQuantity", table: "Products");

            migrationBuilder.DropColumn(name: "Title", table: "Products");

            migrationBuilder.RenameColumn(name: "Id", table: "Products", newName: "ProductId");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true
            );
        }
    }
}
