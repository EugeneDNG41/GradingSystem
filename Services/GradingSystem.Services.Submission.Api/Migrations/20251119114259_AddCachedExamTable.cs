using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GradingSystem.Services.Submissions.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCachedExamTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "SubmissionAssets");

            migrationBuilder.AddColumn<string>(
                name: "BlobName",
                table: "SubmissionAssets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CachedExams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SemesterId = table.Column<int>(type: "integer", nullable: false),
                    CachedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedExams", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedExams");

            migrationBuilder.DropColumn(
                name: "BlobName",
                table: "SubmissionAssets");

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "SubmissionAssets",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
