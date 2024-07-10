using AnonymousWordBackend.Contexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AnonymousWordBackend.Migrations;

[DbContext(typeof(DatabaseContext))]
[Migration("AddRolesColumnToUserModel")]
public class AddRolesColumnToUserModel : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<short>(
            name: "roles",
            table: "users",
            type: "smallint",
            nullable: false,
            defaultValue: 1);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "roles",
            table: "users");
    }
}