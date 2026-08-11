using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLoverECommerce.ShopperTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialShopperTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopperEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopperEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopperEvents_CustomerId",
                table: "ShopperEvents",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopperEvents_EventType",
                table: "ShopperEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_ShopperEvents_OccurredAt",
                table: "ShopperEvents",
                column: "OccurredAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopperEvents");
        }
    }
}
