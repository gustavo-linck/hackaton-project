using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UmbLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxLengthConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQLite does not enforce column length constraints (VARCHAR(n)).
            // MaxLength limits are enforced at the application layer:
            // HTML maxlength attributes, FluentValidation, and service guards.
            // The model snapshot has been updated for other DB provider compatibility.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
