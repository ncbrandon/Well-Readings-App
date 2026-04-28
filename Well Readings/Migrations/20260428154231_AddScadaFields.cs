using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Well_Readings.Migrations
{
    /// <inheritdoc />
    public partial class AddScadaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiltrationPlantReadings_DailyEntries_DailyEntryId",
                table: "FiltrationPlantReadings");

            migrationBuilder.DropIndex(
                name: "IX_FiltrationPlantReadings_DailyEntryId",
                table: "FiltrationPlantReadings");

            migrationBuilder.DropColumn(
                name: "DailyEntryId",
                table: "FiltrationPlantReadings");

            migrationBuilder.DropColumn(
                name: "FilterPlantMeterReading",
                table: "FiltrationPlantReadings");

            migrationBuilder.DropColumn(
                name: "MtJeffersonMeterReading",
                table: "FiltrationPlantReadings");

            migrationBuilder.DropColumn(
                name: "Phosphate",
                table: "FiltrationPlantReadings");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "WellReadings",
                newName: "Timestamp");

            migrationBuilder.AlterColumn<Guid>(
                name: "DailyEntryId",
                table: "WellReadings",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<decimal>(
                name: "AlarmThreshold",
                table: "WellReadings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAlarm",
                table: "WellReadings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "Temperature",
                table: "FiltrationPlantReadings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,1)",
                oldPrecision: 3,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Ph",
                table: "FiltrationPlantReadings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,1)",
                oldPrecision: 3,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Chlorine",
                table: "FiltrationPlantReadings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,1)",
                oldPrecision: 3,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FlowRate",
                table: "FiltrationPlantReadings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsAlarm",
                table: "FiltrationPlantReadings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "FiltrationPlantReadings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "Turbidity",
                table: "FiltrationPlantReadings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "FiltrationPlantReadingId",
                table: "DailyEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScadaHistoryPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WellName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MetricType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScadaHistoryPoints", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyEntries_FiltrationPlantReadingId",
                table: "DailyEntries",
                column: "FiltrationPlantReadingId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyEntries_FiltrationPlantReadings_FiltrationPlantReadingId",
                table: "DailyEntries",
                column: "FiltrationPlantReadingId",
                principalTable: "FiltrationPlantReadings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyEntries_FiltrationPlantReadings_FiltrationPlantReadingId",
                table: "DailyEntries");

            migrationBuilder.DropTable(
                name: "ScadaHistoryPoints");

            migrationBuilder.DropIndex(
                name: "IX_DailyEntries_FiltrationPlantReadingId",
                table: "DailyEntries");

            migrationBuilder.DropColumn(
                name: "AlarmThreshold",
                table: "WellReadings");

            migrationBuilder.DropColumn(
                name: "IsAlarm",
                table: "WellReadings");

            migrationBuilder.DropColumn(
                name: "FlowRate",
                table: "FiltrationPlantReadings");

            migrationBuilder.DropColumn(
                name: "IsAlarm",
                table: "FiltrationPlantReadings");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "FiltrationPlantReadings");

            migrationBuilder.DropColumn(
                name: "Turbidity",
                table: "FiltrationPlantReadings");

            migrationBuilder.DropColumn(
                name: "FiltrationPlantReadingId",
                table: "DailyEntries");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "WellReadings",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<Guid>(
                name: "DailyEntryId",
                table: "WellReadings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Temperature",
                table: "FiltrationPlantReadings",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Ph",
                table: "FiltrationPlantReadings",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Chlorine",
                table: "FiltrationPlantReadings",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<Guid>(
                name: "DailyEntryId",
                table: "FiltrationPlantReadings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "FilterPlantMeterReading",
                table: "FiltrationPlantReadings",
                type: "decimal(18,0)",
                precision: 18,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MtJeffersonMeterReading",
                table: "FiltrationPlantReadings",
                type: "decimal(18,0)",
                precision: 18,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Phosphate",
                table: "FiltrationPlantReadings",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FiltrationPlantReadings_DailyEntryId",
                table: "FiltrationPlantReadings",
                column: "DailyEntryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FiltrationPlantReadings_DailyEntries_DailyEntryId",
                table: "FiltrationPlantReadings",
                column: "DailyEntryId",
                principalTable: "DailyEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
