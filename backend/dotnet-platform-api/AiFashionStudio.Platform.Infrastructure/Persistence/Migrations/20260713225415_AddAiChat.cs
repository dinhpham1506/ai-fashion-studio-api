using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ai_chat");

            migrationBuilder.CreateTable(
                name: "conversations",
                schema: "ai_chat",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    page_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    related_product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    related_order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "ai_chat",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    intent = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_messages_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalSchema: "ai_chat",
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "support_tickets",
                schema: "ai_chat",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    issue_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    severity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    assigned_to = table.Column<Guid>(type: "uuid", nullable: true),
                    summary = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_tickets", x => x.id);
                    table.ForeignKey(
                        name: "FK_support_tickets_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalSchema: "ai_chat",
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tool_runs",
                schema: "ai_chat",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tool_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    input_json = table.Column<string>(type: "jsonb", nullable: true),
                    output_summary_json = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tool_runs", x => x.id);
                    table.ForeignKey(
                        name: "FK_tool_runs_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalSchema: "ai_chat",
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_conversations_related_order_id",
                schema: "ai_chat",
                table: "conversations",
                column: "related_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_related_product_id",
                schema: "ai_chat",
                table: "conversations",
                column: "related_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_status",
                schema: "ai_chat",
                table: "conversations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_user_id",
                schema: "ai_chat",
                table: "conversations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_conversation_id",
                schema: "ai_chat",
                table: "messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_intent",
                schema: "ai_chat",
                table: "messages",
                column: "intent");

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_conversation_id",
                schema: "ai_chat",
                table: "support_tickets",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_order_id",
                schema: "ai_chat",
                table: "support_tickets",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_status",
                schema: "ai_chat",
                table: "support_tickets",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_user_id",
                schema: "ai_chat",
                table: "support_tickets",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tool_runs_conversation_id",
                schema: "ai_chat",
                table: "tool_runs",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_tool_runs_tool_name",
                schema: "ai_chat",
                table: "tool_runs",
                column: "tool_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "messages",
                schema: "ai_chat");

            migrationBuilder.DropTable(
                name: "support_tickets",
                schema: "ai_chat");

            migrationBuilder.DropTable(
                name: "tool_runs",
                schema: "ai_chat");

            migrationBuilder.DropTable(
                name: "conversations",
                schema: "ai_chat");
        }
    }
}
