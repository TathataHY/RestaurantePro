using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMesaExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                schema: "Operaciones",
                table: "Mesas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsAccesible",
                schema: "Operaciones",
                table: "Mesas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                schema: "Operaciones",
                table: "Mesas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TieneEnchufe",
                schema: "Operaciones",
                table: "Mesas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TieneSofa",
                schema: "Operaciones",
                table: "Mesas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TieneVentana",
                schema: "Operaciones",
                table: "Mesas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                schema: "Operaciones",
                table: "Mesas");

            migrationBuilder.DropColumn(
                name: "EsAccesible",
                schema: "Operaciones",
                table: "Mesas");

            migrationBuilder.DropColumn(
                name: "Notas",
                schema: "Operaciones",
                table: "Mesas");

            migrationBuilder.DropColumn(
                name: "TieneEnchufe",
                schema: "Operaciones",
                table: "Mesas");

            migrationBuilder.DropColumn(
                name: "TieneSofa",
                schema: "Operaciones",
                table: "Mesas");

            migrationBuilder.DropColumn(
                name: "TieneVentana",
                schema: "Operaciones",
                table: "Mesas");
        }
    }
}
