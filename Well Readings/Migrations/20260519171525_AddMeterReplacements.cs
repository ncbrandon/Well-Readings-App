using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Well_Readings.Migrations
{
    /// <inheritdoc />
    public partial class AddMeterReplacements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsumptionReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastReadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentReadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BillingDays = table.Column<int>(type: "int", nullable: false),
                    WaterPumped = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    WaterConsumed = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    WaterLoss = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    LossPercent = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    PumpedAveragePerDay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConsumedAveragePerDay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumptionReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeterReplacements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReplacementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldMeterFinalReading = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewMeterStartingReading = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterReplacements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionReports_LastReadDate_CurrentReadDate",
                table: "ConsumptionReports",
                columns: new[] { "LastReadDate", "CurrentReadDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumptionReports");

            migrationBuilder.DropTable(
                name: "MeterReplacements");
        }
    }
}
