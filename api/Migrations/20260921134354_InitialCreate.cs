using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "amenities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    cost = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_amenities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "halls",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    cost_per_hour = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_halls", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    hall_id = table.Column<int>(type: "integer", nullable: false),
                    hall_base_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    hall_discounted_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    amenities_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    total_cost = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookings", x => x.id);
                    table.ForeignKey(
                        name: "fk_bookings_halls_hall_id",
                        column: x => x.hall_id,
                        principalTable: "halls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hall_amenities",
                columns: table => new
                {
                    hall_id = table.Column<int>(type: "integer", nullable: false),
                    amenity_id = table.Column<int>(type: "integer", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hall_amenities", x => new { x.hall_id, x.amenity_id });
                    table.ForeignKey(
                        name: "fk_hall_amenities_amenities_amenity_id",
                        column: x => x.amenity_id,
                        principalTable: "amenities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_hall_amenities_halls_hall_id",
                        column: x => x.hall_id,
                        principalTable: "halls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_amenities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_id = table.Column<int>(type: "integer", nullable: false),
                    amenity_id = table.Column<int>(type: "integer", nullable: false),
                    price_at_booking = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_booking_amenities", x => x.id);
                    table.ForeignKey(
                        name: "fk_booking_amenities_amenities_amenity_id",
                        column: x => x.amenity_id,
                        principalTable: "amenities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_booking_amenities_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_booking_amenities_amenity_id",
                table: "booking_amenities",
                column: "amenity_id");

            migrationBuilder.CreateIndex(
                name: "ix_booking_amenities_booking_id",
                table: "booking_amenities",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_hall_id",
                table: "bookings",
                column: "hall_id");

            migrationBuilder.CreateIndex(
                name: "ix_hall_amenities_amenity_id",
                table: "hall_amenities",
                column: "amenity_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_amenities");

            migrationBuilder.DropTable(
                name: "hall_amenities");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "amenities");

            migrationBuilder.DropTable(
                name: "halls");
        }
    }
}
