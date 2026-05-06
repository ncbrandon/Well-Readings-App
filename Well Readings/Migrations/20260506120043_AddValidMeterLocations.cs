using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Well_Readings.Migrations
{
    /// <inheritdoc />
    public partial class AddValidMeterLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ValidMeterLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IncludeInMeterTotal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidMeterLocations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ValidMeterLocations",
                columns: new[] { "Id", "DisplayName", "IncludeInMeterTotal", "Location" },
                values: new object[,]
                {
                    { 1, "Reeves Well Meter Reading", true, "Reeves Well" },
                    { 2, "Reeves Well A Meter Reading", true, "Reeves Well A" },
                    { 3, "Park Well Meter Reading", true, "Park Well" },
                    { 4, "Park Well A Meter Reading", true, "Park Well A" },
                    { 5, "Park Well B Meter Reading", true, "Park Well B" },
                    { 6, "Woods Well Meter Reading", true, "Woods" },
                    { 7, "Catawissa Well Meter Reading", true, "Catawissa" },
                    { 8, "New Well Meter Reading", true, "New" },
                    { 9, "Oakwood Well Meter Reading", true, "Oakwood" },
                    { 10, "Ray Well Meter Reading", true, "Ray" },
                    { 11, "Filter Plant Meter Reading", true, "Filter Plant" },
                    { 12, "Mt. Jefferson Meter Reading", true, "Mt. Jefferson" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ValidMeterLocations");
        }
    }
}
