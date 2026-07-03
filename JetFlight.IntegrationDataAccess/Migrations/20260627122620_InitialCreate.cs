using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JetFlight.IntegrationDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccumulationCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountToComplete = table.Column<int>(type: "int", nullable: false),
                    AllRequired = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccumulationCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sex = table.Column<int>(type: "int", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Street = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhereFindOut = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Longitude = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RegistrationPlatform = table.Column<int>(type: "int", nullable: true),
                    TypeOfActivity = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StoreNearHomeId = table.Column<int>(type: "int", nullable: true),
                    PersonalQuestionaryCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfChildren = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLink", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    MessageId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductDivisions",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDivisions", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ProductManufacturers",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductManufacturers", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ProductsSuppliers",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(20)", nullable: false),
                    Title = table.Column<string>(type: "varchar(150)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsSuppliers", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ProductsTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromotionDisplayRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<byte>(type: "tinyint", nullable: false),
                    RelevantCount = table.Column<int>(type: "int", nullable: false),
                    PerRuleCount = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    AgeFrom = table.Column<int>(type: "int", nullable: true),
                    AgeTo = table.Column<int>(type: "int", nullable: true),
                    CheckAmountFrom = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CheckAmountTo = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionDisplayRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromotionQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromotionId = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionQueues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromotionsType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NavisionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionsType", x => x.Id);
                    table.UniqueConstraint("AK_PromotionsType_NavisionId", x => x.NavisionId);
                });

            migrationBuilder.CreateTable(
                name: "TargetSmsMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<byte>(type: "tinyint", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Theme = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetSmsMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebProductCategories",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    ParentCode = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebProductCategories", x => x.Code);
                    table.ForeignKey(
                        name: "FK_WebProductCategories_WebProductCategories_ParentCode",
                        column: x => x.ParentCode,
                        principalTable: "WebProductCategories",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "AccumulationCardToTarget",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccumulationCardId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccumulationCardToTarget", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccumulationCardToTarget_AccumulationCards_AccumulationCardId",
                        column: x => x.AccumulationCardId,
                        principalTable: "AccumulationCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivateName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivateDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmissionBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCardCoupon = table.Column<bool>(type: "bit", nullable: false),
                    UseTimes = table.Column<int>(type: "int", nullable: false),
                    Emission = table.Column<int>(type: "int", nullable: false),
                    Class = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    AccumulationCardId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coupons_AccumulationCards_AccumulationCardId",
                        column: x => x.AccumulationCardId,
                        principalTable: "AccumulationCards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerAccumulationCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Counter = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AccumulationCardId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAccumulationCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerAccumulationCards_AccumulationCards_AccumulationCardId",
                        column: x => x.AccumulationCardId,
                        principalTable: "AccumulationCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerAccumulationCards_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerCards",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<byte>(type: "tinyint", nullable: false),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCards", x => x.Code);
                    table.ForeignKey(
                        name: "FK_CustomerCards_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DevicePlatformId = table.Column<int>(type: "int", nullable: true),
                    DeviceToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastActive = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerDevices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: true),
                    Read = table.Column<bool>(type: "bit", nullable: true),
                    Received = table.Column<bool>(type: "bit", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerNotifications_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    PushNotificationToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchId = table.Column<byte>(type: "tinyint", nullable: false),
                    EnablePushNotifications = table.Column<bool>(type: "bit", nullable: false),
                    EnableEmailNotifications = table.Column<bool>(type: "bit", nullable: false),
                    EnableSmsNotifications = table.Column<bool>(type: "bit", nullable: false),
                    EnableSubscription = table.Column<bool>(type: "bit", nullable: false),
                    EnableCookies = table.Column<bool>(type: "bit", nullable: true),
                    AutomaticWithdrawal = table.Column<bool>(type: "bit", nullable: false),
                    AccumulateRest = table.Column<bool>(type: "bit", nullable: false),
                    ActiveStoreId = table.Column<int>(type: "int", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerSettings_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailVerificationTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerificationTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailVerificationTokens_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TokenFamily = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReplacedByTokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ReplacedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RfmCustomerSnapshots",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "date", nullable: false),
                    RfmId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<byte>(type: "tinyint", nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    StoreId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RfmCustomerSnapshots", x => new { x.SnapshotDate, x.CustomerId });
                    table.ForeignKey(
                        name: "FK_RfmCustomerSnapshots_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TargetEmailMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<byte>(type: "tinyint", nullable: false),
                    Theme = table.Column<int>(type: "int", nullable: false),
                    MainImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MainHeader = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondHeader = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetEmailMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TargetEmailMessages_EmailLink_LinkId",
                        column: x => x.LinkId,
                        principalTable: "EmailLink",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductSegments",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionCode = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSegments", x => x.Code);
                    table.ForeignKey(
                        name: "FK_ProductSegments_ProductDivisions_DivisionCode",
                        column: x => x.DivisionCode,
                        principalTable: "ProductDivisions",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductBrands",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManufacturerCode = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBrands", x => x.Code);
                    table.ForeignKey(
                        name: "FK_ProductBrands_ProductManufacturers_ManufacturerCode",
                        column: x => x.ManufacturerCode,
                        principalTable: "ProductManufacturers",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionDisplayRuleToActivityType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeOfActivity = table.Column<int>(type: "int", nullable: false),
                    PromotionDisplayRuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionDisplayRuleToActivityType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionDisplayRuleToActivityType_PromotionDisplayRules_PromotionDisplayRuleId",
                        column: x => x.PromotionDisplayRuleId,
                        principalTable: "PromotionDisplayRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionDisplayRuleToStore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PromotionDisplayRuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionDisplayRuleToStore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionDisplayRuleToStore_PromotionDisplayRules_PromotionDisplayRuleId",
                        column: x => x.PromotionDisplayRuleId,
                        principalTable: "PromotionDisplayRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionTypeBranch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    PromotionsTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionTypeBranch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionTypeBranch_PromotionsType_PromotionsTypeId",
                        column: x => x.PromotionsTypeId,
                        principalTable: "PromotionsType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerSmsMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SmsMessageId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSmsMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerSmsMessages_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerSmsMessages_TargetSmsMessages_SmsMessageId",
                        column: x => x.SmsMessageId,
                        principalTable: "TargetSmsMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponAdditionalBonuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    Bonus = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponAdditionalBonuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponAdditionalBonuses_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponBonusMultipliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    Multiplier = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponBonusMultipliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponBonusMultipliers_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponCombinationPriceFixedPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    FixedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponCombinationPriceFixedPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponCombinationPriceFixedPrices_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponDiscountAmounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponDiscountAmounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponDiscountAmounts_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponDiscountPercents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponDiscountPercents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponDiscountPercents_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponToStore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CouponId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponToStore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponToStore_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponToTarget",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponToTarget", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponToTarget_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerCoupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    UsedTimes = table.Column<int>(type: "int", nullable: false),
                    Activated = table.Column<bool>(type: "bit", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCoupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerCoupons_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerCoupons_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questionaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchId = table.Column<byte>(type: "tinyint", nullable: true),
                    BonusReward = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActiveDaysAfterComplete = table.Column<int>(type: "int", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    CouponId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questionaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questionaries_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerBonusTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    CardCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountRemaining = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: true),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerBonusTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerBonusTransactions_CustomerCards_CardCode",
                        column: x => x.CardCode,
                        principalTable: "CustomerCards",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerBonusTransactions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerEmailMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailMessageId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerEmailMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerEmailMessages_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerEmailMessages_TargetEmailMessages_EmailMessageId",
                        column: x => x.EmailMessageId,
                        principalTable: "TargetEmailMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailBlock",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Header = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkId = table.Column<int>(type: "int", nullable: true),
                    TargetEmailMessageId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailBlock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailBlock_EmailLink_LinkId",
                        column: x => x.LinkId,
                        principalTable: "EmailLink",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmailBlock_TargetEmailMessages_TargetEmailMessageId",
                        column: x => x.TargetEmailMessageId,
                        principalTable: "TargetEmailMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SegmentCode = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Code);
                    table.ForeignKey(
                        name: "FK_ProductCategories_ProductSegments_SegmentCode",
                        column: x => x.SegmentCode,
                        principalTable: "ProductSegments",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponMultiplierBrandActivators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CouponBonusMultiplierId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponMultiplierBrandActivators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponMultiplierBrandActivators_CouponBonusMultipliers_CouponBonusMultiplierId",
                        column: x => x.CouponBonusMultiplierId,
                        principalTable: "CouponBonusMultipliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponMultiplierBrandActivators_ProductBrands_BrandCode",
                        column: x => x.BrandCode,
                        principalTable: "ProductBrands",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CouponMultiplierManufacturerActivators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManufacturerCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CouponBonusMultiplierId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponMultiplierManufacturerActivators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponMultiplierManufacturerActivators_CouponBonusMultipliers_CouponBonusMultiplierId",
                        column: x => x.CouponBonusMultiplierId,
                        principalTable: "CouponBonusMultipliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponMultiplierManufacturerActivators_ProductManufacturers_ManufacturerCode",
                        column: x => x.ManufacturerCode,
                        principalTable: "ProductManufacturers",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CouponMultiplierSupplierActivators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    CouponBonusMultiplierId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponMultiplierSupplierActivators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponMultiplierSupplierActivators_CouponBonusMultipliers_CouponBonusMultiplierId",
                        column: x => x.CouponBonusMultiplierId,
                        principalTable: "CouponBonusMultipliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponMultiplierSupplierActivators_ProductsSuppliers_SupplierCode",
                        column: x => x.SupplierCode,
                        principalTable: "ProductsSuppliers",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionaryAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionaryId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BranchId = table.Column<byte>(type: "tinyint", nullable: false),
                    ClientPlatform = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionaryAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswers_Questionaries_QuestionaryId",
                        column: x => x.QuestionaryId,
                        principalTable: "Questionaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionaryField",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    QuestionaryId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Validation = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionaryField", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionaryField_Questionaries_QuestionaryId",
                        column: x => x.QuestionaryId,
                        principalTable: "Questionaries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BonusUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PosTerminal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerBonusTransactionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonusUsages_CustomerBonusTransactions_CustomerBonusTransactionId",
                        column: x => x.CustomerBonusTransactionId,
                        principalTable: "CustomerBonusTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TransactionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PosTerminal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    IsReturn = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoreCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmountWithDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccumulatedBonusesSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsedBonusesSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustomerBonusTransactionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receipts_CustomerBonusTransactions_CustomerBonusTransactionId",
                        column: x => x.CustomerBonusTransactionId,
                        principalTable: "CustomerBonusTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Receipts_CustomerCards_CardCode",
                        column: x => x.CardCode,
                        principalTable: "CustomerCards",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "CouponMultiplierCategoryActivators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CouponBonusMultiplierId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponMultiplierCategoryActivators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponMultiplierCategoryActivators_CouponBonusMultipliers_CouponBonusMultiplierId",
                        column: x => x.CouponBonusMultiplierId,
                        principalTable: "CouponBonusMultipliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponMultiplierCategoryActivators_ProductCategories_CategoryCode",
                        column: x => x.CategoryCode,
                        principalTable: "ProductCategories",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductFamilies",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryCode = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFamilies", x => x.Code);
                    table.ForeignKey(
                        name: "FK_ProductFamilies_ProductCategories_CategoryCode",
                        column: x => x.CategoryCode,
                        principalTable: "ProductCategories",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryCode = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionCategories_ProductCategories_CategoryCode",
                        column: x => x.CategoryCode,
                        principalTable: "ProductCategories",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionaryAnswerField<DateTime>",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionaryFieldId = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuestionaryAnswerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionaryAnswerField<DateTime>", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswerField<DateTime>_QuestionaryAnswers_QuestionaryAnswerId",
                        column: x => x.QuestionaryAnswerId,
                        principalTable: "QuestionaryAnswers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswerField<DateTime>_QuestionaryField_QuestionaryFieldId",
                        column: x => x.QuestionaryFieldId,
                        principalTable: "QuestionaryField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionaryAnswerField<float>",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionaryFieldId = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<float>(type: "real", nullable: false),
                    QuestionaryAnswerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionaryAnswerField<float>", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswerField<float>_QuestionaryAnswers_QuestionaryAnswerId",
                        column: x => x.QuestionaryAnswerId,
                        principalTable: "QuestionaryAnswers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswerField<float>_QuestionaryField_QuestionaryFieldId",
                        column: x => x.QuestionaryFieldId,
                        principalTable: "QuestionaryField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionaryAnswerField<int>",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionaryFieldId = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<int>(type: "int", nullable: false),
                    QuestionaryAnswerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionaryAnswerField<int>", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswerField<int>_QuestionaryAnswers_QuestionaryAnswerId",
                        column: x => x.QuestionaryAnswerId,
                        principalTable: "QuestionaryAnswers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswerField<int>_QuestionaryField_QuestionaryFieldId",
                        column: x => x.QuestionaryFieldId,
                        principalTable: "QuestionaryField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionaryAnswerField<List<QuestionarySelectOption>>",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionaryFieldId = table.Column<int>(type: "int", nullable: false),
                    QuestionaryAnswerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionaryAnswerField<List<QuestionarySelectOption>>", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswerField<List<QuestionarySelectOption>>_QuestionaryAnswers_QuestionaryAnswerId",
                        column: x => x.QuestionaryAnswerId,
                        principalTable: "QuestionaryAnswers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswerField<List<QuestionarySelectOption>>_QuestionaryField_QuestionaryFieldId",
                        column: x => x.QuestionaryFieldId,
                        principalTable: "QuestionaryField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionaryAnswerField<string>",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionaryFieldId = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuestionaryAnswerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionaryAnswerField<string>", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswerField<string>_QuestionaryAnswers_QuestionaryAnswerId",
                        column: x => x.QuestionaryAnswerId,
                        principalTable: "QuestionaryAnswers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswerField<string>_QuestionaryField_QuestionaryFieldId",
                        column: x => x.QuestionaryFieldId,
                        principalTable: "QuestionaryField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptCustomerCoupon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerCouponId = table.Column<int>(type: "int", nullable: false),
                    LineNo = table.Column<int>(type: "int", nullable: false),
                    ReceiptId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptCustomerCoupon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptCustomerCoupon_CustomerCoupons_CustomerCouponId",
                        column: x => x.CustomerCouponId,
                        principalTable: "CustomerCoupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceiptCustomerCoupon_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FamilyCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BrandCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    InActive = table.Column<bool>(type: "bit", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Products_ProductBrands_BrandCode",
                        column: x => x.BrandCode,
                        principalTable: "ProductBrands",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_ProductFamilies_FamilyCode",
                        column: x => x.FamilyCode,
                        principalTable: "ProductFamilies",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionarySelectOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionaryFieldId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionaryAnswerFieldListQuestionarySelectOptionId = table.Column<int>(name: "QuestionaryAnswerField<List<QuestionarySelectOption>>Id", type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionarySelectOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionarySelectOption_QuestionaryAnswerField<List<QuestionarySelectOption>>_QuestionaryAnswerField<List<QuestionarySelectO~",
                        column: x => x.QuestionaryAnswerFieldListQuestionarySelectOptionId,
                        principalTable: "QuestionaryAnswerField<List<QuestionarySelectOption>>",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestionarySelectOption_QuestionaryField_QuestionaryFieldId",
                        column: x => x.QuestionaryFieldId,
                        principalTable: "QuestionaryField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponCombinationFixedPriceActivator",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CouponCombinationFixedPriceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponCombinationFixedPriceActivator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponCombinationFixedPriceActivator_CouponCombinationPriceFixedPrices_CouponCombinationFixedPriceId",
                        column: x => x.CouponCombinationFixedPriceId,
                        principalTable: "CouponCombinationPriceFixedPrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponCombinationFixedPriceActivator_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalTable: "Products",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CouponCombinationPriceDiscounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    AllRequired = table.Column<bool>(type: "bit", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SupplierCode = table.Column<string>(type: "varchar(20)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Compensation = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponCombinationPriceDiscounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponCombinationPriceDiscounts_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponCombinationPriceDiscounts_ProductsSuppliers_SupplierCode",
                        column: x => x.SupplierCode,
                        principalTable: "ProductsSuppliers",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponCombinationPriceDiscounts_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalTable: "Products",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponMultiplierProductActivators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CouponBonusMultiplierId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponMultiplierProductActivators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponMultiplierProductActivators_CouponBonusMultipliers_CouponBonusMultiplierId",
                        column: x => x.CouponBonusMultiplierId,
                        principalTable: "CouponBonusMultipliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponMultiplierProductActivators_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalTable: "Products",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CouponProductFixedPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quanitity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponProductFixedPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponProductFixedPrices_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponProductFixedPrices_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalTable: "Products",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyProgramRestrictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SegmentCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CategoryCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FamilyCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ExcludeAllProducts = table.Column<bool>(type: "bit", nullable: false),
                    ExcludeMinPrice = table.Column<bool>(type: "bit", nullable: false),
                    ExcludeProductsWithPromotionPrice = table.Column<bool>(type: "bit", nullable: false),
                    ExcludePromotions = table.Column<bool>(type: "bit", nullable: false),
                    ExcludeOtherDocuments = table.Column<bool>(type: "bit", nullable: false),
                    ExcludePartialDiscount = table.Column<bool>(type: "bit", nullable: false),
                    ExcludeServiceCode = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyProgramRestrictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyProgramRestrictions_ProductCategories_CategoryCode",
                        column: x => x.CategoryCode,
                        principalTable: "ProductCategories",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_LoyaltyProgramRestrictions_ProductDivisions_DivisionCode",
                        column: x => x.DivisionCode,
                        principalTable: "ProductDivisions",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_LoyaltyProgramRestrictions_ProductFamilies_FamilyCode",
                        column: x => x.FamilyCode,
                        principalTable: "ProductFamilies",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_LoyaltyProgramRestrictions_ProductSegments_SegmentCode",
                        column: x => x.SegmentCode,
                        principalTable: "ProductSegments",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_LoyaltyProgramRestrictions_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalTable: "Products",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "ProductQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductQueues_Products_Code",
                        column: x => x.Code,
                        principalTable: "Products",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTags_ProductsTags_TagId",
                        column: x => x.TagId,
                        principalTable: "ProductsTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTags_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalTable: "Products",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Offer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EligibleAirportIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PromotionTypeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PromoPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsComplexPromotion = table.Column<bool>(type: "bit", nullable: false),
                    WebProductCategoryCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ItemUnit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Promotions_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalTable: "Products",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_Promotions_PromotionsType_PromotionTypeId",
                        column: x => x.PromotionTypeId,
                        principalTable: "PromotionsType",
                        principalColumn: "NavisionId");
                    table.ForeignKey(
                        name: "FK_Promotions_WebProductCategories_WebProductCategoryCode",
                        column: x => x.WebProductCategoryCode,
                        principalTable: "WebProductCategories",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemUnit = table.Column<int>(type: "int", nullable: false),
                    LineNo = table.Column<int>(type: "int", nullable: false),
                    LineTotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotalAmountWithDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptProducts_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalTable: "Products",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_ReceiptProducts_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionaryAnswerField<QuestionarySelectOption>",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionaryFieldId = table.Column<int>(type: "int", nullable: false),
                    AnswerId = table.Column<int>(type: "int", nullable: true),
                    QuestionaryAnswerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionaryAnswerField<QuestionarySelectOption>", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswerField<QuestionarySelectOption>_QuestionaryAnswers_QuestionaryAnswerId",
                        column: x => x.QuestionaryAnswerId,
                        principalTable: "QuestionaryAnswers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswerField<QuestionarySelectOption>_QuestionaryField_QuestionaryFieldId",
                        column: x => x.QuestionaryFieldId,
                        principalTable: "QuestionaryField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionaryAnswerField<QuestionarySelectOption>_QuestionarySelectOption_AnswerId",
                        column: x => x.AnswerId,
                        principalTable: "QuestionarySelectOption",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CouponCombinationBrandActivator",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CouponCombinationPriceDiscountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponCombinationBrandActivator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponCombinationBrandActivator_CouponCombinationPriceDiscounts_CouponCombinationPriceDiscountId",
                        column: x => x.CouponCombinationPriceDiscountId,
                        principalTable: "CouponCombinationPriceDiscounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponCombinationBrandActivator_ProductBrands_BrandCode",
                        column: x => x.BrandCode,
                        principalTable: "ProductBrands",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CouponCombinationCategoryActivator",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CouponCombinationPriceDiscountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponCombinationCategoryActivator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponCombinationCategoryActivator_CouponCombinationPriceDiscounts_CouponCombinationPriceDiscountId",
                        column: x => x.CouponCombinationPriceDiscountId,
                        principalTable: "CouponCombinationPriceDiscounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponCombinationCategoryActivator_ProductCategories_CategoryCode",
                        column: x => x.CategoryCode,
                        principalTable: "ProductCategories",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CouponCombinationManufacturerActivator",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManufacturerCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CouponCombinationPriceDiscountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponCombinationManufacturerActivator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponCombinationManufacturerActivator_CouponCombinationPriceDiscounts_CouponCombinationPriceDiscountId",
                        column: x => x.CouponCombinationPriceDiscountId,
                        principalTable: "CouponCombinationPriceDiscounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponCombinationManufacturerActivator_ProductManufacturers_ManufacturerCode",
                        column: x => x.ManufacturerCode,
                        principalTable: "ProductManufacturers",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CouponCombinationProductActivator",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CouponCombinationPriceDiscountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponCombinationProductActivator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponCombinationProductActivator_CouponCombinationPriceDiscounts_CouponCombinationPriceDiscountId",
                        column: x => x.CouponCombinationPriceDiscountId,
                        principalTable: "CouponCombinationPriceDiscounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponCombinationProductActivator_Products_ProductCode",
                        column: x => x.ProductCode,
                        principalTable: "Products",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CouponCombinationSupplierActivator",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    CouponCombinationPriceDiscountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponCombinationSupplierActivator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponCombinationSupplierActivator_CouponCombinationPriceDiscounts_CouponCombinationPriceDiscountId",
                        column: x => x.CouponCombinationPriceDiscountId,
                        principalTable: "CouponCombinationPriceDiscounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponCombinationSupplierActivator_ProductsSuppliers_SupplierCode",
                        column: x => x.SupplierCode,
                        principalTable: "ProductsSuppliers",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SavedPromotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    PromotionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedPromotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedPromotions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedPromotions_Promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccumulationCardToTarget_AccumulationCardId",
                table: "AccumulationCardToTarget",
                column: "AccumulationCardId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusUsages_CustomerBonusTransactionId",
                table: "BonusUsages",
                column: "CustomerBonusTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponAdditionalBonuses_CouponId",
                table: "CouponAdditionalBonuses",
                column: "CouponId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponBonusMultipliers_CouponId",
                table: "CouponBonusMultipliers",
                column: "CouponId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationBrandActivator_BrandCode",
                table: "CouponCombinationBrandActivator",
                column: "BrandCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationBrandActivator_CouponCombinationPriceDiscountId",
                table: "CouponCombinationBrandActivator",
                column: "CouponCombinationPriceDiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationCategoryActivator_CategoryCode",
                table: "CouponCombinationCategoryActivator",
                column: "CategoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationCategoryActivator_CouponCombinationPriceDiscountId",
                table: "CouponCombinationCategoryActivator",
                column: "CouponCombinationPriceDiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationFixedPriceActivator_CouponCombinationFixedPriceId",
                table: "CouponCombinationFixedPriceActivator",
                column: "CouponCombinationFixedPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationFixedPriceActivator_ProductCode",
                table: "CouponCombinationFixedPriceActivator",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationManufacturerActivator_CouponCombinationPriceDiscountId",
                table: "CouponCombinationManufacturerActivator",
                column: "CouponCombinationPriceDiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationManufacturerActivator_ManufacturerCode",
                table: "CouponCombinationManufacturerActivator",
                column: "ManufacturerCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationPriceDiscounts_CouponId",
                table: "CouponCombinationPriceDiscounts",
                column: "CouponId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationPriceDiscounts_ProductCode",
                table: "CouponCombinationPriceDiscounts",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationPriceDiscounts_SupplierCode",
                table: "CouponCombinationPriceDiscounts",
                column: "SupplierCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationPriceFixedPrices_CouponId",
                table: "CouponCombinationPriceFixedPrices",
                column: "CouponId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationProductActivator_CouponCombinationPriceDiscountId",
                table: "CouponCombinationProductActivator",
                column: "CouponCombinationPriceDiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationProductActivator_ProductCode",
                table: "CouponCombinationProductActivator",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationSupplierActivator_CouponCombinationPriceDiscountId",
                table: "CouponCombinationSupplierActivator",
                column: "CouponCombinationPriceDiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponCombinationSupplierActivator_SupplierCode",
                table: "CouponCombinationSupplierActivator",
                column: "SupplierCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponDiscountAmounts_CouponId",
                table: "CouponDiscountAmounts",
                column: "CouponId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponDiscountPercents_CouponId",
                table: "CouponDiscountPercents",
                column: "CouponId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponMultiplierBrandActivators_BrandCode",
                table: "CouponMultiplierBrandActivators",
                column: "BrandCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponMultiplierBrandActivators_CouponBonusMultiplierId",
                table: "CouponMultiplierBrandActivators",
                column: "CouponBonusMultiplierId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponMultiplierCategoryActivators_CategoryCode",
                table: "CouponMultiplierCategoryActivators",
                column: "CategoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponMultiplierCategoryActivators_CouponBonusMultiplierId",
                table: "CouponMultiplierCategoryActivators",
                column: "CouponBonusMultiplierId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponMultiplierManufacturerActivators_CouponBonusMultiplierId",
                table: "CouponMultiplierManufacturerActivators",
                column: "CouponBonusMultiplierId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponMultiplierManufacturerActivators_ManufacturerCode",
                table: "CouponMultiplierManufacturerActivators",
                column: "ManufacturerCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponMultiplierProductActivators_CouponBonusMultiplierId",
                table: "CouponMultiplierProductActivators",
                column: "CouponBonusMultiplierId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponMultiplierProductActivators_ProductCode",
                table: "CouponMultiplierProductActivators",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponMultiplierSupplierActivators_CouponBonusMultiplierId",
                table: "CouponMultiplierSupplierActivators",
                column: "CouponBonusMultiplierId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponMultiplierSupplierActivators_SupplierCode",
                table: "CouponMultiplierSupplierActivators",
                column: "SupplierCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponProductFixedPrices_CouponId",
                table: "CouponProductFixedPrices",
                column: "CouponId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponProductFixedPrices_ProductCode",
                table: "CouponProductFixedPrices",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_AccumulationCardId",
                table: "Coupons",
                column: "AccumulationCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponToStore_CouponId",
                table: "CouponToStore",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponToTarget_CouponId",
                table: "CouponToTarget",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAccumulationCards_AccumulationCardId",
                table: "CustomerAccumulationCards",
                column: "AccumulationCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAccumulationCards_CustomerId",
                table: "CustomerAccumulationCards",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerBonusTransactions_CardCode",
                table: "CustomerBonusTransactions",
                column: "CardCode");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerBonusTransactions_CustomerId",
                table: "CustomerBonusTransactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCard_CustomerId",
                table: "CustomerCards",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCoupons_CouponId",
                table: "CustomerCoupons",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCoupons_CustomerId",
                table: "CustomerCoupons",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDevices_CustomerId",
                table: "CustomerDevices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerEmailMessages_CustomerId",
                table: "CustomerEmailMessages",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerEmailMessages_EmailMessageId",
                table: "CustomerEmailMessages",
                column: "EmailMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotifications_CustomerId",
                table: "CustomerNotifications",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Birthday",
                table: "Customers",
                column: "Birthday");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNumber",
                table: "Customers",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSettings_CustomerId",
                table: "CustomerSettings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSmsMessages_CustomerId",
                table: "CustomerSmsMessages",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSmsMessages_SmsMessageId",
                table: "CustomerSmsMessages",
                column: "SmsMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailBlock_LinkId",
                table: "EmailBlock",
                column: "LinkId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailBlock_TargetEmailMessageId",
                table: "EmailBlock",
                column: "TargetEmailMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationTokens_CustomerId",
                table: "EmailVerificationTokens",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationTokens_Token",
                table: "EmailVerificationTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramRestrictions_CategoryCode",
                table: "LoyaltyProgramRestrictions",
                column: "CategoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramRestrictions_DivisionCode",
                table: "LoyaltyProgramRestrictions",
                column: "DivisionCode");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramRestrictions_FamilyCode",
                table: "LoyaltyProgramRestrictions",
                column: "FamilyCode");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramRestrictions_ProductCode",
                table: "LoyaltyProgramRestrictions",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramRestrictions_SegmentCode",
                table: "LoyaltyProgramRestrictions",
                column: "SegmentCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBrands_ManufacturerCode",
                table: "ProductBrands",
                column: "ManufacturerCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_SegmentCode",
                table: "ProductCategories",
                column: "SegmentCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFamilies_CategoryCode",
                table: "ProductFamilies",
                column: "CategoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQueues_Code",
                table: "ProductQueues",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandCode",
                table: "Products",
                column: "BrandCode");

            migrationBuilder.CreateIndex(
                name: "IX_Products_FamilyCode",
                table: "Products",
                column: "FamilyCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSegments_DivisionCode",
                table: "ProductSegments",
                column: "DivisionCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTags_ProductCode",
                table: "ProductTags",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTags_TagId",
                table: "ProductTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCategories_CategoryCode",
                table: "PromotionCategories",
                column: "CategoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionDisplayRuleToActivityType_PromotionDisplayRuleId",
                table: "PromotionDisplayRuleToActivityType",
                column: "PromotionDisplayRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionDisplayRuleToStore_PromotionDisplayRuleId",
                table: "PromotionDisplayRuleToStore",
                column: "PromotionDisplayRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_ProductCode",
                table: "Promotions",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_PromotionTypeId",
                table: "Promotions",
                column: "PromotionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_WebProductCategoryCode",
                table: "Promotions",
                column: "WebProductCategoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionTypeBranch_PromotionsTypeId",
                table: "PromotionTypeBranch",
                column: "PromotionsTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Questionaries_CouponId",
                table: "Questionaries",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswerField<DateTime>_QuestionaryAnswerId",
                table: "QuestionaryAnswerField<DateTime>",
                column: "QuestionaryAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswerField<DateTime>_QuestionaryFieldId",
                table: "QuestionaryAnswerField<DateTime>",
                column: "QuestionaryFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswerField<float>_QuestionaryAnswerId",
                table: "QuestionaryAnswerField<float>",
                column: "QuestionaryAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswerField<float>_QuestionaryFieldId",
                table: "QuestionaryAnswerField<float>",
                column: "QuestionaryFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswerField<int>_QuestionaryAnswerId",
                table: "QuestionaryAnswerField<int>",
                column: "QuestionaryAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswerField<int>_QuestionaryFieldId",
                table: "QuestionaryAnswerField<int>",
                column: "QuestionaryFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswerField<List<QuestionarySelectOption>>_QuestionaryAnswerId",
                table: "QuestionaryAnswerField<List<QuestionarySelectOption>>",
                column: "QuestionaryAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswerField<List<QuestionarySelectOption>>_QuestionaryFieldId",
                table: "QuestionaryAnswerField<List<QuestionarySelectOption>>",
                column: "QuestionaryFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswerField<QuestionarySelectOption>_AnswerId",
                table: "QuestionaryAnswerField<QuestionarySelectOption>",
                column: "AnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswerField<QuestionarySelectOption>_QuestionaryAnswerId",
                table: "QuestionaryAnswerField<QuestionarySelectOption>",
                column: "QuestionaryAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswerField<QuestionarySelectOption>_QuestionaryFieldId",
                table: "QuestionaryAnswerField<QuestionarySelectOption>",
                column: "QuestionaryFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswerField<string>_QuestionaryAnswerId",
                table: "QuestionaryAnswerField<string>",
                column: "QuestionaryAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswerField<string>_QuestionaryFieldId",
                table: "QuestionaryAnswerField<string>",
                column: "QuestionaryFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswers_CustomerId",
                table: "QuestionaryAnswers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryAnswers_QuestionaryId",
                table: "QuestionaryAnswers",
                column: "QuestionaryId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionaryField_QuestionaryId",
                table: "QuestionaryField",
                column: "QuestionaryId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionarySelectOption_QuestionaryAnswerField<List<QuestionarySelectOption>>Id",
                table: "QuestionarySelectOption",
                column: "QuestionaryAnswerField<List<QuestionarySelectOption>>Id");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionarySelectOption_QuestionaryFieldId",
                table: "QuestionarySelectOption",
                column: "QuestionaryFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptCustomerCoupon_CustomerCouponId",
                table: "ReceiptCustomerCoupon",
                column: "CustomerCouponId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptCustomerCoupon_ReceiptId",
                table: "ReceiptCustomerCoupon",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptProducts_ProductCode",
                table: "ReceiptProducts",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptProducts_ReceiptId",
                table: "ReceiptProducts",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipt_BranchId",
                table: "Receipts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipt_BranchId_CreatedAt",
                table: "Receipts",
                columns: new[] { "BranchId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Receipt_CardCode",
                table: "Receipts",
                column: "CardCode");

            migrationBuilder.CreateIndex(
                name: "IX_Receipt_CreatedAt",
                table: "Receipts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Receipt_StoreCode_CreatedAt",
                table: "Receipts",
                columns: new[] { "StoreCode", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_CustomerBonusTransactionId",
                table: "Receipts",
                column: "CustomerBonusTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_StoreCode",
                table: "Receipts",
                column: "StoreCode");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_CustomerId_IsRevoked_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "CustomerId", "IsRevoked", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt_IsRevoked",
                table: "RefreshTokens",
                columns: new[] { "ExpiresAt", "IsRevoked" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenFamily_Active",
                table: "RefreshTokens",
                column: "TokenFamily",
                unique: true,
                filter: "[IsRevoked] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RfmCustomerSnapshots_CustomerId",
                table: "RfmCustomerSnapshots",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RfmCustomerSnapshots_RfmId_SnapshotDate",
                table: "RfmCustomerSnapshots",
                columns: new[] { "RfmId", "SnapshotDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RfmCustomerSnapshots_SnapshotDate_RfmId",
                table: "RfmCustomerSnapshots",
                columns: new[] { "SnapshotDate", "RfmId" });

            migrationBuilder.CreateIndex(
                name: "IX_SavedPromotions_CustomerId",
                table: "SavedPromotions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedPromotions_PromotionId",
                table: "SavedPromotions",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetEmailMessages_LinkId",
                table: "TargetEmailMessages",
                column: "LinkId");

            migrationBuilder.CreateIndex(
                name: "IX_WebProductCategories_ParentCode",
                table: "WebProductCategories",
                column: "ParentCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccumulationCardToTarget");

            migrationBuilder.DropTable(
                name: "BonusUsages");

            migrationBuilder.DropTable(
                name: "CouponAdditionalBonuses");

            migrationBuilder.DropTable(
                name: "CouponCombinationBrandActivator");

            migrationBuilder.DropTable(
                name: "CouponCombinationCategoryActivator");

            migrationBuilder.DropTable(
                name: "CouponCombinationFixedPriceActivator");

            migrationBuilder.DropTable(
                name: "CouponCombinationManufacturerActivator");

            migrationBuilder.DropTable(
                name: "CouponCombinationProductActivator");

            migrationBuilder.DropTable(
                name: "CouponCombinationSupplierActivator");

            migrationBuilder.DropTable(
                name: "CouponDiscountAmounts");

            migrationBuilder.DropTable(
                name: "CouponDiscountPercents");

            migrationBuilder.DropTable(
                name: "CouponMultiplierBrandActivators");

            migrationBuilder.DropTable(
                name: "CouponMultiplierCategoryActivators");

            migrationBuilder.DropTable(
                name: "CouponMultiplierManufacturerActivators");

            migrationBuilder.DropTable(
                name: "CouponMultiplierProductActivators");

            migrationBuilder.DropTable(
                name: "CouponMultiplierSupplierActivators");

            migrationBuilder.DropTable(
                name: "CouponProductFixedPrices");

            migrationBuilder.DropTable(
                name: "CouponToStore");

            migrationBuilder.DropTable(
                name: "CouponToTarget");

            migrationBuilder.DropTable(
                name: "CustomerAccumulationCards");

            migrationBuilder.DropTable(
                name: "CustomerDevices");

            migrationBuilder.DropTable(
                name: "CustomerEmailMessages");

            migrationBuilder.DropTable(
                name: "CustomerNotifications");

            migrationBuilder.DropTable(
                name: "CustomerSettings");

            migrationBuilder.DropTable(
                name: "CustomerSmsMessages");

            migrationBuilder.DropTable(
                name: "EmailBlock");

            migrationBuilder.DropTable(
                name: "EmailVerificationTokens");

            migrationBuilder.DropTable(
                name: "LoyaltyProgramRestrictions");

            migrationBuilder.DropTable(
                name: "NotificationHistories");

            migrationBuilder.DropTable(
                name: "ProductQueues");

            migrationBuilder.DropTable(
                name: "ProductTags");

            migrationBuilder.DropTable(
                name: "PromotionCategories");

            migrationBuilder.DropTable(
                name: "PromotionDisplayRuleToActivityType");

            migrationBuilder.DropTable(
                name: "PromotionDisplayRuleToStore");

            migrationBuilder.DropTable(
                name: "PromotionQueues");

            migrationBuilder.DropTable(
                name: "PromotionTypeBranch");

            migrationBuilder.DropTable(
                name: "QuestionaryAnswerField<DateTime>");

            migrationBuilder.DropTable(
                name: "QuestionaryAnswerField<float>");

            migrationBuilder.DropTable(
                name: "QuestionaryAnswerField<int>");

            migrationBuilder.DropTable(
                name: "QuestionaryAnswerField<QuestionarySelectOption>");

            migrationBuilder.DropTable(
                name: "QuestionaryAnswerField<string>");

            migrationBuilder.DropTable(
                name: "ReceiptCustomerCoupon");

            migrationBuilder.DropTable(
                name: "ReceiptProducts");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RfmCustomerSnapshots");

            migrationBuilder.DropTable(
                name: "SavedPromotions");

            migrationBuilder.DropTable(
                name: "CouponCombinationPriceFixedPrices");

            migrationBuilder.DropTable(
                name: "CouponCombinationPriceDiscounts");

            migrationBuilder.DropTable(
                name: "CouponBonusMultipliers");

            migrationBuilder.DropTable(
                name: "TargetSmsMessages");

            migrationBuilder.DropTable(
                name: "TargetEmailMessages");

            migrationBuilder.DropTable(
                name: "ProductsTags");

            migrationBuilder.DropTable(
                name: "PromotionDisplayRules");

            migrationBuilder.DropTable(
                name: "QuestionarySelectOption");

            migrationBuilder.DropTable(
                name: "CustomerCoupons");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropTable(
                name: "ProductsSuppliers");

            migrationBuilder.DropTable(
                name: "EmailLink");

            migrationBuilder.DropTable(
                name: "QuestionaryAnswerField<List<QuestionarySelectOption>>");

            migrationBuilder.DropTable(
                name: "CustomerBonusTransactions");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "PromotionsType");

            migrationBuilder.DropTable(
                name: "WebProductCategories");

            migrationBuilder.DropTable(
                name: "QuestionaryAnswers");

            migrationBuilder.DropTable(
                name: "QuestionaryField");

            migrationBuilder.DropTable(
                name: "CustomerCards");

            migrationBuilder.DropTable(
                name: "ProductBrands");

            migrationBuilder.DropTable(
                name: "ProductFamilies");

            migrationBuilder.DropTable(
                name: "Questionaries");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "ProductManufacturers");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropTable(
                name: "ProductSegments");

            migrationBuilder.DropTable(
                name: "AccumulationCards");

            migrationBuilder.DropTable(
                name: "ProductDivisions");
        }
    }
}
