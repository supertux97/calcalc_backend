using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace calcalc.Migrations
{
    /// <inheritdoc />
    public partial class foodItemAddInformationUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InformationUrl",
                table: "FoodItem",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InformationUrl",
                table: "FoodItem");
        }
    }
}
