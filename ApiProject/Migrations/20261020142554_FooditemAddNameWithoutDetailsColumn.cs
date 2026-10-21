using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace calcalc.Migrations
{
    /// <inheritdoc />
    public partial class FooditemAddNameWithoutDetailsColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameWithoutDetails",
                table: "FoodItem",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameWithoutDetails",
                table: "FoodItem");
        }
    }
}
