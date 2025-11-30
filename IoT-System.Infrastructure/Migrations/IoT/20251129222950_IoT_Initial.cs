using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoT_System.Infrastructure.Migrations.IoT
{
    /// <inheritdoc />
    public partial class IoT_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "IoT.System");

            migrationBuilder.CreateTable(
                name: "Devices",
                schema: "IoT.System",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ApiKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApiKeyExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceAccessPermissions",
                schema: "IoT.System",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    PermissionType = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAccessPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceAccessPermissions_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "IoT.System",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceFields",
                schema: "IoT.System",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataType = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    WarningMin = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    WarningMax = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    CriticalMin = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    CriticalMax = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceFields_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "IoT.System",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceMeasurements",
                schema: "IoT.System",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RawData = table.Column<string>(type: "text", nullable: true),
                    DataFormat = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MeasurementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ParsedSuccessfully = table.Column<bool>(type: "boolean", nullable: false),
                    ParsingErrors = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceMeasurements_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "IoT.System",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementDateMappings",
                schema: "IoT.System",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataFormat = table.Column<int>(type: "integer", nullable: false),
                    SourceFieldPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TransformationPipeline = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementDateMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeasurementDateMappings_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "IoT.System",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldMappings",
                schema: "IoT.System",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataFormat = table.Column<int>(type: "integer", nullable: false),
                    SourceFieldPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TransformationPipeline = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldMappings_DeviceFields_FieldId",
                        column: x => x.FieldId,
                        principalSchema: "IoT.System",
                        principalTable: "DeviceFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldMeasurementValues",
                schema: "IoT.System",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MeasurementId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldMeasurementValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldMeasurementValues_DeviceFields_FieldId",
                        column: x => x.FieldId,
                        principalSchema: "IoT.System",
                        principalTable: "DeviceFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FieldMeasurementValues_DeviceMeasurements_MeasurementId",
                        column: x => x.MeasurementId,
                        principalSchema: "IoT.System",
                        principalTable: "DeviceMeasurements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAccessPermissions_DeviceId_GroupId",
                schema: "IoT.System",
                table: "DeviceAccessPermissions",
                columns: new[] { "DeviceId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAccessPermissions_DeviceId_RoleId",
                schema: "IoT.System",
                table: "DeviceAccessPermissions",
                columns: new[] { "DeviceId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAccessPermissions_DeviceId_UserId",
                schema: "IoT.System",
                table: "DeviceAccessPermissions",
                columns: new[] { "DeviceId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceFields_DeviceId_FieldName",
                schema: "IoT.System",
                table: "DeviceFields",
                columns: new[] { "DeviceId", "FieldName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceFields_DeviceId_IsActive_DisplayOrder",
                schema: "IoT.System",
                table: "DeviceFields",
                columns: new[] { "DeviceId", "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMeasurements_CreatedOn",
                schema: "IoT.System",
                table: "DeviceMeasurements",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMeasurements_DeviceId_MeasurementDate",
                schema: "IoT.System",
                table: "DeviceMeasurements",
                columns: new[] { "DeviceId", "MeasurementDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMeasurements_DeviceId_ParsedSuccessfully",
                schema: "IoT.System",
                table: "DeviceMeasurements",
                columns: new[] { "DeviceId", "ParsedSuccessfully" });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_ApiKey",
                schema: "IoT.System",
                table: "Devices",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_IsActive_CreatedOn",
                schema: "IoT.System",
                table: "Devices",
                columns: new[] { "IsActive", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_FieldMappings_FieldId_DataFormat_IsActive",
                schema: "IoT.System",
                table: "FieldMappings",
                columns: new[] { "FieldId", "DataFormat", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_FieldMeasurementValues_FieldId",
                schema: "IoT.System",
                table: "FieldMeasurementValues",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldMeasurementValues_MeasurementId_FieldId",
                schema: "IoT.System",
                table: "FieldMeasurementValues",
                columns: new[] { "MeasurementId", "FieldId" });

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementDateMappings_DeviceId_DataFormat_IsActive",
                schema: "IoT.System",
                table: "MeasurementDateMappings",
                columns: new[] { "DeviceId", "DataFormat", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceAccessPermissions",
                schema: "IoT.System");

            migrationBuilder.DropTable(
                name: "FieldMappings",
                schema: "IoT.System");

            migrationBuilder.DropTable(
                name: "FieldMeasurementValues",
                schema: "IoT.System");

            migrationBuilder.DropTable(
                name: "MeasurementDateMappings",
                schema: "IoT.System");

            migrationBuilder.DropTable(
                name: "DeviceFields",
                schema: "IoT.System");

            migrationBuilder.DropTable(
                name: "DeviceMeasurements",
                schema: "IoT.System");

            migrationBuilder.DropTable(
                name: "Devices",
                schema: "IoT.System");
        }
    }
}
