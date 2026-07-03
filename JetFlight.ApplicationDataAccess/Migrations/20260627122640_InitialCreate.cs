using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JetFlight.ApplicationDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    Salt = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    IsSuperadmin = table.Column<bool>(type: "boolean", nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByAdminId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_Admins_CreatedByAdminId",
                        column: x => x.CreatedByAdminId,
                        principalTable: "Admins",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MimeType = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Size = table.Column<string>(type: "text", nullable: true),
                    Width = table.Column<string>(type: "text", nullable: true),
                    Height = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Page",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchId = table.Column<byte>(type: "smallint", nullable: true),
                    Published = table.Column<bool>(type: "boolean", nullable: true),
                    OriginId = table.Column<int>(type: "integer", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Link = table.Column<string>(type: "text", nullable: true),
                    RootPage = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledPublishDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Page", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Page_Page_OriginId",
                        column: x => x.OriginId,
                        principalTable: "Page",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Page_Page_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Page",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    OriginId = table.Column<int>(type: "integer", nullable: true),
                    Subtitle = table.Column<string>(type: "text", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Published = table.Column<bool>(type: "boolean", nullable: true),
                    Status = table.Column<bool>(type: "boolean", nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: false),
                    ImageMimeType = table.Column<string>(type: "text", nullable: true),
                    ImageName = table.Column<string>(type: "text", nullable: true),
                    ImageSize = table.Column<string>(type: "text", nullable: true),
                    ImageAlt = table.Column<string>(type: "text", nullable: true),
                    ReadDurationMin = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BranchId = table.Column<byte>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Posts_OriginId",
                        column: x => x.OriginId,
                        principalTable: "Posts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PostsTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostsTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Range<int>",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    From = table.Column<int>(type: "integer", nullable: false),
                    To = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Range<int>", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolesPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityType = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Crud = table.Column<byte>(type: "smallint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeoMeta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityType = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Keywords = table.Column<string>(type: "text", nullable: true),
                    CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeoMeta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchId = table.Column<byte>(type: "smallint", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscription",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    UnsubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Targets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchId = table.Column<byte>(type: "smallint", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AgeFrom = table.Column<int>(type: "integer", nullable: true),
                    AgeTo = table.Column<int>(type: "integer", nullable: true),
                    DayOfBirthFrom = table.Column<int>(type: "integer", nullable: true),
                    DayOfBirthTo = table.Column<int>(type: "integer", nullable: true),
                    MonthOfBirthFrom = table.Column<int>(type: "integer", nullable: true),
                    MonthOfBirthTo = table.Column<int>(type: "integer", nullable: true),
                    YearOfBirthFrom = table.Column<int>(type: "integer", nullable: true),
                    YearOfBirthTo = table.Column<int>(type: "integer", nullable: true),
                    ShoppingPeriodTo = table.Column<int>(type: "integer", nullable: true),
                    ShoppingPeriodFrom = table.Column<int>(type: "integer", nullable: true),
                    AverageCheckPositionsFrom = table.Column<int>(type: "integer", nullable: true),
                    AverageCheckPositionsTo = table.Column<int>(type: "integer", nullable: true),
                    AverageCheckAmountFrom = table.Column<decimal>(type: "numeric", nullable: true),
                    AverageCheckAmountTo = table.Column<decimal>(type: "numeric", nullable: true),
                    ShoppingTimeFrom = table.Column<TimeSpan>(type: "interval", nullable: true),
                    ShoppingTimeTo = table.Column<TimeSpan>(type: "interval", nullable: true),
                    FrequencyType = table.Column<int>(type: "integer", nullable: true),
                    AverageFrequencyTimes = table.Column<int>(type: "integer", nullable: true),
                    RegisteredDateFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegisteredDateTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Period = table.Column<int>(type: "integer", nullable: true),
                    CheckCountFrom = table.Column<int>(type: "integer", nullable: true),
                    CheckCountTo = table.Column<int>(type: "integer", nullable: true),
                    IncludeOnBirthdayOnly = table.Column<bool>(type: "boolean", nullable: false),
                    RegisteredDaysFrom = table.Column<int>(type: "integer", nullable: true),
                    RegisteredDaysTo = table.Column<int>(type: "integer", nullable: true),
                    ProductAmountFrom = table.Column<decimal>(type: "numeric", nullable: true),
                    ProductAmountTo = table.Column<decimal>(type: "numeric", nullable: true),
                    CategoryCodes = table.Column<List<string>>(type: "text[]", nullable: true),
                    ManufacturerCodes = table.Column<List<string>>(type: "text[]", nullable: true),
                    Sex = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Targets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Volunteers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Volunteers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminRoles_AdminRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AdminRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdminRoles_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogsHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdminId = table.Column<int>(type: "integer", nullable: true),
                    EntityType = table.Column<string>(type: "text", nullable: true),
                    UpdatedFrom = table.Column<string>(type: "text", nullable: true),
                    UpdatedTo = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<int>(type: "integer", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogsHistory_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ResetPassword",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AuthCode = table.Column<string>(type: "text", nullable: true),
                    AdminId = table.Column<int>(type: "integer", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResetPassword", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResetPassword_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<string>(type: "text", nullable: true),
                    Link = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Latitude = table.Column<string>(type: "text", nullable: true),
                    Longitude = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Address2 = table.Column<string>(type: "text", nullable: true),
                    Region = table.Column<string>(type: "text", nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: false),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    MediaFileId = table.Column<int>(type: "integer", nullable: true),
                    BranchId = table.Column<byte>(type: "smallint", nullable: true),
                    MapLink = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stores_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Stores_MediaFiles_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "MediaFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PageId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    IsHtml = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OriginId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Page_PageId",
                        column: x => x.PageId,
                        principalTable: "Page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sections_Sections_OriginId",
                        column: x => x.OriginId,
                        principalTable: "Sections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PostTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostTag_PostsTags_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PostsTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostTag_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RFMs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Color = table.Column<string>(type: "text", nullable: true),
                    PeriodId = table.Column<int>(type: "integer", nullable: true),
                    AmountId = table.Column<int>(type: "integer", nullable: true),
                    CountId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFMs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RFMs_Range<int>_AmountId",
                        column: x => x.AmountId,
                        principalTable: "Range<int>",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RFMs_Range<int>_CountId",
                        column: x => x.CountId,
                        principalTable: "Range<int>",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RFMs_Range<int>_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "Range<int>",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    PermissionsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermission_AdminRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AdminRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermission_RolesPermissions_PermissionsId",
                        column: x => x.PermissionsId,
                        principalTable: "RolesPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TargetToCity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetToCity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TargetToCity_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TargetToCity_Targets_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Targets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactUs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    TopicId = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssigneeId = table.Column<int>(type: "integer", nullable: true),
                    ResolveMessage = table.Column<string>(type: "text", nullable: true),
                    ResolveSignature = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<byte>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactUs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactUs_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<byte>(type: "smallint", nullable: false),
                    StoreId = table.Column<int>(type: "integer", nullable: true),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<byte>(type: "smallint", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    ResolveMessage = table.Column<string>(type: "text", nullable: true),
                    ResolveSignature = table.Column<string>(type: "text", nullable: true),
                    AssigneeId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProcessingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CustomerCouponIds = table.Column<List<int>>(type: "integer[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Admins_AssigneeId",
                        column: x => x.AssigneeId,
                        principalTable: "Admins",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Feedbacks_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkingHours",
                columns: table => new
                {
                    WorkingHoursId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Day = table.Column<int>(type: "integer", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OpeningTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ClosingTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    StoreId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingHours", x => x.WorkingHoursId);
                    table.ForeignKey(
                        name: "FK_WorkingHours_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SectionField",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SectionId = table.Column<int>(type: "integer", nullable: true),
                    MediaFilesId = table.Column<int>(type: "integer", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    RelatedTitle = table.Column<string>(type: "text", nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: true),
                    Extendable = table.Column<bool>(type: "boolean", nullable: true),
                    SubSectionTitle = table.Column<string>(type: "text", nullable: true),
                    Placeholder = table.Column<string>(type: "text", nullable: true),
                    Dimensions = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionField", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionField_MediaFiles_MediaFilesId",
                        column: x => x.MediaFilesId,
                        principalTable: "MediaFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SectionField_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TargetToRFM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RFMId = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetToRFM", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TargetToRFM_RFMs_RFMId",
                        column: x => x.RFMId,
                        principalTable: "RFMs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TargetToRFM_Targets_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Targets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactUsAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContactUsId = table.Column<int>(type: "integer", nullable: true),
                    MimeType = table.Column<string>(type: "text", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    FilePath = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactUsAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactUsAttachments_ContactUs_ContactUsId",
                        column: x => x.ContactUsId,
                        principalTable: "ContactUs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeedbacksAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FeedbackId = table.Column<int>(type: "integer", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: true),
                    FilePath = table.Column<string>(type: "text", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbacksAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbacksAttachments_Feedbacks_FeedbackId",
                        column: x => x.FeedbackId,
                        principalTable: "Feedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminRole_Id",
                table: "AdminRole",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminRoles_AdminId",
                table: "AdminRoles",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminRoles_Id",
                table: "AdminRoles",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminRoles_RoleId",
                table: "AdminRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_CreatedByAdminId",
                table: "Admins",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Id",
                table: "Admins",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Id",
                table: "Cities",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactUs_Id",
                table: "ContactUs",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactUs_TopicId",
                table: "ContactUs",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactUsAttachments_ContactUsId",
                table: "ContactUsAttachments",
                column: "ContactUsId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactUsAttachments_Id",
                table: "ContactUsAttachments",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_AssigneeId",
                table: "Feedbacks",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_Id",
                table: "Feedbacks",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_StoreId",
                table: "Feedbacks",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbacksAttachments_FeedbackId",
                table: "FeedbacksAttachments",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbacksAttachments_Id",
                table: "FeedbacksAttachments",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogsHistory_AdminId",
                table: "LogsHistory",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_LogsHistory_Id",
                table: "LogsHistory",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Id",
                table: "MediaFiles",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Page_Id",
                table: "Page",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Page_OriginId",
                table: "Page",
                column: "OriginId");

            migrationBuilder.CreateIndex(
                name: "IX_Page_ParentId",
                table: "Page",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_Id",
                table: "Partners",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Id",
                table: "Posts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_OriginId",
                table: "Posts",
                column: "OriginId");

            migrationBuilder.CreateIndex(
                name: "IX_PostsTags_Id",
                table: "PostsTags",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostTag_CategoryId",
                table: "PostTag",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PostTag_Id",
                table: "PostTag",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostTag_PostId",
                table: "PostTag",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_ResetPassword_AdminId",
                table: "ResetPassword",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ResetPassword_Id",
                table: "ResetPassword",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RFMs_AmountId",
                table: "RFMs",
                column: "AmountId");

            migrationBuilder.CreateIndex(
                name: "IX_RFMs_CountId",
                table: "RFMs",
                column: "CountId");

            migrationBuilder.CreateIndex(
                name: "IX_RFMs_PeriodId",
                table: "RFMs",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_Id",
                table: "RolePermission",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_PermissionsId",
                table: "RolePermission",
                column: "PermissionsId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_RoleId",
                table: "RolePermission",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermissions_Id",
                table: "RolesPermissions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionField_Id",
                table: "SectionField",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionField_MediaFilesId",
                table: "SectionField",
                column: "MediaFilesId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionField_SectionId",
                table: "SectionField",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_Id",
                table: "Sections",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sections_OriginId",
                table: "Sections",
                column: "OriginId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_PageId",
                table: "Sections",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_SeoMeta_Id",
                table: "SeoMeta",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CityId",
                table: "Stores",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Id",
                table: "Stores",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_MediaFileId",
                table: "Stores",
                column: "MediaFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_Id",
                table: "Subscription",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TargetToCity_CityId",
                table: "TargetToCity",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetToCity_TargetId",
                table: "TargetToCity",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetToRFM_RFMId",
                table: "TargetToRFM",
                column: "RFMId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetToRFM_TargetId",
                table: "TargetToRFM",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_Id",
                table: "Topics",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Volunteers_Id",
                table: "Volunteers",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkingHours_StoreId",
                table: "WorkingHours",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkingHours_WorkingHoursId",
                table: "WorkingHours",
                column: "WorkingHoursId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminRoles");

            migrationBuilder.DropTable(
                name: "ContactUsAttachments");

            migrationBuilder.DropTable(
                name: "FeedbacksAttachments");

            migrationBuilder.DropTable(
                name: "LogsHistory");

            migrationBuilder.DropTable(
                name: "Partners");

            migrationBuilder.DropTable(
                name: "PostTag");

            migrationBuilder.DropTable(
                name: "ResetPassword");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "SectionField");

            migrationBuilder.DropTable(
                name: "SeoMeta");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "Subscription");

            migrationBuilder.DropTable(
                name: "TargetToCity");

            migrationBuilder.DropTable(
                name: "TargetToRFM");

            migrationBuilder.DropTable(
                name: "Volunteers");

            migrationBuilder.DropTable(
                name: "WorkingHours");

            migrationBuilder.DropTable(
                name: "ContactUs");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "PostsTags");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "AdminRole");

            migrationBuilder.DropTable(
                name: "RolesPermissions");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "RFMs");

            migrationBuilder.DropTable(
                name: "Targets");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "Page");

            migrationBuilder.DropTable(
                name: "Range<int>");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "MediaFiles");
        }
    }
}
