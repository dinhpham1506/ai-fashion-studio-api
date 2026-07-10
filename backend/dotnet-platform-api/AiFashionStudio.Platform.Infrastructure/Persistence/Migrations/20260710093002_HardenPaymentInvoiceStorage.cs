using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HardenPaymentInvoiceStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_items_invoices_InvoiceId",
                table: "invoice_items");

            migrationBuilder.RenameColumn(
                name: "InvoiceId",
                table: "invoice_items",
                newName: "invoice_id");

            migrationBuilder.RenameIndex(
                name: "IX_invoice_items_InvoiceId",
                table: "invoice_items",
                newName: "IX_invoice_items_invoice_id");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "payment_orders",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_customer_id",
                table: "invoices",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_payment_id",
                table: "invoices",
                column: "payment_id");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_items_invoices_invoice_id",
                table: "invoice_items",
                column: "invoice_id",
                principalTable: "invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_payment_orders_payment_id",
                table: "invoices",
                column: "payment_id",
                principalTable: "payment_orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_users_customer_id",
                table: "invoices",
                column: "customer_id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_items_invoices_invoice_id",
                table: "invoice_items");

            migrationBuilder.DropForeignKey(
                name: "FK_invoices_payment_orders_payment_id",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_invoices_users_customer_id",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_customer_id",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_payment_id",
                table: "invoices");

            migrationBuilder.RenameColumn(
                name: "invoice_id",
                table: "invoice_items",
                newName: "InvoiceId");

            migrationBuilder.RenameIndex(
                name: "IX_invoice_items_invoice_id",
                table: "invoice_items",
                newName: "IX_invoice_items_InvoiceId");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "payment_orders",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_items_invoices_InvoiceId",
                table: "invoice_items",
                column: "InvoiceId",
                principalTable: "invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
