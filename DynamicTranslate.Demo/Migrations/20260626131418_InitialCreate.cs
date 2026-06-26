using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DynamicTranslate.Demo.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Translation");

            migrationBuilder.CreateTable(
                name: "LookupCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OverrideTranslations",
                schema: "Translation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Entity = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Property = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Text = table.Column<string>(type: "nvarchar(3500)", maxLength: 3500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverrideTranslations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupMasters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LookupMasters_LookupCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "LookupCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OverrideTranslationDetails",
                schema: "Translation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OverrideTranslationId = table.Column<long>(type: "bigint", nullable: false),
                    Translation = table.Column<string>(type: "nvarchar(3500)", maxLength: 3500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverrideTranslationDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OverrideTranslationDetails_OverrideTranslations_OverrideTranslationId",
                        column: x => x.OverrideTranslationId,
                        principalSchema: "Translation",
                        principalTable: "OverrideTranslations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LookupDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MasterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LookupDetails_LookupMasters_MasterId",
                        column: x => x.MasterId,
                        principalTable: "LookupMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "LookupCategories",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1L, "books" });

            migrationBuilder.InsertData(
                table: "LookupMasters",
                columns: new[] { "Id", "CategoryId", "Name" },
                values: new object[,]
                {
                    { 1L, 1L, "Computer Science" },
                    { 2L, 1L, "Physics" },
                    { 3L, 1L, "Mathematics" },
                    { 4L, 1L, "Biology" },
                    { 5L, 1L, "History" }
                });

            migrationBuilder.InsertData(
                table: "LookupDetails",
                columns: new[] { "Id", "MasterId", "Name" },
                values: new object[,]
                {
                    { 1L, 1L, "Clean Code: A Handbook of Agile Software Craftsmanship" },
                    { 2L, 1L, "Design Patterns: Elements of Reusable Object-Oriented Software" },
                    { 3L, 2L, "Quantum Physics" },
                    { 4L, 2L, "Hardware" },
                    { 5L, 1L, "Introduction to Algorithms" },
                    { 6L, 1L, "The Pragmatic Programmer" },
                    { 7L, 1L, "Computer Networking: A Top-Down Approach" },
                    { 8L, 2L, "The Feynman Lectures on Physics" },
                    { 9L, 2L, "Introduction to Quantum Mechanics" },
                    { 10L, 2L, "Classical Mechanics" },
                    { 11L, 2L, "Thermodynamics and Statistical Mechanics" },
                    { 12L, 3L, "Calculus: Early Transcendentals" },
                    { 13L, 3L, "Linear Algebra and Its Applications" },
                    { 14L, 3L, "Probability and Statistics" },
                    { 15L, 3L, "Discrete Mathematics and Its Applications" },
                    { 16L, 4L, "Molecular Biology of the Cell" },
                    { 17L, 4L, "Genetics: Analysis of Genes and Genomes" },
                    { 18L, 4L, "Ecology: Concepts and Applications" },
                    { 19L, 5L, "Sapiens: A Brief History of Humankind" },
                    { 20L, 5L, "The History of the Ancient World" },
                    { 21L, 5L, "A People's History of the United States" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LookupDetails_MasterId",
                table: "LookupDetails",
                column: "MasterId");

            migrationBuilder.CreateIndex(
                name: "IX_LookupMasters_CategoryId",
                table: "LookupMasters",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OverrideTranslationDetails_OverrideTranslationId",
                schema: "Translation",
                table: "OverrideTranslationDetails",
                column: "OverrideTranslationId");

            migrationBuilder.CreateIndex(
                name: "IX_OverrideTranslations_Entity_Property_Key_LanguageCode",
                schema: "Translation",
                table: "OverrideTranslations",
                columns: new[] { "Entity", "Property", "Key", "LanguageCode" },
                unique: true,
                filter: "[Entity] IS NOT NULL AND [Property] IS NOT NULL AND [Key] IS NOT NULL AND [LanguageCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OverrideTranslations_Text",
                schema: "Translation",
                table: "OverrideTranslations",
                column: "Text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LookupDetails");

            migrationBuilder.DropTable(
                name: "OverrideTranslationDetails",
                schema: "Translation");

            migrationBuilder.DropTable(
                name: "LookupMasters");

            migrationBuilder.DropTable(
                name: "OverrideTranslations",
                schema: "Translation");

            migrationBuilder.DropTable(
                name: "LookupCategories");
        }
    }
}
