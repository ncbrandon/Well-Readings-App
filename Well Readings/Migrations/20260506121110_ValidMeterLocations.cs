using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Well_Readings.Migrations
{
    /// <inheritdoc />
    public partial class ValidMeterLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "ValidMeterLocations");

            migrationBuilder.DropColumn(
                name: "IncludeInMeterTotal",
                table: "ValidMeterLocations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "ValidMeterLocations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IncludeInMeterTotal",
                table: "ValidMeterLocations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "ValidMeterLocations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DisplayName", "IncludeInMeterTotal" },
                values: new object[] { "Reeves Well Meter Reading", true });

            migrationBuilder.UpdateData(
                table: "ValidMeterLocations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DisplayName", "IncludeInMeterTotal" },
                values: new object[] { "Reeves Well A Meter Reading", true });

            migrationBuilder.UpdateData(
                table: "ValidMeterLocations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DisplayName", "IncludeInMeterTotal" },
                values: new object[] { "Park Well Meter Reading", true });

            migrationBuilder.UpdateData(
                table: "ValidMeterLocations",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DisplayName", "IncludeInMeterTotal" },
                values: new object[] { "Park Well A Meter Reading", true });

            migrationBuilder.UpdateData(
                table: "ValidMeterLocations",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DisplayName", "IncludeInMeterTotal" },
                values: new object[] { "Park Well B Meter Reading", true });

            migrationBuilder.UpdateData(
                table: "ValidMeterLocations",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "DisplayName", "IncludeInMeterTotal" },
                values: new object[] { "Woods Well Meter Reading", true });

            migrationBuilder.UpdateData(
                table: "ValidMeterLocations",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "DisplayName", "IncludeInMeterTotal" },
                values: new object[] { "Catawissa Well Meter Reading", true });

            migrationBuilder.UpdateData(
                table: "ValidMeterLocations",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "DisplayName", "IncludeInMeterTotal" },
                values: new object[] { "New Well Meter Reading", true });

            migrationBuilder.UpdateData(
                table: "ValidMeterLocations",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "DisplayName", "IncludeInMeterTotal" },
                values: new object[] { "Oakwood Well Meter Reading", true });

            migrationBuilder.UpdateData(
                table: "ValidMeterLocations",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "DisplayName", "IncludeInMeterTotal" },
                values: new object[] { "Ray Well Meter Reading", true });

            migrationBuilder.UpdateData(
                table: "ValidMeterLocations",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "DisplayName", "IncludeInMeterTotal" },
                values: new object[] { "Filter Plant Meter Reading", true });

            migrationBuilder.UpdateData(
                table: "ValidMeterLocations",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "DisplayName", "IncludeInMeterTotal" },
                values: new object[] { "Mt. Jefferson Meter Reading", true });
        }
    }
}
