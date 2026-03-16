using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyBepAn.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionToNguyenLieu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Cali",
                table: "NguyenLieu",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Calo",
                table: "NguyenLieu",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Dam",
                table: "NguyenLieu",
                type: "float",
                nullable: true);
            // Note: other column type changes were removed to avoid altering tables
            // that may not exist in the target database. This migration only adds
            // the nutrition columns to NguyenLieu.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cali",
                table: "NguyenLieu");

            migrationBuilder.DropColumn(
                name: "Calo",
                table: "NguyenLieu");

            migrationBuilder.DropColumn(
                name: "Dam",
                table: "NguyenLieu");
        }
    }
}
