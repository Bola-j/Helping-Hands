using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpingHands.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialDemo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "ben_reference_seq");

            migrationBuilder.CreateSequence(
                name: "out_reference_seq");

            migrationBuilder.CreateSequence(
                name: "rec_reference_seq");

            migrationBuilder.CreateSequence(
                name: "ref_reference_seq");

            migrationBuilder.CreateSequence(
                name: "req_reference_seq");

            migrationBuilder.CreateTable(
                name: "ReferralRecipient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RecipientType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactDetails = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralRecipient", x => x.Id);
                    table.CheckConstraint("CK_Recipient_Active", "NOT \"Active\" OR \"ApprovalStatus\" = 'Approved'");
                    table.CheckConstraint("CK_Recipient_Approval", "\"ApprovalStatus\" IN ('Pending', 'Approved', 'Rejected')");
                    table.CheckConstraint("CK_Recipient_Type", "\"RecipientType\" IN ('Charity', 'Contributor')");
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AccountStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccount", x => x.Id);
                    table.CheckConstraint("CK_UserAccount_Status", "\"AccountStatus\" IN ('Pending', 'Active', 'Disabled')");
                });

            migrationBuilder.CreateTable(
                name: "Charity",
                columns: table => new
                {
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationInformation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charity", x => x.RecipientId);
                    table.ForeignKey(
                        name: "FK_Charity_ReferralRecipient_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "ReferralRecipient",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApplicationAttachment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OriginalName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MediaType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationAttachment", x => x.Id);
                    table.CheckConstraint("CK_ApplicationAttachment_Size", "\"SizeBytes\" > 0");
                    table.ForeignKey(
                        name: "FK_ApplicationAttachment_ReferralRecipient_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "ReferralRecipient",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationAttachment_UserAccount_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "UserAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Beneficiary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LinkedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactDetails = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsFamily = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beneficiary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Beneficiary_UserAccount_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "UserAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Beneficiary_UserAccount_LinkedUserId",
                        column: x => x.LinkedUserId,
                        principalTable: "UserAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RecipientDecision",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipientDecision", x => x.Id);
                    table.CheckConstraint("CK_RecipientDecision_Decision", "\"Decision\" IN ('Pending', 'Approved', 'Rejected')");
                    table.ForeignKey(
                        name: "FK_RecipientDecision_ReferralRecipient_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "ReferralRecipient",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RecipientDecision_UserAccount_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "UserAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RecipientMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipientMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipientMember_ReferralRecipient_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "ReferralRecipient",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RecipientMember_UserAccount_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserRole_UserAccount_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssistanceRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BeneficiaryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ServiceSubtype = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AuthorityConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    SharingAgreed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistanceRequest", x => x.Id);
                    table.CheckConstraint("CK_Request_Category", "\"Category\" IS NULL OR \"Category\" IN ('Goods', 'Money', 'OrganizationalReferral')");
                    table.CheckConstraint("CK_Request_Confirmed", "\"AuthorityConfirmed\" AND \"SharingAgreed\"");
                    table.CheckConstraint("CK_Request_Quantity", "\"RequiredQuantity\" IS NULL OR \"RequiredQuantity\" > 0");
                    table.CheckConstraint("CK_Request_Status", "\"Status\" IN ('Submitted', 'Clarification', 'Accepted', 'Rejected', 'Open', 'Completed', 'ClosedUnresolved', 'Split')");
                    table.ForeignKey(
                        name: "FK_AssistanceRequest_Beneficiary_BeneficiaryId",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiary",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssistanceRequest_UserAccount_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "UserAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Referral",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedQuantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    ScopeDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referral", x => x.Id);
                    table.CheckConstraint("CK_Referral_Quantity", "\"AssignedQuantity\" > 0");
                    table.CheckConstraint("CK_Referral_Status", "\"Status\" IN ('Assigned', 'PartiallyFulfilled', 'Completed', 'Released', 'Declined', 'Unresolved')");
                    table.ForeignKey(
                        name: "FK_Referral_AssistanceRequest_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AssistanceRequest",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Referral_ReferralRecipient_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "ReferralRecipient",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Referral_UserAccount_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "UserAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DecisionHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferralId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionHistory", x => x.Id);
                    table.CheckConstraint("CK_DecisionHistory_Subject", "(\"RequestId\" IS NOT NULL) <> (\"ReferralId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_DecisionHistory_AssistanceRequest_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AssistanceRequest",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DecisionHistory_Referral_ReferralId",
                        column: x => x.ReferralId,
                        principalTable: "Referral",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DecisionHistory_UserAccount_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "UserAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Outcome",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReferralId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedQuantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outcome", x => x.Id);
                    table.CheckConstraint("CK_Outcome_Quantity", "\"ReportedQuantity\" > 0");
                    table.CheckConstraint("CK_Outcome_Status", "\"Status\" IN ('Draft', 'Submitted', 'Clarification', 'Accepted', 'Unresolved')");
                    table.ForeignKey(
                        name: "FK_Outcome_Referral_ReferralId",
                        column: x => x.ReferralId,
                        principalTable: "Referral",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Outcome_UserAccount_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalTable: "UserAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EvidenceAttachment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OutcomeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OriginalName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MediaType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenceAttachment", x => x.Id);
                    table.CheckConstraint("CK_EvidenceAttachment_Size", "\"SizeBytes\" > 0");
                    table.ForeignKey(
                        name: "FK_EvidenceAttachment_Outcome_OutcomeId",
                        column: x => x.OutcomeId,
                        principalTable: "Outcome",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EvidenceAttachment_UserAccount_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "UserAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OutcomeReview",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OutcomeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutcomeReview", x => x.Id);
                    table.CheckConstraint("CK_OutcomeReview_Decision", "\"Decision\" IN ('Accepted', 'Clarification', 'Unresolved')");
                    table.ForeignKey(
                        name: "FK_OutcomeReview_Outcome_OutcomeId",
                        column: x => x.OutcomeId,
                        principalTable: "Outcome",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OutcomeReview_UserAccount_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "UserAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAttachment_RecipientId",
                table: "ApplicationAttachment",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAttachment_StorageKey",
                table: "ApplicationAttachment",
                column: "StorageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAttachment_UploadedByUserId",
                table: "ApplicationAttachment",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceRequest_BeneficiaryId_CreatedAt",
                table: "AssistanceRequest",
                columns: new[] { "BeneficiaryId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceRequest_Reference",
                table: "AssistanceRequest",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceRequest_Status_CreatedAt",
                table: "AssistanceRequest",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceRequest_SubmittedByUserId_CreatedAt",
                table: "AssistanceRequest",
                columns: new[] { "SubmittedByUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiary_CreatedByUserId",
                table: "Beneficiary",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiary_LinkedUserId",
                table: "Beneficiary",
                column: "LinkedUserId",
                unique: true,
                filter: "\"LinkedUserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiary_Reference",
                table: "Beneficiary",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DecisionHistory_ActorUserId",
                table: "DecisionHistory",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionHistory_ReferralId_OccurredAt",
                table: "DecisionHistory",
                columns: new[] { "ReferralId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DecisionHistory_RequestId_OccurredAt",
                table: "DecisionHistory",
                columns: new[] { "RequestId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceAttachment_OutcomeId",
                table: "EvidenceAttachment",
                column: "OutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceAttachment_StorageKey",
                table: "EvidenceAttachment",
                column: "StorageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceAttachment_UploadedByUserId",
                table: "EvidenceAttachment",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Outcome_Reference",
                table: "Outcome",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Outcome_ReferralId_Status",
                table: "Outcome",
                columns: new[] { "ReferralId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Outcome_ReportedByUserId",
                table: "Outcome",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OutcomeReview_ActorUserId",
                table: "OutcomeReview",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OutcomeReview_OutcomeId_ReviewedAt",
                table: "OutcomeReview",
                columns: new[] { "OutcomeId", "ReviewedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RecipientDecision_ActorUserId",
                table: "RecipientDecision",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipientDecision_RecipientId_DecidedAt",
                table: "RecipientDecision",
                columns: new[] { "RecipientId", "DecidedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RecipientMember_RecipientId_UserId",
                table: "RecipientMember",
                columns: new[] { "RecipientId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipientMember_UserId_IsActive",
                table: "RecipientMember",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Referral_CreatedByUserId",
                table: "Referral",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_RecipientId_Status",
                table: "Referral",
                columns: new[] { "RecipientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Referral_Reference",
                table: "Referral",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Referral_RequestId_Id",
                table: "Referral",
                columns: new[] { "RequestId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRecipient_ApprovalStatus_Active",
                table: "ReferralRecipient",
                columns: new[] { "ApprovalStatus", "Active" });

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRecipient_Reference",
                table: "ReferralRecipient",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_NormalizedName",
                table: "Role",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccount_NormalizedEmail",
                table: "UserAccount",
                column: "NormalizedEmail",
                unique: true,
                filter: "\"NormalizedEmail\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccount_NormalizedUserName",
                table: "UserAccount",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId_RoleId",
                table: "UserRole",
                columns: new[] { "UserId", "RoleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationAttachment");

            migrationBuilder.DropTable(
                name: "Charity");

            migrationBuilder.DropTable(
                name: "DecisionHistory");

            migrationBuilder.DropTable(
                name: "EvidenceAttachment");

            migrationBuilder.DropTable(
                name: "OutcomeReview");

            migrationBuilder.DropTable(
                name: "RecipientDecision");

            migrationBuilder.DropTable(
                name: "RecipientMember");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Outcome");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Referral");

            migrationBuilder.DropTable(
                name: "AssistanceRequest");

            migrationBuilder.DropTable(
                name: "ReferralRecipient");

            migrationBuilder.DropTable(
                name: "Beneficiary");

            migrationBuilder.DropTable(
                name: "UserAccount");

            migrationBuilder.DropSequence(
                name: "ben_reference_seq");

            migrationBuilder.DropSequence(
                name: "out_reference_seq");

            migrationBuilder.DropSequence(
                name: "rec_reference_seq");

            migrationBuilder.DropSequence(
                name: "ref_reference_seq");

            migrationBuilder.DropSequence(
                name: "req_reference_seq");
        }
    }
}
