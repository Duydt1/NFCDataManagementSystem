using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NFC.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Gender",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionLineId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RoleId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateTable(
                name: "ProductionLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionLines_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLines_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Hearings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Speaker1SPL_1kHz = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NUM = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CH = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionLineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hearings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hearings_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Hearings_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Hearings_ProductionLines_ProductionLineId",
                        column: x => x.ProductionLineId,
                        principalTable: "ProductionLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoryUploads",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    FileContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ProductionLineId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryUploads_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoryUploads_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoryUploads_ProductionLines_ProductionLineId",
                        column: x => x.ProductionLineId,
                        principalTable: "ProductionLines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "KT_MIC_WF_SPLs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SPL_100Hz = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SPL_1kHz = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Polarity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Impedance_1kHz = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MIC1SENS_1kHz = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MIC1Current = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NUM = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CH = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionLineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KT_MIC_WF_SPLs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KT_MIC_WF_SPLs_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KT_MIC_WF_SPLs_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KT_MIC_WF_SPLs_ProductionLines_ProductionLineId",
                        column: x => x.ProductionLineId,
                        principalTable: "ProductionLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KT_TW_SPLs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Grade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SPL_1kHz = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Polarity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    THD_1kHz = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Impedance_1kHz = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NUM = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CH = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionLineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KT_TW_SPLs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KT_TW_SPLs_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KT_TW_SPLs_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KT_TW_SPLs_ProductionLines_ProductionLineId",
                        column: x => x.ProductionLineId,
                        principalTable: "ProductionLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BattVolt = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NUM = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CH = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionLineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sensors_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sensors_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sensors_ProductionLines_ProductionLineId",
                        column: x => x.ProductionLineId,
                        principalTable: "ProductionLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ProductionLineId",
                table: "AspNetUsers",
                column: "ProductionLineId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_RoleId",
                table: "AspNetUsers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Hearings_CreatedById",
                table: "Hearings",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Hearings_ModifiedById",
                table: "Hearings",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Hearings_ProductionLineId",
                table: "Hearings",
                column: "ProductionLineId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryUploads_CreatedById",
                table: "HistoryUploads",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryUploads_ModifiedById",
                table: "HistoryUploads",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryUploads_ProductionLineId",
                table: "HistoryUploads",
                column: "ProductionLineId");

            migrationBuilder.CreateIndex(
                name: "IX_KT_MIC_WF_SPLs_CreatedById",
                table: "KT_MIC_WF_SPLs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_KT_MIC_WF_SPLs_ModifiedById",
                table: "KT_MIC_WF_SPLs",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_KT_MIC_WF_SPLs_ProductionLineId",
                table: "KT_MIC_WF_SPLs",
                column: "ProductionLineId");

            migrationBuilder.CreateIndex(
                name: "IX_KT_TW_SPLs_CreatedById",
                table: "KT_TW_SPLs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_KT_TW_SPLs_ModifiedById",
                table: "KT_TW_SPLs",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_KT_TW_SPLs_ProductionLineId",
                table: "KT_TW_SPLs",
                column: "ProductionLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLines_CreatedById",
                table: "ProductionLines",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLines_ModifiedById",
                table: "ProductionLines",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_CreatedById",
                table: "Sensors",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_ModifiedById",
                table: "Sensors",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_ProductionLineId",
                table: "Sensors",
                column: "ProductionLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetRoles_RoleId",
                table: "AspNetUsers",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ProductionLines_ProductionLineId",
                table: "AspNetUsers",
                column: "ProductionLineId",
                principalTable: "ProductionLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetRoles_RoleId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ProductionLines_ProductionLineId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Hearings");

            migrationBuilder.DropTable(
                name: "HistoryUploads");

            migrationBuilder.DropTable(
                name: "KT_MIC_WF_SPLs");

            migrationBuilder.DropTable(
                name: "KT_TW_SPLs");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropTable(
                name: "ProductionLines");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ProductionLineId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_RoleId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProductionLineId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
