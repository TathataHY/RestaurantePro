using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionCamposRecientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comandas_Mesas_MesaId",
                schema: "Operaciones",
                table: "Comandas");

            migrationBuilder.AlterColumn<Guid>(
                name: "MesaId",
                schema: "Operaciones",
                table: "Comandas",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "DireccionEntrega",
                schema: "Operaciones",
                table: "Comandas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreEntrega",
                schema: "Operaciones",
                table: "Comandas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoEntrega",
                schema: "Operaciones",
                table: "Comandas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comandas_Mesas_MesaId",
                schema: "Operaciones",
                table: "Comandas",
                column: "MesaId",
                principalSchema: "Operaciones",
                principalTable: "Mesas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comandas_Mesas_MesaId",
                schema: "Operaciones",
                table: "Comandas");

            migrationBuilder.DropColumn(
                name: "DireccionEntrega",
                schema: "Operaciones",
                table: "Comandas");

            migrationBuilder.DropColumn(
                name: "NombreEntrega",
                schema: "Operaciones",
                table: "Comandas");

            migrationBuilder.DropColumn(
                name: "TelefonoEntrega",
                schema: "Operaciones",
                table: "Comandas");

            migrationBuilder.AlterColumn<Guid>(
                name: "MesaId",
                schema: "Operaciones",
                table: "Comandas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comandas_Mesas_MesaId",
                schema: "Operaciones",
                table: "Comandas",
                column: "MesaId",
                principalSchema: "Operaciones",
                principalTable: "Mesas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
