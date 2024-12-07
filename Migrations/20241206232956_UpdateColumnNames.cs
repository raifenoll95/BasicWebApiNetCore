using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace progresApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "Correo",
                table: "Users",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Contraseña",
                table: "Users",
                newName: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Users",
                newName: "Correo");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "Contraseña");
        }
    }
}
