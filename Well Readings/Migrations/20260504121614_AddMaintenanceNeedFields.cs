using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Well_Readings.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceNeedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedChemicalPump",
                table: "MaintenanceSiteStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedChlorine",
                table: "MaintenanceSiteStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedInjector",
                table: "MaintenanceSiteStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedPhosphate",
                table: "MaintenanceSiteStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedChemicalPump",
                table: "MaintenanceSiteStatuses");

            migrationBuilder.DropColumn(
                name: "NeedChlorine",
                table: "MaintenanceSiteStatuses");

            migrationBuilder.DropColumn(
                name: "NeedInjector",
                table: "MaintenanceSiteStatuses");

            migrationBuilder.DropColumn(
                name: "NeedPhosphate",
                table: "MaintenanceSiteStatuses");
        }
    }
}
