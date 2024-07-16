using AnonymousWordBackend.Contexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace AnonymousWordBackend.Migrations;

[DbContext(typeof(DatabaseContext))]
[Migration("AddBanListModel")]
public class AddBanListModel : Migration
{
/// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ban_list",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                is_banned = table.Column<bool>(type: "boolean", nullable: false, comment: "Is User banned."),
                message_id = table.Column<long>(type: "bigint", nullable: false, comment: "Message ID from the issuer chat."),
                issuer = table.Column<long>(type: "bigint", nullable: false),
                banned = table.Column<long>(type: "bigint", nullable: false),
                banned_on = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ban_list", x => x.id);
                table.ForeignKey(
                    name: "FK_ban_list_users_banned",
                    column: x => x.banned,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ban_list_users_issuer",
                    column: x => x.issuer,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ban_list_banned",
            table: "ban_list",
            column: "banned");

        migrationBuilder.CreateIndex(
            name: "IX_ban_list_issuer",
            table: "ban_list",
            column: "issuer");

        migrationBuilder.CreateIndex(
            name: "IX_messages_author",
            table: "messages",
            column: "author");

        migrationBuilder.CreateIndex(
            name: "IX_messages_recipient",
            table: "messages",
            column: "recipient");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ban_list");
    }
}