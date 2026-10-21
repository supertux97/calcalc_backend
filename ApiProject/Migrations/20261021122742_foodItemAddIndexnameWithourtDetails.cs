using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace calcalc.Migrations
{
    /// <inheritdoc />
    public partial class foodItemAddIndexnameWithourtDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NameWithoutDetails",
                table: "FoodItem",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);

            migrationBuilder.CreateIndex(
                name: "IX_FoodItem_NameWithoutDetails",
                table: "FoodItem",
                column: "NameWithoutDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FoodItem_NameWithoutDetails",
                table: "FoodItem");

            migrationBuilder.AlterColumn<string>(
                name: "NameWithoutDetails",
                table: "FoodItem",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true);
        }
    }
}
