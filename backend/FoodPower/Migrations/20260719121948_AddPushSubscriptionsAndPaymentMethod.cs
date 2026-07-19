using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodPower.Migrations
{
    /// <inheritdoc />
    public partial class AddPushSubscriptionsAndPaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "payment_method",
                table: "fp_payments",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.CreateTable(
                name: "fp_push_subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    endpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    p256dh = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    auth = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fp_push_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_fp_push_subscriptions_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fp_push_subscriptions_endpoint",
                table: "fp_push_subscriptions",
                column: "endpoint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fp_push_subscriptions_user_id",
                table: "fp_push_subscriptions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fp_push_subscriptions");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "fp_payments");
        }
    }
}
