using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaAPI.Migrations
{
    /// <inheritdoc />
    public partial class NuevasColumnasAutores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Renombra la columna conservando los datos existentes
            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Autores",
                newName: "Nombres");

            // Cambia la longitud de nvarchar(max) a nvarchar(150)
            migrationBuilder.AlterColumn<string>(
                name: "Nombres",
                table: "Autores",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                table: "Autores",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Identificacion",
                table: "Autores",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apellido",
                table: "Autores");

            migrationBuilder.DropColumn(
                name: "Identificacion",
                table: "Autores");

            // Devuelve la longitud original
            migrationBuilder.AlterColumn<string>(
                name: "Nombres",
                table: "Autores",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            // Devuelve el nombre original de la columna
            migrationBuilder.RenameColumn(
                name: "Nombres",
                table: "Autores",
                newName: "Nombre");
        }
    }
}
