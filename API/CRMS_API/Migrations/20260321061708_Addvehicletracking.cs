using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS_API.Migrations
{
    /// <inheritdoc />
    public partial class Addvehicletracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTrackingEnabled",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTrackingEnabled",
                table: "Vehicles");
        }
    }
}
