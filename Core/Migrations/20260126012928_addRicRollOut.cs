using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class addRicRollOut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FormRicApprovalResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFormRic = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdApprover = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRicApprovalResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormRicHistoryResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFormRic = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdEditor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Snapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EditedFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRicHistoryResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormRicResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Judul = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hastag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AsIsProcessRasciFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Permasalahan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DampakMasalah = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FaktorPenyebabMasalah = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SolusiSaatIni = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlternatifSolusi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToBeProcessBusinessRasciKkiFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PotensiValueCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExcpectedCompletionTargetFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasilSetelahPerbaikan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrConfirm = table.Column<bool>(type: "bit", nullable: false),
                    SarmConfirm = table.Column<bool>(type: "bit", nullable: false),
                    EcsConfirm = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRicResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NamaDivisi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaPerusahaan = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NamaDivisi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaPerusahaan = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewFormRicResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFormRic = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Catatan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleReview = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewFormRicResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UndanganFormRicResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdBr = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdGroupUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UndanganFormRicResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdGroup = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TandaTanganFile = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdGroup = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TandaTanganFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FormRicHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFormRic = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdEditor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EditedFieldsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRicHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormRicHistories_Users_EditorId",
                        column: x => x.EditorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FormRicRollOuts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdGroupUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Entitas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JudulAplikasi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hashtag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompareWithAsIsHoldingProcessFiles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StkAsIsToBeFiles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsJoinedDomainAdPertamina = table.Column<bool>(type: "bit", nullable: false),
                    IsUsingErpPertamina = table.Column<bool>(type: "bit", nullable: false),
                    IsImplementedRequiredActivation = table.Column<bool>(type: "bit", nullable: false),
                    HasDataCenterConnection = table.Column<bool>(type: "bit", nullable: false),
                    HasRequiredResource = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRicRollOuts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormRicRollOuts_Groups_IdGroupUser",
                        column: x => x.IdGroupUser,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormRicRollOuts_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormRics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdGroupUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Judul = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hastag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AsIsProcessRasciFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Permasalahan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DampakMasalah = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FaktorPenyebabMasalah = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SolusiSaatIni = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlternatifSolusi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToBeProcessBusinessRasciKkiFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PotensiValueCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExcpectedCompletionTargetFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasilSetelahPerbaikan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrConfirm = table.Column<bool>(type: "bit", nullable: false),
                    SarmConfirm = table.Column<bool>(type: "bit", nullable: false),
                    EcsConfirm = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormRics_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormRics_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UndanganFormRics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdBr = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdGroupUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BrId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UndanganFormRics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UndanganFormRics_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UndanganFormRics_Users_BrId",
                        column: x => x.BrId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UndanganFormRics_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FormRicRollOutApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFormRicRollOut = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdApprover = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormRicRollOutId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRicRollOutApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormRicRollOutApprovals_FormRicRollOuts_FormRicRollOutId",
                        column: x => x.FormRicRollOutId,
                        principalTable: "FormRicRollOuts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormRicRollOutApprovals_Users_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FormRicRollOutHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFormRicRollOut = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdEditor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EditedFieldsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FormRicRollOutId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRicRollOutHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormRicRollOutHistories_FormRicRollOuts_FormRicRollOutId",
                        column: x => x.FormRicRollOutId,
                        principalTable: "FormRicRollOuts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormRicRollOutHistories_Users_EditorId",
                        column: x => x.EditorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReviewFormRicRollOuts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFormRicRollOut = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Catatan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleReview = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormRicRollOutId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewFormRicRollOuts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewFormRicRollOuts_FormRicRollOuts_FormRicRollOutId",
                        column: x => x.FormRicRollOutId,
                        principalTable: "FormRicRollOuts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReviewFormRicRollOuts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FormRicApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFormRic = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdApprover = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormRicId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRicApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormRicApprovals_FormRics_FormRicId",
                        column: x => x.FormRicId,
                        principalTable: "FormRics",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormRicApprovals_Users_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReviewFormRics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFormRic = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Catatan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleReview = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormRicId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewFormRics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewFormRics_FormRics_FormRicId",
                        column: x => x.FormRicId,
                        principalTable: "FormRics",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReviewFormRics_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Groups",
                columns: new[] { "Id", "NamaDivisi", "NamaPerusahaan" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "Enterprise Data Management", "PT Pertamina (Persero)" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "Business RIC", "PT Pertamina (Persero)" },
                    { new Guid("30000000-0000-0000-0000-000000000003"), "Strategic & Risk Management", "PT Pertamina (Persero)" },
                    { new Guid("40000000-0000-0000-0000-000000000004"), "Enterprise Corporate Strategy", "PT Pertamina (Persero)" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "GroupId", "IdGroup", "Name", "PasswordHash", "Position", "Role", "TandaTanganFile" },
                values: new object[,]
                {
                    { new Guid("11000000-0000-0000-0000-000000000001"), "user.member@pertamina.com", null, new Guid("10000000-0000-0000-0000-000000000001"), "User Member", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "Staff", 0, null },
                    { new Guid("11000000-0000-0000-0000-000000000002"), "user.pic@pertamina.com", null, new Guid("10000000-0000-0000-0000-000000000001"), "Fadhil Rabbani", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "PIC", 1, null },
                    { new Guid("11000000-0000-0000-0000-000000000003"), "user.manager@pertamina.com", null, new Guid("10000000-0000-0000-0000-000000000001"), "User Manager", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "Manager", 2, null },
                    { new Guid("11000000-0000-0000-0000-000000000004"), "user.vp@pertamina.com", null, new Guid("10000000-0000-0000-0000-000000000001"), "User VP", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "Vice President", 3, null },
                    { new Guid("22000000-0000-0000-0000-000000000001"), "br.pic@pertamina.com", null, new Guid("20000000-0000-0000-0000-000000000002"), "BR PIC", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "PIC", 4, null },
                    { new Guid("22000000-0000-0000-0000-000000000002"), "br.manager@pertamina.com", null, new Guid("20000000-0000-0000-0000-000000000002"), "BR Manager", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "Manager", 6, null },
                    { new Guid("22000000-0000-0000-0000-000000000003"), "br.vp@pertamina.com", null, new Guid("20000000-0000-0000-0000-000000000002"), "BR VP", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "Vice President", 7, null },
                    { new Guid("22000000-0000-0000-0000-000000000004"), "br.member@pertamina.com", null, new Guid("20000000-0000-0000-0000-000000000002"), "BR Member", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "Staff", 5, null },
                    { new Guid("33000000-0000-0000-0000-000000000001"), "sarm.pic@pertamina.com", null, new Guid("30000000-0000-0000-0000-000000000003"), "SARM PIC", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "PIC", 8, null },
                    { new Guid("33000000-0000-0000-0000-000000000002"), "sarm.manager@pertamina.com", null, new Guid("30000000-0000-0000-0000-000000000003"), "SARM Manager", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "Manager", 10, null },
                    { new Guid("33000000-0000-0000-0000-000000000003"), "sarm.vp@pertamina.com", null, new Guid("30000000-0000-0000-0000-000000000003"), "SARM VP", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "Vice President", 11, null },
                    { new Guid("33000000-0000-0000-0000-000000000004"), "sarm.member@pertamina.com", null, new Guid("30000000-0000-0000-0000-000000000003"), "SARM Member", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "Staff", 9, null },
                    { new Guid("44000000-0000-0000-0000-000000000001"), "ecs.pic@pertamina.com", null, new Guid("40000000-0000-0000-0000-000000000004"), "ECS PIC", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "PIC", 12, null },
                    { new Guid("44000000-0000-0000-0000-000000000002"), "ecs.manager@pertamina.com", null, new Guid("40000000-0000-0000-0000-000000000004"), "ECS Manager", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "Manager", 14, null },
                    { new Guid("44000000-0000-0000-0000-000000000003"), "ecs.vp@pertamina.com", null, new Guid("40000000-0000-0000-0000-000000000004"), "ECS VP", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "Vice President", 15, null },
                    { new Guid("44000000-0000-0000-0000-000000000004"), "ecs.member@pertamina.com", null, new Guid("40000000-0000-0000-0000-000000000004"), "ECS Member", "$2y$12$OKEN8e5STPrspBDtwA7.uOZ1JAURhlNpuyvXyl6WEomPgi2KoruXu", "Staff", 13, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormRicApprovals_ApproverId",
                table: "FormRicApprovals",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRicApprovals_FormRicId",
                table: "FormRicApprovals",
                column: "FormRicId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRicHistories_EditorId",
                table: "FormRicHistories",
                column: "EditorId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRicRollOutApprovals_ApproverId",
                table: "FormRicRollOutApprovals",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRicRollOutApprovals_FormRicRollOutId",
                table: "FormRicRollOutApprovals",
                column: "FormRicRollOutId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRicRollOutHistories_EditorId",
                table: "FormRicRollOutHistories",
                column: "EditorId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRicRollOutHistories_FormRicRollOutId",
                table: "FormRicRollOutHistories",
                column: "FormRicRollOutId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRicRollOuts_IdGroupUser",
                table: "FormRicRollOuts",
                column: "IdGroupUser");

            migrationBuilder.CreateIndex(
                name: "IX_FormRicRollOuts_IdUser",
                table: "FormRicRollOuts",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_FormRics_GroupId",
                table: "FormRics",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRics_UserId",
                table: "FormRics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewFormRicRollOuts_FormRicRollOutId",
                table: "ReviewFormRicRollOuts",
                column: "FormRicRollOutId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewFormRicRollOuts_UserId",
                table: "ReviewFormRicRollOuts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewFormRics_FormRicId",
                table: "ReviewFormRics",
                column: "FormRicId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewFormRics_UserId",
                table: "ReviewFormRics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UndanganFormRics_BrId",
                table: "UndanganFormRics",
                column: "BrId");

            migrationBuilder.CreateIndex(
                name: "IX_UndanganFormRics_GroupId",
                table: "UndanganFormRics",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UndanganFormRics_UserId",
                table: "UndanganFormRics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GroupId",
                table: "Users",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormRicApprovalResponses");

            migrationBuilder.DropTable(
                name: "FormRicApprovals");

            migrationBuilder.DropTable(
                name: "FormRicHistories");

            migrationBuilder.DropTable(
                name: "FormRicHistoryResponses");

            migrationBuilder.DropTable(
                name: "FormRicResponses");

            migrationBuilder.DropTable(
                name: "FormRicRollOutApprovals");

            migrationBuilder.DropTable(
                name: "FormRicRollOutHistories");

            migrationBuilder.DropTable(
                name: "GroupResponses");

            migrationBuilder.DropTable(
                name: "ReviewFormRicResponses");

            migrationBuilder.DropTable(
                name: "ReviewFormRicRollOuts");

            migrationBuilder.DropTable(
                name: "ReviewFormRics");

            migrationBuilder.DropTable(
                name: "UndanganFormRicResponses");

            migrationBuilder.DropTable(
                name: "UndanganFormRics");

            migrationBuilder.DropTable(
                name: "UserResponses");

            migrationBuilder.DropTable(
                name: "FormRicRollOuts");

            migrationBuilder.DropTable(
                name: "FormRics");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
