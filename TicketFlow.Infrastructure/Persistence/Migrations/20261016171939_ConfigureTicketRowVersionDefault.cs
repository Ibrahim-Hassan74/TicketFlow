using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureTicketRowVersionDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Tickets",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "md5(random()::text)::bytea",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true);

            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION update_ticket_rowversion()
RETURNS TRIGGER AS $$
BEGIN
    NEW.""RowVersion"" = md5(random()::text)::bytea;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_tickets_rowversion ON ""Tickets"";
CREATE TRIGGER trg_tickets_rowversion
BEFORE UPDATE ON ""Tickets""
FOR EACH ROW
EXECUTE FUNCTION update_ticket_rowversion();
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TRIGGER IF EXISTS trg_tickets_rowversion ON ""Tickets"";
DROP FUNCTION IF EXISTS update_ticket_rowversion();
");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Tickets",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldDefaultValueSql: "md5(random()::text)::bytea");
        }
    }
}
