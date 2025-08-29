using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace calcalc.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoodItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Calories = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserAdded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodUnit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AmountDeciliters = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodUnit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodSynonym",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FoodItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodSynonym", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodSynonym_FoodItem_FoodItemId",
                        column: x => x.FoodItemId,
                        principalTable: "FoodItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodHasUnit",
                columns: table => new
                {
                    FoodUnitId = table.Column<int>(type: "int", nullable: false),
                    FoodItemId = table.Column<int>(type: "int", nullable: false),
                    GramsPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodHasUnit", x => new { x.FoodItemId, x.FoodUnitId });
                    table.ForeignKey(
                        name: "FK_FoodHasUnit_FoodItem_FoodItemId",
                        column: x => x.FoodItemId,
                        principalTable: "FoodItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodHasUnit_FoodUnit_FoodUnitId",
                        column: x => x.FoodUnitId,
                        principalTable: "FoodUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodUnitSynonyms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FoodUnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodUnitSynonyms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodUnitSynonyms_FoodUnit_FoodUnitId",
                        column: x => x.FoodUnitId,
                        principalTable: "FoodUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodHasUnit_FoodUnitId",
                table: "FoodHasUnit",
                column: "FoodUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItem_Name",
                table: "FoodItem",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoodSynonym_FoodItemId",
                table: "FoodSynonym",
                column: "FoodItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodUnitSynonyms_FoodUnitId",
                table: "FoodUnitSynonyms",
                column: "FoodUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodHasUnit");

            migrationBuilder.DropTable(
                name: "FoodSynonym");

            migrationBuilder.DropTable(
                name: "FoodUnitSynonyms");

            migrationBuilder.DropTable(
                name: "FoodItem");

            migrationBuilder.DropTable(
                name: "FoodUnit");
        }
    }
}
