using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GradingSystem.Services.Submissions.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionProcessingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubmissionBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmissionFileId = table.Column<int>(type: "integer", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionBatches_SubmissionFiles_SubmissionFileId",
                        column: x => x.SubmissionFileId,
                        principalTable: "SubmissionFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmissionBatchId = table.Column<int>(type: "integer", nullable: false),
                    StudentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    TextHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TemporaryScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ExtractedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionEntries_SubmissionBatches_SubmissionBatchId",
                        column: x => x.SubmissionBatchId,
                        principalTable: "SubmissionBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmissionEntryId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionAssets_SubmissionEntries_SubmissionEntryId",
                        column: x => x.SubmissionEntryId,
                        principalTable: "SubmissionEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionViolations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmissionEntryId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AssetLink = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SubmissionAssetId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionViolations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionViolations_SubmissionAssets_SubmissionAssetId",
                        column: x => x.SubmissionAssetId,
                        principalTable: "SubmissionAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SubmissionViolations_SubmissionEntries_SubmissionEntryId",
                        column: x => x.SubmissionEntryId,
                        principalTable: "SubmissionEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionAssets_SubmissionEntryId",
                table: "SubmissionAssets",
                column: "SubmissionEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionBatches_Status",
                table: "SubmissionBatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionBatches_SubmissionFileId",
                table: "SubmissionBatches",
                column: "SubmissionFileId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionBatches_UploadedByUserId",
                table: "SubmissionBatches",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionEntries_StudentCode",
                table: "SubmissionEntries",
                column: "StudentCode");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionEntries_SubmissionBatchId",
                table: "SubmissionEntries",
                column: "SubmissionBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionEntries_TextHash",
                table: "SubmissionEntries",
                column: "TextHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionViolations_SubmissionAssetId",
                table: "SubmissionViolations",
                column: "SubmissionAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionViolations_SubmissionEntryId",
                table: "SubmissionViolations",
                column: "SubmissionEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubmissionViolations");

            migrationBuilder.DropTable(
                name: "SubmissionAssets");

            migrationBuilder.DropTable(
                name: "SubmissionEntries");

            migrationBuilder.DropTable(
                name: "SubmissionBatches");
        }
    }
}

