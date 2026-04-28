using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Well_Readings.Migrations
{
    /// <inheritdoc />
    public partial class UpdateScadaHistoryPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WellName",
                table: "ScadaHistoryPoints");

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "ScadaHistoryPoints",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "MetricType",
                table: "ScadaHistoryPoints",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "ScadaHistoryPoints",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceColumn",
                table: "ScadaHistoryPoints",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ScadaHistoryPoints_Timestamp_Location_MetricType",
                table: "ScadaHistoryPoints",
                columns: new[] { "Timestamp", "Location", "MetricType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScadaHistoryPoints_Timestamp_Location_MetricType",
                table: "ScadaHistoryPoints");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "ScadaHistoryPoints");

            migrationBuilder.DropColumn(
                name: "SourceColumn",
                table: "ScadaHistoryPoints");

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "ScadaHistoryPoints",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MetricType",
                table: "ScadaHistoryPoints",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "WellName",
                table: "ScadaHistoryPoints",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
