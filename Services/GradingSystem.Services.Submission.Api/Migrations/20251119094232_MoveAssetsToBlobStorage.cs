using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradingSystem.Services.Submissions.Api.Migrations
{
    /// <inheritdoc />
    public partial class MoveAssetsToBlobStorage : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
