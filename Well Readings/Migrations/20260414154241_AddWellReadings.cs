using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Well_Readings.Migrations
{
    /// <inheritdoc />
    public partial class AddWellReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WellReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WellName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MeterReading = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Chlorine = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Phosphate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ph = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WellReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WellReadings_DailyEntries_DailyEntryId",
                        column: x => x.DailyEntryId,
                        principalTable: "DailyEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WellReadings_DailyEntryId",
                table: "WellReadings",
                column: "DailyEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WellReadings");
        }
    }
}
