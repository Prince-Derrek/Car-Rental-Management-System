using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddManualAvailabilityToggle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManuallyDisabled",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManuallyDisabled",
                table: "Vehicles");
        }
    }
}
