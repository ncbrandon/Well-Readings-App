using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Well_Readings.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenancePumpInstalls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaintenancePumpInstalls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PumpType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstalledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenancePumpInstalls", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePumpInstalls_SiteName_PumpType_InstalledDate",
                table: "MaintenancePumpInstalls",
                columns: new[] { "SiteName", "PumpType", "InstalledDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenancePumpInstalls");
        }
    }
}
