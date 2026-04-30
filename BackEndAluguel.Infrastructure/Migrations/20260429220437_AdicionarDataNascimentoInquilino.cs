using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndAluguel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDataNascimentoInquilino : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DataNascimento",
                table: "Inquilinos",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Inquilinos");
        }
    }
}
