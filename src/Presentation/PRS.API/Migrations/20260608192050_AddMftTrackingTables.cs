using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMftTrackingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        

            migrationBuilder.CreateTable(
                name: "MFT_File_History",
                columns: table => new
                {
                    FileId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordsReceived = table.Column<int>(type: "int", nullable: false),
                    RecordsProcessed = table.Column<int>(type: "int", nullable: false),
                    RecordsFailed = table.Column<int>(type: "int", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFT_File_History", x => x.FileId);
                });


            migrationBuilder.CreateTable(
                name: "Personnel_MFT_Audit",
                columns: table => new
                {
                    AuditId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedField = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personnel_MFT_Audit", x => x.AuditId);
                });


            migrationBuilder.CreateTable(
                name: "MFT_File_Errors",
                columns: table => new
                {
                    ErrorId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<long>(type: "bigint", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileHistoryFileId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFT_File_Errors", x => x.ErrorId);
                    table.ForeignKey(
                        name: "FK_MFT_File_Errors_MFT_File_History_FileHistoryFileId",
                        column: x => x.FileHistoryFileId,
                        principalTable: "MFT_File_History",
                        principalColumn: "FileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MFT_File_Staging",
                columns: table => new
                {
                    StagingId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<long>(type: "bigint", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmploymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkOffice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineOfService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortfolioRequired = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryCode = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    ValidationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidationMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessingStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileHistoryFileId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MFT_File_Staging", x => x.StagingId);
                    table.ForeignKey(
                        name: "FK_MFT_File_Staging_MFT_File_History_FileHistoryFileId",
                        column: x => x.FileHistoryFileId,
                        principalTable: "MFT_File_History",
                        principalColumn: "FileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MFT_File_Errors_FileHistoryFileId",
                table: "MFT_File_Errors",
                column: "FileHistoryFileId");

            migrationBuilder.CreateIndex(
                name: "IX_MFT_File_Staging_FileHistoryFileId",
                table: "MFT_File_Staging",
                column: "FileHistoryFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropTable(
                name: "MFT_File_Errors");

            migrationBuilder.DropTable(
                name: "MFT_File_Staging");

            migrationBuilder.DropTable(
                name: "Personnel_MFT_Audit");

            migrationBuilder.DropTable(
                name: "MFT_File_History");

        }
    }
}
