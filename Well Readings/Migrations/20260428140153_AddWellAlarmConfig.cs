using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Well_Readings.Migrations
{
    /// <inheritdoc />
    public partial class AddWellAlarmConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WellAlarmConfigs",
                columns: table => new
                {
                    WellId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HighThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "bit", nullable: false),
                    LastAlarmTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WellAlarmConfigs", x => x.WellId);
                });

            migrationBuilder.CreateTable(
                name: "WellAlarms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WellId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HighLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LowLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WellAlarms", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WellAlarmConfigs");

            migrationBuilder.DropTable(
                name: "WellAlarms");
        }
    }
}
