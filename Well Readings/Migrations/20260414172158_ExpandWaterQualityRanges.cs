using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Well_Readings.Migrations
{
    /// <inheritdoc />
    public partial class ExpandWaterQualityRanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Phosphate",
                table: "WellReadings",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(1,1)",
                oldPrecision: 1,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Ph",
                table: "WellReadings",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(2,1)",
                oldPrecision: 2,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MeterReading",
                table: "WellReadings",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,0)",
                oldPrecision: 9);

            migrationBuilder.AlterColumn<decimal>(
                name: "Chlorine",
                table: "WellReadings",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(1,1)",
                oldPrecision: 1,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Temperature",
                table: "FiltrationPlantReadings",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,1)",
                oldPrecision: 4,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Phosphate",
                table: "FiltrationPlantReadings",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,1)",
                oldPrecision: 4,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Chlorine",
                table: "FiltrationPlantReadings",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,1)",
                oldPrecision: 4,
                oldScale: 1,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Phosphate",
                table: "WellReadings",
                type: "decimal(1,1)",
                precision: 1,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,1)",
                oldPrecision: 3,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Ph",
                table: "WellReadings",
                type: "decimal(2,1)",
                precision: 2,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,1)",
                oldPrecision: 3,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MeterReading",
                table: "WellReadings",
                type: "decimal(9,0)",
                precision: 9,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,0)",
                oldPrecision: 18,
                oldScale: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "Chlorine",
                table: "WellReadings",
                type: "decimal(1,1)",
                precision: 1,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,1)",
                oldPrecision: 3,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Temperature",
                table: "FiltrationPlantReadings",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,1)",
                oldPrecision: 3,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Phosphate",
                table: "FiltrationPlantReadings",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,1)",
                oldPrecision: 3,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Chlorine",
                table: "FiltrationPlantReadings",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,1)",
                oldPrecision: 3,
                oldScale: 1,
                oldNullable: true);
        }
    }
}
