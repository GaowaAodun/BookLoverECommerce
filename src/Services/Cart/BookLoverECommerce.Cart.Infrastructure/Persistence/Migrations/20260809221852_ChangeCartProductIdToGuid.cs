using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLoverECommerce.Cart.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCartProductIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropIndex(
        name: "IX_CartItems_ShoppingCartId_ProductId",
        table: "CartItems");

    migrationBuilder.DropColumn(
        name: "ProductId",
        table: "CartItems");

    migrationBuilder.AddColumn<Guid>(
        name: "ProductId",
        table: "CartItems",
        type: "uuid",
        nullable: false);

    migrationBuilder.CreateIndex(
        name: "IX_CartItems_ShoppingCartId_ProductId",
        table: "CartItems",
        columns: new[] { "ShoppingCartId", "ProductId" },
        unique: true);
}

        /// <inheritdoc />
       protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropIndex(
        name: "IX_CartItems_ShoppingCartId_ProductId",
        table: "CartItems");

    migrationBuilder.DropColumn(
        name: "ProductId",
        table: "CartItems");

    migrationBuilder.AddColumn<int>(
        name: "ProductId",
        table: "CartItems",
        type: "integer",
        nullable: false,
        defaultValue: 0);

    migrationBuilder.CreateIndex(
        name: "IX_CartItems_ShoppingCartId_ProductId",
        table: "CartItems",
        columns: new[] { "ShoppingCartId", "ProductId" },
        unique: true);
}
    }
}
