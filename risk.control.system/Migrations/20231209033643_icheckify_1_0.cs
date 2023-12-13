using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace risk.control.system.Migrations
{
    /// <inheritdoc />
    public partial class icheckify_1_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentReport",
                columns: table => new
                {
                    AgentReportId = table.Column<string>(type: "TEXT", nullable: false),
                    AgentEmail = table.Column<string>(type: "TEXT", nullable: true),
                    AgentRemarksUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AgentRemarks = table.Column<string>(type: "TEXT", nullable: true),
                    AgentLocationPictureUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AgentLocationPicture = table.Column<byte[]>(type: "BLOB", nullable: true),
                    AgentOcrUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AgentOcrPicture = table.Column<byte[]>(type: "BLOB", nullable: true),
                    AgentOcrData = table.Column<string>(type: "TEXT", nullable: true),
                    AgentQrUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AgentQrPicture = table.Column<byte[]>(type: "BLOB", nullable: true),
                    QrData = table.Column<string>(type: "TEXT", nullable: true),
                    LongLat = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentReport", x => x.AgentReportId);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    TableName = table.Column<string>(type: "TEXT", nullable: true),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OldValues = table.Column<string>(type: "TEXT", nullable: true),
                    NewValues = table.Column<string>(type: "TEXT", nullable: true),
                    AffectedColumns = table.Column<string>(type: "TEXT", nullable: true),
                    PrimaryKey = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BeneficiaryRelation",
                columns: table => new
                {
                    BeneficiaryRelationId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficiaryRelation", x => x.BeneficiaryRelationId);
                });

            migrationBuilder.CreateTable(
                name: "CaseEnabler",
                columns: table => new
                {
                    CaseEnablerId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseEnabler", x => x.CaseEnablerId);
                });

            migrationBuilder.CreateTable(
                name: "CostCentre",
                columns: table => new
                {
                    CostCentreId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCentre", x => x.CostCentreId);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    CountryId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.CountryId);
                });

            migrationBuilder.CreateTable(
                name: "FilesOnDatabase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FileType = table.Column<string>(type: "TEXT", nullable: false),
                    Extension = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CompanyId = table.Column<string>(type: "TEXT", nullable: true),
                    UploadedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Saved = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilesOnDatabase", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FilesOnFileSystem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FileType = table.Column<string>(type: "TEXT", nullable: false),
                    Extension = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CompanyId = table.Column<string>(type: "TEXT", nullable: true),
                    UploadedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Saved = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilesOnFileSystem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvestigationCaseOutcome",
                columns: table => new
                {
                    InvestigationCaseOutcomeId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationCaseOutcome", x => x.InvestigationCaseOutcomeId);
                });

            migrationBuilder.CreateTable(
                name: "InvestigationCaseStatus",
                columns: table => new
                {
                    InvestigationCaseStatusId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    MasterData = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationCaseStatus", x => x.InvestigationCaseStatusId);
                });

            migrationBuilder.CreateTable(
                name: "LineOfBusiness",
                columns: table => new
                {
                    LineOfBusinessId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    MasterData = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineOfBusiness", x => x.LineOfBusinessId);
                });

            migrationBuilder.CreateTable(
                name: "UploadClaim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Policy = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<string>(type: "TEXT", nullable: false),
                    IssueDate = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: false),
                    ServiceType = table.Column<string>(type: "TEXT", nullable: false),
                    DateOfIncident = table.Column<string>(type: "TEXT", nullable: false),
                    CauseOfLoss = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    Dept = table.Column<string>(type: "TEXT", nullable: false),
                    PDocument = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CustomerType = table.Column<string>(type: "TEXT", nullable: false),
                    Gender = table.Column<string>(type: "TEXT", nullable: false),
                    CustomerDob = table.Column<string>(type: "TEXT", nullable: false),
                    Contact = table.Column<string>(type: "TEXT", nullable: false),
                    Education = table.Column<string>(type: "TEXT", nullable: false),
                    Occupation = table.Column<string>(type: "TEXT", nullable: false),
                    Income = table.Column<string>(type: "TEXT", nullable: false),
                    CAddress = table.Column<string>(type: "TEXT", nullable: false),
                    Pincode = table.Column<string>(type: "TEXT", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: false),
                    CPhoto = table.Column<string>(type: "TEXT", nullable: true),
                    BeneficiaryName = table.Column<string>(type: "TEXT", nullable: false),
                    Relation = table.Column<string>(type: "TEXT", nullable: false),
                    BeneficiaryDob = table.Column<string>(type: "TEXT", nullable: false),
                    BeneficiaryIncome = table.Column<string>(type: "TEXT", nullable: false),
                    BeneficiaryContact = table.Column<string>(type: "TEXT", nullable: false),
                    BAddress = table.Column<string>(type: "TEXT", nullable: false),
                    BPincode = table.Column<string>(type: "TEXT", nullable: false),
                    BPhoto = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadClaim", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "State",
                columns: table => new
                {
                    StateId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    CountryId = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_State", x => x.StateId);
                    table.ForeignKey(
                        name: "FK_State_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestigationCaseSubStatus",
                columns: table => new
                {
                    InvestigationCaseSubStatusId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    InvestigationCaseStatusId = table.Column<string>(type: "TEXT", nullable: true),
                    MasterData = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationCaseSubStatus", x => x.InvestigationCaseSubStatusId);
                    table.ForeignKey(
                        name: "FK_InvestigationCaseSubStatus_InvestigationCaseStatus_InvestigationCaseStatusId",
                        column: x => x.InvestigationCaseStatusId,
                        principalTable: "InvestigationCaseStatus",
                        principalColumn: "InvestigationCaseStatusId");
                });

            migrationBuilder.CreateTable(
                name: "InvestigationServiceType",
                columns: table => new
                {
                    InvestigationServiceTypeId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    LineOfBusinessId = table.Column<string>(type: "TEXT", nullable: false),
                    MasterData = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationServiceType", x => x.InvestigationServiceTypeId);
                    table.ForeignKey(
                        name: "FK_InvestigationServiceType_LineOfBusiness_LineOfBusinessId",
                        column: x => x.LineOfBusinessId,
                        principalTable: "LineOfBusiness",
                        principalColumn: "LineOfBusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "District",
                columns: table => new
                {
                    DistrictId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    StateId = table.Column<string>(type: "TEXT", nullable: true),
                    CountryId = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_District", x => x.DistrictId);
                    table.ForeignKey(
                        name: "FK_District_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_District_State_StateId",
                        column: x => x.StateId,
                        principalTable: "State",
                        principalColumn: "StateId");
                });

            migrationBuilder.CreateTable(
                name: "InvestigationCase",
                columns: table => new
                {
                    InvestigationId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    LineOfBusinessId = table.Column<string>(type: "TEXT", nullable: true),
                    InvestigationServiceTypeId = table.Column<string>(type: "TEXT", nullable: true),
                    InvestigationCaseStatusId = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationCase", x => x.InvestigationId);
                    table.ForeignKey(
                        name: "FK_InvestigationCase_InvestigationCaseStatus_InvestigationCaseStatusId",
                        column: x => x.InvestigationCaseStatusId,
                        principalTable: "InvestigationCaseStatus",
                        principalColumn: "InvestigationCaseStatusId");
                    table.ForeignKey(
                        name: "FK_InvestigationCase_InvestigationServiceType_InvestigationServiceTypeId",
                        column: x => x.InvestigationServiceTypeId,
                        principalTable: "InvestigationServiceType",
                        principalColumn: "InvestigationServiceTypeId");
                    table.ForeignKey(
                        name: "FK_InvestigationCase_LineOfBusiness_LineOfBusinessId",
                        column: x => x.LineOfBusinessId,
                        principalTable: "LineOfBusiness",
                        principalColumn: "LineOfBusinessId");
                });

            migrationBuilder.CreateTable(
                name: "PinCode",
                columns: table => new
                {
                    PinCodeId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Latitude = table.Column<string>(type: "TEXT", nullable: true),
                    Longitude = table.Column<string>(type: "TEXT", nullable: true),
                    DistrictId = table.Column<string>(type: "TEXT", nullable: true),
                    StateId = table.Column<string>(type: "TEXT", nullable: true),
                    CountryId = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinCode", x => x.PinCodeId);
                    table.ForeignKey(
                        name: "FK_PinCode_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PinCode_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "DistrictId");
                    table.ForeignKey(
                        name: "FK_PinCode_State_StateId",
                        column: x => x.StateId,
                        principalTable: "State",
                        principalColumn: "StateId");
                });

            migrationBuilder.CreateTable(
                name: "ClientCompany",
                columns: table => new
                {
                    ClientCompanyId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Branch = table.Column<string>(type: "TEXT", nullable: false),
                    Addressline = table.Column<string>(type: "TEXT", nullable: false),
                    StateId = table.Column<string>(type: "TEXT", nullable: true),
                    CountryId = table.Column<string>(type: "TEXT", nullable: true),
                    PinCodeId = table.Column<string>(type: "TEXT", nullable: true),
                    DistrictId = table.Column<string>(type: "TEXT", nullable: true),
                    BankName = table.Column<string>(type: "TEXT", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "TEXT", nullable: false),
                    IFSCCode = table.Column<string>(type: "TEXT", nullable: false),
                    AgreementDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActivatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: true),
                    DocumentUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DocumentImage = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Auto = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerifyOcr = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApiBaseUrl = table.Column<string>(type: "TEXT", nullable: false),
                    PanIdfyUrl = table.Column<string>(type: "TEXT", nullable: false),
                    RapidAPIKey = table.Column<string>(type: "TEXT", nullable: false),
                    RapidAPITaskId = table.Column<string>(type: "TEXT", nullable: false),
                    RapidAPIGroupId = table.Column<string>(type: "TEXT", nullable: false),
                    RapidAPIPanRemainCount = table.Column<string>(type: "TEXT", nullable: true),
                    SendSMS = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCompany", x => x.ClientCompanyId);
                    table.ForeignKey(
                        name: "FK_ClientCompany_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_ClientCompany_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "DistrictId");
                    table.ForeignKey(
                        name: "FK_ClientCompany_PinCode_PinCodeId",
                        column: x => x.PinCodeId,
                        principalTable: "PinCode",
                        principalColumn: "PinCodeId");
                    table.ForeignKey(
                        name: "FK_ClientCompany_State_StateId",
                        column: x => x.StateId,
                        principalTable: "State",
                        principalColumn: "StateId");
                });

            migrationBuilder.CreateTable(
                name: "CustomerDetail",
                columns: table => new
                {
                    CustomerDetailId = table.Column<string>(type: "TEXT", nullable: false),
                    CustomerName = table.Column<string>(type: "TEXT", nullable: false),
                    CustomerDateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: true),
                    ContactNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    Addressline = table.Column<string>(type: "TEXT", nullable: false),
                    PinCodeId = table.Column<string>(type: "TEXT", nullable: true),
                    StateId = table.Column<string>(type: "TEXT", nullable: true),
                    CountryId = table.Column<string>(type: "TEXT", nullable: true),
                    DistrictId = table.Column<string>(type: "TEXT", nullable: true),
                    CustomerType = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomerIncome = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomerOccupation = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomerEducation = table.Column<int>(type: "INTEGER", nullable: false),
                    ProfilePictureUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ProfilePicture = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDetail", x => x.CustomerDetailId);
                    table.ForeignKey(
                        name: "FK_CustomerDetail_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_CustomerDetail_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "DistrictId");
                    table.ForeignKey(
                        name: "FK_CustomerDetail_PinCode_PinCodeId",
                        column: x => x.PinCodeId,
                        principalTable: "PinCode",
                        principalColumn: "PinCodeId");
                    table.ForeignKey(
                        name: "FK_CustomerDetail_State_StateId",
                        column: x => x.StateId,
                        principalTable: "State",
                        principalColumn: "StateId");
                });

            migrationBuilder.CreateTable(
                name: "PolicyDetail",
                columns: table => new
                {
                    PolicyDetailId = table.Column<string>(type: "TEXT", nullable: false),
                    ClientCompanyId = table.Column<string>(type: "TEXT", nullable: false),
                    LineOfBusinessId = table.Column<string>(type: "TEXT", nullable: true),
                    InvestigationServiceTypeId = table.Column<string>(type: "TEXT", nullable: true),
                    ContractNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ContractIssueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<int>(type: "INTEGER", nullable: true),
                    DateOfIncident = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CauseOfLoss = table.Column<string>(type: "TEXT", nullable: false),
                    SumAssuredValue = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    CostCentreId = table.Column<string>(type: "TEXT", nullable: false),
                    CaseEnablerId = table.Column<string>(type: "TEXT", nullable: true),
                    DocumentImage = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Comments = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyDetail", x => x.PolicyDetailId);
                    table.ForeignKey(
                        name: "FK_PolicyDetail_CaseEnabler_CaseEnablerId",
                        column: x => x.CaseEnablerId,
                        principalTable: "CaseEnabler",
                        principalColumn: "CaseEnablerId");
                    table.ForeignKey(
                        name: "FK_PolicyDetail_ClientCompany_ClientCompanyId",
                        column: x => x.ClientCompanyId,
                        principalTable: "ClientCompany",
                        principalColumn: "ClientCompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PolicyDetail_CostCentre_CostCentreId",
                        column: x => x.CostCentreId,
                        principalTable: "CostCentre",
                        principalColumn: "CostCentreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PolicyDetail_InvestigationServiceType_InvestigationServiceTypeId",
                        column: x => x.InvestigationServiceTypeId,
                        principalTable: "InvestigationServiceType",
                        principalColumn: "InvestigationServiceTypeId");
                    table.ForeignKey(
                        name: "FK_PolicyDetail_LineOfBusiness_LineOfBusinessId",
                        column: x => x.LineOfBusinessId,
                        principalTable: "LineOfBusiness",
                        principalColumn: "LineOfBusinessId");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<long>(type: "INTEGER", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ApplicationUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProfilePictureUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IsSuperAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsClientAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVendorAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProfilePicture = table.Column<byte[]>(type: "BLOB", nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    PinCodeId = table.Column<string>(type: "TEXT", nullable: true),
                    StateId = table.Column<string>(type: "TEXT", nullable: true),
                    CountryId = table.Column<string>(type: "TEXT", nullable: true),
                    DistrictId = table.Column<string>(type: "TEXT", nullable: true),
                    Addressline = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    SecretPin = table.Column<string>(type: "TEXT", nullable: true),
                    MobileUId = table.Column<string>(type: "TEXT", nullable: true),
                    Discriminator = table.Column<string>(type: "TEXT", nullable: false),
                    ClientCompanyId = table.Column<string>(type: "TEXT", nullable: true),
                    Comments = table.Column<string>(type: "TEXT", nullable: true),
                    VendorId = table.Column<string>(type: "TEXT", nullable: true),
                    VendorApplicationUser_Comments = table.Column<string>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_ClientCompany_ClientCompanyId",
                        column: x => x.ClientCompanyId,
                        principalTable: "ClientCompany",
                        principalColumn: "ClientCompanyId");
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_AspNetUsers_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "DistrictId");
                    table.ForeignKey(
                        name: "FK_AspNetUsers_PinCode_PinCodeId",
                        column: x => x.PinCodeId,
                        principalTable: "PinCode",
                        principalColumn: "PinCodeId");
                    table.ForeignKey(
                        name: "FK_AspNetUsers_State_StateId",
                        column: x => x.StateId,
                        principalTable: "State",
                        principalColumn: "StateId");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mailbox",
                columns: table => new
                {
                    MailboxId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ApplicationUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mailbox", x => x.MailboxId);
                    table.ForeignKey(
                        name: "FK_Mailbox_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeletedMessage",
                columns: table => new
                {
                    DeletedMessageId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderEmail = table.Column<string>(type: "TEXT", nullable: false),
                    ReceipientEmail = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    RawMessage = table.Column<string>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Read = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    SendDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReceiveDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Attachment = table.Column<byte[]>(type: "BLOB", nullable: true),
                    AttachmentName = table.Column<string>(type: "TEXT", nullable: true),
                    FileType = table.Column<string>(type: "TEXT", nullable: true),
                    Extension = table.Column<string>(type: "TEXT", nullable: true),
                    IsDraft = table.Column<bool>(type: "INTEGER", nullable: true),
                    Trashed = table.Column<bool>(type: "INTEGER", nullable: true),
                    DeleteTrashed = table.Column<bool>(type: "INTEGER", nullable: true),
                    MessageStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    MailboxId = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedMessage", x => x.DeletedMessageId);
                    table.ForeignKey(
                        name: "FK_DeletedMessage_Mailbox_MailboxId",
                        column: x => x.MailboxId,
                        principalTable: "Mailbox",
                        principalColumn: "MailboxId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DraftMessage",
                columns: table => new
                {
                    DraftMessageId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderEmail = table.Column<string>(type: "TEXT", nullable: false),
                    ReceipientEmail = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Read = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    SendDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReceiveDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AttachmentName = table.Column<string>(type: "TEXT", nullable: true),
                    FileType = table.Column<string>(type: "TEXT", nullable: true),
                    Extension = table.Column<string>(type: "TEXT", nullable: true),
                    Attachment = table.Column<byte[]>(type: "BLOB", nullable: true),
                    IsDraft = table.Column<bool>(type: "INTEGER", nullable: true),
                    Trashed = table.Column<bool>(type: "INTEGER", nullable: true),
                    DeleteTrashed = table.Column<bool>(type: "INTEGER", nullable: true),
                    MessageStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    MailboxId = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftMessage", x => x.DraftMessageId);
                    table.ForeignKey(
                        name: "FK_DraftMessage_Mailbox_MailboxId",
                        column: x => x.MailboxId,
                        principalTable: "Mailbox",
                        principalColumn: "MailboxId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InboxMessage",
                columns: table => new
                {
                    InboxMessageId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderEmail = table.Column<string>(type: "TEXT", nullable: false),
                    ReceipientEmail = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    RawMessage = table.Column<string>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Read = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    SendDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReceiveDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Attachment = table.Column<byte[]>(type: "BLOB", nullable: true),
                    AttachmentName = table.Column<string>(type: "TEXT", nullable: true),
                    FileType = table.Column<string>(type: "TEXT", nullable: true),
                    Extension = table.Column<string>(type: "TEXT", nullable: true),
                    IsDraft = table.Column<bool>(type: "INTEGER", nullable: true),
                    Trashed = table.Column<bool>(type: "INTEGER", nullable: true),
                    DeleteTrashed = table.Column<bool>(type: "INTEGER", nullable: true),
                    MessageStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    MailboxId = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessage", x => x.InboxMessageId);
                    table.ForeignKey(
                        name: "FK_InboxMessage_Mailbox_MailboxId",
                        column: x => x.MailboxId,
                        principalTable: "Mailbox",
                        principalColumn: "MailboxId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                columns: table => new
                {
                    OutboxMessageId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderEmail = table.Column<string>(type: "TEXT", nullable: false),
                    ReceipientEmail = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    RawMessage = table.Column<string>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Read = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    SendDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReceiveDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Attachment = table.Column<byte[]>(type: "BLOB", nullable: true),
                    AttachmentName = table.Column<string>(type: "TEXT", nullable: true),
                    FileType = table.Column<string>(type: "TEXT", nullable: true),
                    Extension = table.Column<string>(type: "TEXT", nullable: true),
                    IsDraft = table.Column<bool>(type: "INTEGER", nullable: true),
                    Trashed = table.Column<bool>(type: "INTEGER", nullable: true),
                    DeleteTrashed = table.Column<bool>(type: "INTEGER", nullable: true),
                    MessageStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    MailboxId = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.OutboxMessageId);
                    table.ForeignKey(
                        name: "FK_OutboxMessage_Mailbox_MailboxId",
                        column: x => x.MailboxId,
                        principalTable: "Mailbox",
                        principalColumn: "MailboxId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SentMessage",
                columns: table => new
                {
                    SentMessageId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderEmail = table.Column<string>(type: "TEXT", nullable: false),
                    ReceipientEmail = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    RawMessage = table.Column<string>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Read = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    SendDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReceiveDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Attachment = table.Column<byte[]>(type: "BLOB", nullable: true),
                    AttachmentName = table.Column<string>(type: "TEXT", nullable: true),
                    FileType = table.Column<string>(type: "TEXT", nullable: true),
                    Extension = table.Column<string>(type: "TEXT", nullable: true),
                    IsDraft = table.Column<bool>(type: "INTEGER", nullable: true),
                    Trashed = table.Column<bool>(type: "INTEGER", nullable: true),
                    DeleteTrashed = table.Column<bool>(type: "INTEGER", nullable: true),
                    MessageStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    MailboxId = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentMessage", x => x.SentMessageId);
                    table.ForeignKey(
                        name: "FK_SentMessage_Mailbox_MailboxId",
                        column: x => x.MailboxId,
                        principalTable: "Mailbox",
                        principalColumn: "MailboxId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrashMessage",
                columns: table => new
                {
                    TrashMessageId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderEmail = table.Column<string>(type: "TEXT", nullable: false),
                    ReceipientEmail = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    RawMessage = table.Column<string>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Read = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    SendDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReceiveDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Attachment = table.Column<byte[]>(type: "BLOB", nullable: true),
                    AttachmentName = table.Column<string>(type: "TEXT", nullable: true),
                    FileType = table.Column<string>(type: "TEXT", nullable: true),
                    Extension = table.Column<string>(type: "TEXT", nullable: true),
                    IsDraft = table.Column<bool>(type: "INTEGER", nullable: true),
                    Trashed = table.Column<bool>(type: "INTEGER", nullable: true),
                    DeleteTrashed = table.Column<bool>(type: "INTEGER", nullable: true),
                    MessageStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    MailboxId = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrashMessage", x => x.TrashMessageId);
                    table.ForeignKey(
                        name: "FK_TrashMessage_Mailbox_MailboxId",
                        column: x => x.MailboxId,
                        principalTable: "Mailbox",
                        principalColumn: "MailboxId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseLocation",
                columns: table => new
                {
                    CaseLocationId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BeneficiaryName = table.Column<string>(type: "TEXT", nullable: false),
                    BeneficiaryRelationId = table.Column<long>(type: "INTEGER", nullable: false),
                    BeneficiaryContactNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    BeneficiaryIncome = table.Column<int>(type: "INTEGER", nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ProfilePicture = table.Column<byte[]>(type: "BLOB", nullable: true),
                    BeneficiaryDateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CountryId = table.Column<string>(type: "TEXT", nullable: true),
                    StateId = table.Column<string>(type: "TEXT", nullable: true),
                    DistrictId = table.Column<string>(type: "TEXT", nullable: true),
                    PinCodeId = table.Column<string>(type: "TEXT", nullable: true),
                    Addressline = table.Column<string>(type: "TEXT", nullable: false),
                    Addressline2 = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimsInvestigationId = table.Column<string>(type: "TEXT", nullable: false),
                    VendorId = table.Column<string>(type: "TEXT", nullable: true),
                    InvestigationCaseSubStatusId = table.Column<string>(type: "TEXT", nullable: true),
                    AssignedAgentUserEmail = table.Column<string>(type: "TEXT", nullable: true),
                    IsReviewCaseLocation = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseLocation", x => x.CaseLocationId);
                    table.ForeignKey(
                        name: "FK_CaseLocation_BeneficiaryRelation_BeneficiaryRelationId",
                        column: x => x.BeneficiaryRelationId,
                        principalTable: "BeneficiaryRelation",
                        principalColumn: "BeneficiaryRelationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseLocation_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_CaseLocation_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "DistrictId");
                    table.ForeignKey(
                        name: "FK_CaseLocation_InvestigationCaseSubStatus_InvestigationCaseSubStatusId",
                        column: x => x.InvestigationCaseSubStatusId,
                        principalTable: "InvestigationCaseSubStatus",
                        principalColumn: "InvestigationCaseSubStatusId");
                    table.ForeignKey(
                        name: "FK_CaseLocation_PinCode_PinCodeId",
                        column: x => x.PinCodeId,
                        principalTable: "PinCode",
                        principalColumn: "PinCodeId");
                    table.ForeignKey(
                        name: "FK_CaseLocation_State_StateId",
                        column: x => x.StateId,
                        principalTable: "State",
                        principalColumn: "StateId");
                });

            migrationBuilder.CreateTable(
                name: "VerifyPinCode",
                columns: table => new
                {
                    VerifyPinCodeId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Pincode = table.Column<string>(type: "TEXT", nullable: false),
                    CaseLocationId = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerifyPinCode", x => x.VerifyPinCodeId);
                    table.ForeignKey(
                        name: "FK_VerifyPinCode_CaseLocation_CaseLocationId",
                        column: x => x.CaseLocationId,
                        principalTable: "CaseLocation",
                        principalColumn: "CaseLocationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClaimMessage",
                columns: table => new
                {
                    ClaimMessageId = table.Column<string>(type: "TEXT", nullable: false),
                    SenderEmail = table.Column<string>(type: "TEXT", nullable: true),
                    RecepicientEmail = table.Column<string>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimsInvestigationId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimMessage", x => x.ClaimMessageId);
                });

            migrationBuilder.CreateTable(
                name: "ClaimNote",
                columns: table => new
                {
                    ClaimNoteId = table.Column<string>(type: "TEXT", nullable: false),
                    Sender = table.Column<string>(type: "TEXT", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: false),
                    ParentClaimNoteClaimNoteId = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimsInvestigationId = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimNote", x => x.ClaimNoteId);
                    table.ForeignKey(
                        name: "FK_ClaimNote_ClaimNote_ParentClaimNoteClaimNoteId",
                        column: x => x.ParentClaimNoteClaimNoteId,
                        principalTable: "ClaimNote",
                        principalColumn: "ClaimNoteId");
                });

            migrationBuilder.CreateTable(
                name: "ClaimReport",
                columns: table => new
                {
                    ClaimReportId = table.Column<string>(type: "TEXT", nullable: false),
                    VendorId = table.Column<string>(type: "TEXT", nullable: true),
                    AgentEmail = table.Column<string>(type: "TEXT", nullable: true),
                    AgentRemarksUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AgentRemarks = table.Column<string>(type: "TEXT", nullable: true),
                    Question1 = table.Column<string>(type: "TEXT", nullable: true),
                    Question2 = table.Column<string>(type: "TEXT", nullable: true),
                    Question3 = table.Column<string>(type: "TEXT", nullable: true),
                    Question4 = table.Column<string>(type: "TEXT", nullable: true),
                    Question5 = table.Column<string>(type: "TEXT", nullable: true),
                    AgentLocationPictureUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AgentLocationPicture = table.Column<byte[]>(type: "BLOB", nullable: true),
                    LocationData = table.Column<string>(type: "TEXT", nullable: true),
                    LocationPictureConfidence = table.Column<string>(type: "TEXT", nullable: true),
                    AgentOcrUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AgentOcrPicture = table.Column<byte[]>(type: "BLOB", nullable: true),
                    PanValid = table.Column<bool>(type: "INTEGER", nullable: true),
                    ImageType = table.Column<string>(type: "TEXT", nullable: true),
                    AgentOcrData = table.Column<string>(type: "TEXT", nullable: true),
                    AgentQrUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AgentQrPicture = table.Column<byte[]>(type: "BLOB", nullable: true),
                    QrData = table.Column<string>(type: "TEXT", nullable: true),
                    LocationLongLat = table.Column<string>(type: "TEXT", nullable: true),
                    LocationLongLatTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OcrLongLat = table.Column<string>(type: "TEXT", nullable: true),
                    OcrLongLatTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AgentReportId = table.Column<string>(type: "TEXT", nullable: true),
                    SupervisorPicture = table.Column<byte[]>(type: "BLOB", nullable: true),
                    SupervisorRemarksUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SupervisorEmail = table.Column<string>(type: "TEXT", nullable: true),
                    SupervisorRemarks = table.Column<string>(type: "TEXT", nullable: true),
                    SupervisorRemarkType = table.Column<int>(type: "INTEGER", nullable: true),
                    AssessorRemarksUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssessorEmail = table.Column<string>(type: "TEXT", nullable: true),
                    AssessorRemarks = table.Column<string>(type: "TEXT", nullable: true),
                    AssessorRemarkType = table.Column<int>(type: "INTEGER", nullable: true),
                    CaseLocationId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimReport", x => x.ClaimReportId);
                    table.ForeignKey(
                        name: "FK_ClaimReport_AgentReport_AgentReportId",
                        column: x => x.AgentReportId,
                        principalTable: "AgentReport",
                        principalColumn: "AgentReportId");
                    table.ForeignKey(
                        name: "FK_ClaimReport_CaseLocation_CaseLocationId",
                        column: x => x.CaseLocationId,
                        principalTable: "CaseLocation",
                        principalColumn: "CaseLocationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClaimsInvestigation",
                columns: table => new
                {
                    ClaimsInvestigationId = table.Column<string>(type: "TEXT", nullable: false),
                    VendorId = table.Column<string>(type: "TEXT", nullable: true),
                    PolicyDetailId = table.Column<string>(type: "TEXT", nullable: true),
                    CustomerDetailId = table.Column<string>(type: "TEXT", nullable: true),
                    InvestigationCaseStatusId = table.Column<string>(type: "TEXT", nullable: true),
                    InvestigationCaseSubStatusId = table.Column<string>(type: "TEXT", nullable: true),
                    CurrentUserEmail = table.Column<string>(type: "TEXT", nullable: true),
                    CurrentClaimOwner = table.Column<string>(type: "TEXT", nullable: true),
                    IsReviewCase = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsReady2Assign = table.Column<bool>(type: "INTEGER", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClientCompanyId = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimsInvestigation", x => x.ClaimsInvestigationId);
                    table.ForeignKey(
                        name: "FK_ClaimsInvestigation_ClientCompany_ClientCompanyId",
                        column: x => x.ClientCompanyId,
                        principalTable: "ClientCompany",
                        principalColumn: "ClientCompanyId");
                    table.ForeignKey(
                        name: "FK_ClaimsInvestigation_CustomerDetail_CustomerDetailId",
                        column: x => x.CustomerDetailId,
                        principalTable: "CustomerDetail",
                        principalColumn: "CustomerDetailId");
                    table.ForeignKey(
                        name: "FK_ClaimsInvestigation_InvestigationCaseStatus_InvestigationCaseStatusId",
                        column: x => x.InvestigationCaseStatusId,
                        principalTable: "InvestigationCaseStatus",
                        principalColumn: "InvestigationCaseStatusId");
                    table.ForeignKey(
                        name: "FK_ClaimsInvestigation_InvestigationCaseSubStatus_InvestigationCaseSubStatusId",
                        column: x => x.InvestigationCaseSubStatusId,
                        principalTable: "InvestigationCaseSubStatus",
                        principalColumn: "InvestigationCaseSubStatusId");
                    table.ForeignKey(
                        name: "FK_ClaimsInvestigation_PolicyDetail_PolicyDetailId",
                        column: x => x.PolicyDetailId,
                        principalTable: "PolicyDetail",
                        principalColumn: "PolicyDetailId");
                });

            migrationBuilder.CreateTable(
                name: "FileAttachment",
                columns: table => new
                {
                    FileAttachmentId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AttachedDocument = table.Column<byte[]>(type: "BLOB", nullable: true),
                    ContactMessageId = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimsInvestigationId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileAttachment", x => x.FileAttachmentId);
                    table.ForeignKey(
                        name: "FK_FileAttachment_ClaimsInvestigation_ClaimsInvestigationId",
                        column: x => x.ClaimsInvestigationId,
                        principalTable: "ClaimsInvestigation",
                        principalColumn: "ClaimsInvestigationId");
                });

            migrationBuilder.CreateTable(
                name: "InvestigationTransaction",
                columns: table => new
                {
                    InvestigationTransactionId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimsInvestigationId = table.Column<string>(type: "TEXT", nullable: true),
                    InvestigationCaseStatusId = table.Column<string>(type: "TEXT", nullable: true),
                    InvestigationCaseSubStatusId = table.Column<string>(type: "TEXT", nullable: true),
                    Time2Update = table.Column<int>(type: "INTEGER", nullable: true),
                    HopCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Sender = table.Column<string>(type: "TEXT", nullable: true),
                    Receiver = table.Column<string>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    headerIcon = table.Column<string>(type: "TEXT", nullable: true),
                    headerMessage = table.Column<string>(type: "TEXT", nullable: true),
                    messageIcon = table.Column<string>(type: "TEXT", nullable: true),
                    footerIcon = table.Column<string>(type: "TEXT", nullable: true),
                    footerMessage = table.Column<string>(type: "TEXT", nullable: true),
                    CurrentClaimOwner = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationTransaction", x => x.InvestigationTransactionId);
                    table.ForeignKey(
                        name: "FK_InvestigationTransaction_ClaimsInvestigation_ClaimsInvestigationId",
                        column: x => x.ClaimsInvestigationId,
                        principalTable: "ClaimsInvestigation",
                        principalColumn: "ClaimsInvestigationId");
                    table.ForeignKey(
                        name: "FK_InvestigationTransaction_InvestigationCaseStatus_InvestigationCaseStatusId",
                        column: x => x.InvestigationCaseStatusId,
                        principalTable: "InvestigationCaseStatus",
                        principalColumn: "InvestigationCaseStatusId");
                    table.ForeignKey(
                        name: "FK_InvestigationTransaction_InvestigationCaseSubStatus_InvestigationCaseSubStatusId",
                        column: x => x.InvestigationCaseSubStatusId,
                        principalTable: "InvestigationCaseSubStatus",
                        principalColumn: "InvestigationCaseSubStatusId");
                });

            migrationBuilder.CreateTable(
                name: "Vendor",
                columns: table => new
                {
                    VendorId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Branch = table.Column<string>(type: "TEXT", nullable: false),
                    Addressline = table.Column<string>(type: "TEXT", nullable: false),
                    StateId = table.Column<string>(type: "TEXT", nullable: true),
                    CountryId = table.Column<string>(type: "TEXT", nullable: true),
                    PinCodeId = table.Column<string>(type: "TEXT", nullable: true),
                    DistrictId = table.Column<string>(type: "TEXT", nullable: true),
                    BankName = table.Column<string>(type: "TEXT", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "TEXT", nullable: false),
                    IFSCCode = table.Column<string>(type: "TEXT", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: false),
                    AgreementDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActivatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeListedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: true),
                    DomainName = table.Column<int>(type: "INTEGER", nullable: true),
                    DelistReason = table.Column<string>(type: "TEXT", nullable: true),
                    DocumentUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DocumentImage = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClaimsInvestigationId = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendor", x => x.VendorId);
                    table.ForeignKey(
                        name: "FK_Vendor_ClaimsInvestigation_ClaimsInvestigationId",
                        column: x => x.ClaimsInvestigationId,
                        principalTable: "ClaimsInvestigation",
                        principalColumn: "ClaimsInvestigationId");
                    table.ForeignKey(
                        name: "FK_Vendor_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_Vendor_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "DistrictId");
                    table.ForeignKey(
                        name: "FK_Vendor_PinCode_PinCodeId",
                        column: x => x.PinCodeId,
                        principalTable: "PinCode",
                        principalColumn: "PinCodeId");
                    table.ForeignKey(
                        name: "FK_Vendor_State_StateId",
                        column: x => x.StateId,
                        principalTable: "State",
                        principalColumn: "StateId");
                });

            migrationBuilder.CreateTable(
                name: "VerificationLocation",
                columns: table => new
                {
                    VerificationLocationId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimsInvestigationId = table.Column<string>(type: "TEXT", nullable: false),
                    Addressline = table.Column<string>(type: "TEXT", nullable: true),
                    PinCodeId = table.Column<string>(type: "TEXT", nullable: true),
                    StateId = table.Column<string>(type: "TEXT", nullable: true),
                    CountryId = table.Column<string>(type: "TEXT", nullable: false),
                    DistrictId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationLocation", x => x.VerificationLocationId);
                    table.ForeignKey(
                        name: "FK_VerificationLocation_ClaimsInvestigation_ClaimsInvestigationId",
                        column: x => x.ClaimsInvestigationId,
                        principalTable: "ClaimsInvestigation",
                        principalColumn: "ClaimsInvestigationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VerificationLocation_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VerificationLocation_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "DistrictId");
                    table.ForeignKey(
                        name: "FK_VerificationLocation_PinCode_PinCodeId",
                        column: x => x.PinCodeId,
                        principalTable: "PinCode",
                        principalColumn: "PinCodeId");
                    table.ForeignKey(
                        name: "FK_VerificationLocation_State_StateId",
                        column: x => x.StateId,
                        principalTable: "State",
                        principalColumn: "StateId");
                });

            migrationBuilder.CreateTable(
                name: "ClientCompanyVendor",
                columns: table => new
                {
                    ClientsClientCompanyId = table.Column<string>(type: "TEXT", nullable: false),
                    EmpanelledVendorsVendorId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCompanyVendor", x => new { x.ClientsClientCompanyId, x.EmpanelledVendorsVendorId });
                    table.ForeignKey(
                        name: "FK_ClientCompanyVendor_ClientCompany_ClientsClientCompanyId",
                        column: x => x.ClientsClientCompanyId,
                        principalTable: "ClientCompany",
                        principalColumn: "ClientCompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientCompanyVendor_Vendor_EmpanelledVendorsVendorId",
                        column: x => x.EmpanelledVendorsVendorId,
                        principalTable: "Vendor",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorInvestigationServiceType",
                columns: table => new
                {
                    VendorInvestigationServiceTypeId = table.Column<string>(type: "TEXT", nullable: false),
                    InvestigationServiceTypeId = table.Column<string>(type: "TEXT", nullable: false),
                    LineOfBusinessId = table.Column<string>(type: "TEXT", nullable: true),
                    CountryId = table.Column<string>(type: "TEXT", nullable: true),
                    StateId = table.Column<string>(type: "TEXT", nullable: true),
                    DistrictId = table.Column<string>(type: "TEXT", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    VendorId = table.Column<string>(type: "TEXT", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorInvestigationServiceType", x => x.VendorInvestigationServiceTypeId);
                    table.ForeignKey(
                        name: "FK_VendorInvestigationServiceType_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_VendorInvestigationServiceType_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "DistrictId");
                    table.ForeignKey(
                        name: "FK_VendorInvestigationServiceType_InvestigationServiceType_InvestigationServiceTypeId",
                        column: x => x.InvestigationServiceTypeId,
                        principalTable: "InvestigationServiceType",
                        principalColumn: "InvestigationServiceTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendorInvestigationServiceType_LineOfBusiness_LineOfBusinessId",
                        column: x => x.LineOfBusinessId,
                        principalTable: "LineOfBusiness",
                        principalColumn: "LineOfBusinessId");
                    table.ForeignKey(
                        name: "FK_VendorInvestigationServiceType_State_StateId",
                        column: x => x.StateId,
                        principalTable: "State",
                        principalColumn: "StateId");
                    table.ForeignKey(
                        name: "FK_VendorInvestigationServiceType_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServicedPinCode",
                columns: table => new
                {
                    ServicedPinCodeId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Pincode = table.Column<string>(type: "TEXT", nullable: false),
                    VendorInvestigationServiceTypeId = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicedPinCode", x => x.ServicedPinCodeId);
                    table.ForeignKey(
                        name: "FK_ServicedPinCode_VendorInvestigationServiceType_VendorInvestigationServiceTypeId",
                        column: x => x.VendorInvestigationServiceTypeId,
                        principalTable: "VendorInvestigationServiceType",
                        principalColumn: "VendorInvestigationServiceTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_ApplicationUserId",
                table: "AspNetRoles",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClientCompanyId",
                table: "AspNetUsers",
                column: "ClientCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CountryId",
                table: "AspNetUsers",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DistrictId",
                table: "AspNetUsers",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PinCodeId",
                table: "AspNetUsers",
                column: "PinCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_StateId",
                table: "AspNetUsers",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_VendorId",
                table: "AspNetUsers",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseLocation_BeneficiaryRelationId",
                table: "CaseLocation",
                column: "BeneficiaryRelationId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseLocation_ClaimsInvestigationId",
                table: "CaseLocation",
                column: "ClaimsInvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseLocation_CountryId",
                table: "CaseLocation",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseLocation_DistrictId",
                table: "CaseLocation",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseLocation_InvestigationCaseSubStatusId",
                table: "CaseLocation",
                column: "InvestigationCaseSubStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseLocation_PinCodeId",
                table: "CaseLocation",
                column: "PinCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseLocation_StateId",
                table: "CaseLocation",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseLocation_VendorId",
                table: "CaseLocation",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimMessage_ClaimsInvestigationId",
                table: "ClaimMessage",
                column: "ClaimsInvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimNote_ClaimsInvestigationId",
                table: "ClaimNote",
                column: "ClaimsInvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimNote_ParentClaimNoteClaimNoteId",
                table: "ClaimNote",
                column: "ParentClaimNoteClaimNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimReport_AgentReportId",
                table: "ClaimReport",
                column: "AgentReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimReport_CaseLocationId",
                table: "ClaimReport",
                column: "CaseLocationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClaimReport_VendorId",
                table: "ClaimReport",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimsInvestigation_ClientCompanyId",
                table: "ClaimsInvestigation",
                column: "ClientCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimsInvestigation_CustomerDetailId",
                table: "ClaimsInvestigation",
                column: "CustomerDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimsInvestigation_InvestigationCaseStatusId",
                table: "ClaimsInvestigation",
                column: "InvestigationCaseStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimsInvestigation_InvestigationCaseSubStatusId",
                table: "ClaimsInvestigation",
                column: "InvestigationCaseSubStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimsInvestigation_PolicyDetailId",
                table: "ClaimsInvestigation",
                column: "PolicyDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimsInvestigation_VendorId",
                table: "ClaimsInvestigation",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCompany_CountryId",
                table: "ClientCompany",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCompany_DistrictId",
                table: "ClientCompany",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCompany_PinCodeId",
                table: "ClientCompany",
                column: "PinCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCompany_StateId",
                table: "ClientCompany",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCompanyVendor_EmpanelledVendorsVendorId",
                table: "ClientCompanyVendor",
                column: "EmpanelledVendorsVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDetail_CountryId",
                table: "CustomerDetail",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDetail_DistrictId",
                table: "CustomerDetail",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDetail_PinCodeId",
                table: "CustomerDetail",
                column: "PinCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDetail_StateId",
                table: "CustomerDetail",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedMessage_MailboxId",
                table: "DeletedMessage",
                column: "MailboxId");

            migrationBuilder.CreateIndex(
                name: "IX_District_CountryId",
                table: "District",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_District_StateId",
                table: "District",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftMessage_MailboxId",
                table: "DraftMessage",
                column: "MailboxId");

            migrationBuilder.CreateIndex(
                name: "IX_FileAttachment_ClaimsInvestigationId",
                table: "FileAttachment",
                column: "ClaimsInvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_MailboxId",
                table: "InboxMessage",
                column: "MailboxId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationCase_InvestigationCaseStatusId",
                table: "InvestigationCase",
                column: "InvestigationCaseStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationCase_InvestigationServiceTypeId",
                table: "InvestigationCase",
                column: "InvestigationServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationCase_LineOfBusinessId",
                table: "InvestigationCase",
                column: "LineOfBusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationCaseSubStatus_InvestigationCaseStatusId",
                table: "InvestigationCaseSubStatus",
                column: "InvestigationCaseStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationServiceType_LineOfBusinessId",
                table: "InvestigationServiceType",
                column: "LineOfBusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationTransaction_ClaimsInvestigationId",
                table: "InvestigationTransaction",
                column: "ClaimsInvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationTransaction_InvestigationCaseStatusId",
                table: "InvestigationTransaction",
                column: "InvestigationCaseStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationTransaction_InvestigationCaseSubStatusId",
                table: "InvestigationTransaction",
                column: "InvestigationCaseSubStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Mailbox_ApplicationUserId",
                table: "Mailbox",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_MailboxId",
                table: "OutboxMessage",
                column: "MailboxId");

            migrationBuilder.CreateIndex(
                name: "IX_PinCode_CountryId",
                table: "PinCode",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_PinCode_DistrictId",
                table: "PinCode",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_PinCode_StateId",
                table: "PinCode",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyDetail_CaseEnablerId",
                table: "PolicyDetail",
                column: "CaseEnablerId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyDetail_ClientCompanyId",
                table: "PolicyDetail",
                column: "ClientCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyDetail_CostCentreId",
                table: "PolicyDetail",
                column: "CostCentreId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyDetail_InvestigationServiceTypeId",
                table: "PolicyDetail",
                column: "InvestigationServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyDetail_LineOfBusinessId",
                table: "PolicyDetail",
                column: "LineOfBusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_SentMessage_MailboxId",
                table: "SentMessage",
                column: "MailboxId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicedPinCode_VendorInvestigationServiceTypeId",
                table: "ServicedPinCode",
                column: "VendorInvestigationServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_State_CountryId",
                table: "State",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_TrashMessage_MailboxId",
                table: "TrashMessage",
                column: "MailboxId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_ClaimsInvestigationId",
                table: "Vendor",
                column: "ClaimsInvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_CountryId",
                table: "Vendor",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_DistrictId",
                table: "Vendor",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_PinCodeId",
                table: "Vendor",
                column: "PinCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_StateId",
                table: "Vendor",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvestigationServiceType_CountryId",
                table: "VendorInvestigationServiceType",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvestigationServiceType_DistrictId",
                table: "VendorInvestigationServiceType",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvestigationServiceType_InvestigationServiceTypeId",
                table: "VendorInvestigationServiceType",
                column: "InvestigationServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvestigationServiceType_LineOfBusinessId",
                table: "VendorInvestigationServiceType",
                column: "LineOfBusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvestigationServiceType_StateId",
                table: "VendorInvestigationServiceType",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvestigationServiceType_VendorId",
                table: "VendorInvestigationServiceType",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationLocation_ClaimsInvestigationId",
                table: "VerificationLocation",
                column: "ClaimsInvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationLocation_CountryId",
                table: "VerificationLocation",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationLocation_DistrictId",
                table: "VerificationLocation",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationLocation_PinCodeId",
                table: "VerificationLocation",
                column: "PinCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationLocation_StateId",
                table: "VerificationLocation",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_VerifyPinCode_CaseLocationId",
                table: "VerifyPinCode",
                column: "CaseLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_AspNetUsers_ApplicationUserId",
                table: "AspNetRoles",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Vendor_VendorId",
                table: "AspNetUsers",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseLocation_ClaimsInvestigation_ClaimsInvestigationId",
                table: "CaseLocation",
                column: "ClaimsInvestigationId",
                principalTable: "ClaimsInvestigation",
                principalColumn: "ClaimsInvestigationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseLocation_Vendor_VendorId",
                table: "CaseLocation",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimMessage_ClaimsInvestigation_ClaimsInvestigationId",
                table: "ClaimMessage",
                column: "ClaimsInvestigationId",
                principalTable: "ClaimsInvestigation",
                principalColumn: "ClaimsInvestigationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimNote_ClaimsInvestigation_ClaimsInvestigationId",
                table: "ClaimNote",
                column: "ClaimsInvestigationId",
                principalTable: "ClaimsInvestigation",
                principalColumn: "ClaimsInvestigationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimReport_Vendor_VendorId",
                table: "ClaimReport",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimsInvestigation_Vendor_VendorId",
                table: "ClaimsInvestigation",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClaimsInvestigation_ClientCompany_ClientCompanyId",
                table: "ClaimsInvestigation");

            migrationBuilder.DropForeignKey(
                name: "FK_PolicyDetail_ClientCompany_ClientCompanyId",
                table: "PolicyDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerDetail_Country_CountryId",
                table: "CustomerDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_District_Country_CountryId",
                table: "District");

            migrationBuilder.DropForeignKey(
                name: "FK_PinCode_Country_CountryId",
                table: "PinCode");

            migrationBuilder.DropForeignKey(
                name: "FK_State_Country_CountryId",
                table: "State");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendor_Country_CountryId",
                table: "Vendor");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerDetail_District_DistrictId",
                table: "CustomerDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PinCode_District_DistrictId",
                table: "PinCode");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendor_District_DistrictId",
                table: "Vendor");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerDetail_PinCode_PinCodeId",
                table: "CustomerDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendor_PinCode_PinCodeId",
                table: "Vendor");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerDetail_State_StateId",
                table: "CustomerDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendor_State_StateId",
                table: "Vendor");

            migrationBuilder.DropForeignKey(
                name: "FK_ClaimsInvestigation_Vendor_VendorId",
                table: "ClaimsInvestigation");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ClaimMessage");

            migrationBuilder.DropTable(
                name: "ClaimNote");

            migrationBuilder.DropTable(
                name: "ClaimReport");

            migrationBuilder.DropTable(
                name: "ClientCompanyVendor");

            migrationBuilder.DropTable(
                name: "DeletedMessage");

            migrationBuilder.DropTable(
                name: "DraftMessage");

            migrationBuilder.DropTable(
                name: "FileAttachment");

            migrationBuilder.DropTable(
                name: "FilesOnDatabase");

            migrationBuilder.DropTable(
                name: "FilesOnFileSystem");

            migrationBuilder.DropTable(
                name: "InboxMessage");

            migrationBuilder.DropTable(
                name: "InvestigationCase");

            migrationBuilder.DropTable(
                name: "InvestigationCaseOutcome");

            migrationBuilder.DropTable(
                name: "InvestigationTransaction");

            migrationBuilder.DropTable(
                name: "OutboxMessage");

            migrationBuilder.DropTable(
                name: "SentMessage");

            migrationBuilder.DropTable(
                name: "ServicedPinCode");

            migrationBuilder.DropTable(
                name: "TrashMessage");

            migrationBuilder.DropTable(
                name: "UploadClaim");

            migrationBuilder.DropTable(
                name: "VerificationLocation");

            migrationBuilder.DropTable(
                name: "VerifyPinCode");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AgentReport");

            migrationBuilder.DropTable(
                name: "VendorInvestigationServiceType");

            migrationBuilder.DropTable(
                name: "Mailbox");

            migrationBuilder.DropTable(
                name: "CaseLocation");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "BeneficiaryRelation");

            migrationBuilder.DropTable(
                name: "ClientCompany");

            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.DropTable(
                name: "District");

            migrationBuilder.DropTable(
                name: "PinCode");

            migrationBuilder.DropTable(
                name: "State");

            migrationBuilder.DropTable(
                name: "Vendor");

            migrationBuilder.DropTable(
                name: "ClaimsInvestigation");

            migrationBuilder.DropTable(
                name: "CustomerDetail");

            migrationBuilder.DropTable(
                name: "InvestigationCaseSubStatus");

            migrationBuilder.DropTable(
                name: "PolicyDetail");

            migrationBuilder.DropTable(
                name: "InvestigationCaseStatus");

            migrationBuilder.DropTable(
                name: "CaseEnabler");

            migrationBuilder.DropTable(
                name: "CostCentre");

            migrationBuilder.DropTable(
                name: "InvestigationServiceType");

            migrationBuilder.DropTable(
                name: "LineOfBusiness");
        }
    }
}
