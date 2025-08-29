using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrafficLightCoordinatorData.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "intersections",
                columns: table => new
                {
                    intersection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    location = table.Column<string>(type: "jsonb", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    installed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_intersections", x => x.intersection_id);
                });

            migrationBuilder.CreateTable(
                name: "traffic_configurations",
                columns: table => new
                {
                    config_id = table.Column<Guid>(type: "uuid", nullable: false),
                    intersection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pattern = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    reason = table.Column<string>(type: "text", nullable: true),
                    change_ref = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_traffic_configurations", x => x.config_id);
                    table.ForeignKey(
                        name: "FK_traffic_configurations_intersections_intersection_id",
                        column: x => x.intersection_id,
                        principalTable: "intersections",
                        principalColumn: "intersection_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "traffic_lights",
                columns: table => new
                {
                    light_id = table.Column<Guid>(type: "uuid", nullable: false),
                    intersection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_traffic_lights", x => x.light_id);
                    table.CheckConstraint("ck_traffic_lights_state", "current_state IN ('RED','ORANGE','GREEN','FLASHING','OFF')");
                    table.ForeignKey(
                        name: "FK_traffic_lights_intersections_intersection_id",
                        column: x => x.intersection_id,
                        principalTable: "intersections",
                        principalColumn: "intersection_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_intersections_status",
                table: "intersections",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_cfg_intersection_effective",
                table: "traffic_configurations",
                columns: new[] { "intersection_id", "effective_from" });

            migrationBuilder.CreateIndex(
                name: "IX_traffic_configurations_change_ref",
                table: "traffic_configurations",
                column: "change_ref",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_traffic_lights_intersection_id_updated_at",
                table: "traffic_lights",
                columns: new[] { "intersection_id", "updated_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "traffic_configurations");

            migrationBuilder.DropTable(
                name: "traffic_lights");

            migrationBuilder.DropTable(
                name: "intersections");
        }
    }
}
