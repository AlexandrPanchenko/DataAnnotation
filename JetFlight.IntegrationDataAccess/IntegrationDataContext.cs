using JetFlight.ApplicationDataAccess.DbFunctions;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Shared;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.LogHistory;
using JetFlight.Shared.UserContext;
using HandlebarsDotNet;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System;
using JetFlight.IntegrationDataAccess.Helpers;

namespace JetFlight.IntegrationDataAccess
{
    public class IntegrationDataContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<WebProductCategory> WebProductCategories { get; set; }
        public DbSet<CustomerBonusTransaction> CustomerBonusTransactions { get; set; }
        public DbSet<CustomerCard> CustomerCards { get; set; }
        public DbSet<CustomerDevice> CustomerDevices { get; set; }
        public DbSet<CustomerNotification> CustomerNotifications { get; set; }
        public DbSet<CustomerSetting> CustomerSettings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductDivision> ProductDivisions { get; set; }
        public DbSet<ProductSegment> ProductSegments { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductFamily> ProductFamilies { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<ProductManufacturer> ProductManufacturers { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<ReceiptProduct> ReceiptProducts { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionCategory> PromotionCategories { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        public DbSet<ProductsTag> ProductsTags { get; set; }
        public DbSet<SavedPromotion> SavedPromotions { get; set; }
        public DbSet<PromotionType> PromotionsType { get; set; }
        public DbSet<Questionary> Questionaries { get; set; }
        public DbSet<QuestionaryAnswer> QuestionaryAnswers { get; set; }
        public DbSet<PromotionQueue> PromotionQueues { get; set; }

        public DbSet<ProductQueue> ProductQueues { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponCombinationPriceDiscount> CouponCombinationPriceDiscounts { get; set; }
        public DbSet<CouponCombinationFixedPrice> CouponCombinationPriceFixedPrices { get; set; }
        public DbSet<CouponAdditionalBonus> CouponAdditionalBonuses { get; set; }
        public DbSet<CouponBonusMultiplier> CouponBonusMultipliers { get; set; }
        public DbSet<CouponMultiplierProductActivator> CouponMultiplierProductActivators { get; set; }
        public DbSet<CouponMultiplierBrandActivator> CouponMultiplierBrandActivators { get; set; }
        public DbSet<CouponMultiplierManufacturerActivator> CouponMultiplierManufacturerActivators { get; set; }
        public DbSet<CouponMultiplierCategoryActivator> CouponMultiplierCategoryActivators { get; set; }
        public DbSet<CouponMultiplierSupplierActivator> CouponMultiplierSupplierActivators { get; set; }
        public DbSet<CouponDiscountPercent> CouponDiscountPercents { get; set; }
        public DbSet<CouponDiscountAmount> CouponDiscountAmounts { get; set; }
        public DbSet<CouponProductFixedPrice> CouponProductFixedPrices { get; set; }
        public DbSet<CustomerCoupon> CustomerCoupons { get; set; }
        public DbSet<ProductsSupplier> ProductsSuppliers { get; set; }
        public DbSet<AccumulationCard> AccumulationCards { get; set; }
        public DbSet<CustomerAccumulationCard> CustomerAccumulationCards { get; set; }
        public DbSet<BonusUsage> BonusUsages { get; set; }

        public DbSet<TargetSmsMessage> TargetSmsMessages { get; set; }
        public DbSet<CustomerSmsMessage> CustomerSmsMessages { get; set; }
        public DbSet<TargetEmailMessage> TargetEmailMessages { get; set; }
        public DbSet<CustomerEmailMessage> CustomerEmailMessages { get; set; }
        public DbSet<NotificationHistory> NotificationHistories { get; set; }

        public DbSet<PromotionDisplayRule> PromotionDisplayRules { get; set; }

        public DbSet<LoyaltyProgramRestriction> LoyaltyProgramRestrictions { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<RfmCustomerSnapshot> RfmCustomerSnapshots { get; set; }

        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
        private readonly IBus _bus;
        private readonly IUserContext _userContext;
        public IntegrationDataContext(
            DbContextOptions<IntegrationDataContext> options,
            IBus bus,
            IUserContext userContext)
            : base(options)
        {
            _bus = bus;
            _userContext = userContext;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var databaseUrl = Environment.GetEnvironmentVariable("MSSQL_DATABASE_URL");

            if (!string.IsNullOrEmpty(databaseUrl))
            {
                var uri = new Uri(databaseUrl);
                var userInfo = uri.UserInfo.Split(':');

                var server = uri.Host;
                var port = uri.Port;
                var userId = userInfo[0];
                var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
                var database = uri.AbsolutePath.TrimStart('/');

                optionsBuilder.UseSqlServer(
                   $"Data Source={server},{port};Initial Catalog={database};User Id={userId};Password={password};TrustServerCertificate=True"
                );
            }
            else if (!optionsBuilder.IsConfigured)
            {
                // Fallback-конекшен для локальної розробки / генерації міграцій
                // Можна замінити на свій локальний SQL Server / LocalDB
                optionsBuilder.UseSqlServer(
                    "Server=(localdb)\\mssqllocaldb;Database=JetFlightIntegration_Fake;Trusted_Connection=True;TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes for query performance
                entity.HasIndex(e => e.PhoneNumber);
                entity.HasIndex(e => e.Birthday).HasDatabaseName("IX_Customer_Birthday");

                entity.HasMany(e => e.CustomerDevices).WithOne(e => e.Customer).HasForeignKey(e => e.CustomerId);
                entity.HasMany(e => e.CustomerNotifications).WithOne(e => e.Customer).HasForeignKey(e => e.CustomerId);
                entity.HasMany(e => e.CustomerSettings).WithOne(e => e.Customer).HasForeignKey(e => e.CustomerId);
                entity.HasMany(e => e.CustomerCards).WithOne(e => e.Customer).HasForeignKey(e => e.CustomerId)
                    .IsRequired(false);
            });

            modelBuilder.Entity<Promotion>()
                .HasOne(p => p.PromotionType)
                .WithMany(pt => pt.Promotions)
                .HasForeignKey(p => p.PromotionTypeId)
                .HasPrincipalKey(pt => pt.NavisionId);

            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();

                // Add these relationship configurations:
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductCode)
                      .IsRequired(false);
            });

            modelBuilder.Entity<Promotion>()
                .ToTable(tb => tb.HasTrigger("trg_PromotionUpdated_SaveOldState"));

            modelBuilder.Entity<NotificationHistory>(entity => { entity.HasKey(e => e.Id); });

            modelBuilder.Entity<CustomerDevice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Customer).WithMany(e => e.CustomerDevices).HasForeignKey(e => e.CustomerId);
            });

            modelBuilder.HasDbFunction(() => CustomFunctions.FuzzyContains(default, default))
                .HasName("FuzzyContains")
                .HasSchema("dbo");

            modelBuilder.Entity<CustomerNotification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Customer).WithMany(e => e.CustomerNotifications).HasForeignKey(e => e.CustomerId);
            });

            modelBuilder.Entity<ProductTag>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.ProductsTag)
                    .WithMany(pt => pt.ProductTags)
                    .HasForeignKey(e => e.TagId);
            });

            modelBuilder.Entity<CustomerSetting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Customer).WithMany(e => e.CustomerSettings).HasForeignKey(e => e.CustomerId);
            });

            modelBuilder.Entity<RfmCustomerSnapshot>(entity =>
            {
                entity.HasKey(e => new { e.SnapshotDate, e.CustomerId });

                entity.Property(e => e.SnapshotDate)
                    .HasConversion<DateOnlyConverter>()
                    .HasColumnType("date");

                entity.HasIndex(e => new { e.SnapshotDate, e.RfmId });
                entity.HasIndex(e => new { e.RfmId, e.SnapshotDate });

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CustomerBonusTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.CustomerCard).WithMany(e => e.CustomerBonusTransactions)
                    .HasForeignKey(e => e.CardCode);
            });

            modelBuilder.Entity<CustomerCard>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.HasOne(e => e.Customer).WithMany(e => e.CustomerCards).HasForeignKey(e => e.CustomerId)
                    .IsRequired(false);

                // Index for join performance in analytics queries
                entity.HasIndex(e => e.CustomerId).HasDatabaseName("IX_CustomerCard_CustomerId");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Code);
    
                entity.ToTable(t => t.HasTrigger("TR_Products_ImageChange"));

                entity.HasOne(e => e.Family)
                      .WithMany(g => g.Products)
                      .HasForeignKey(e => e.FamilyCode);
                entity.HasOne(e => e.Family)
                      .WithMany(c => c.Products)
                      .HasForeignKey(e => e.FamilyCode);
                entity.HasOne(e => e.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(x => x.BrandCode);
                entity.HasMany(e => e.ReceiptProducts)
                    .WithOne(x => x.Product)
                    .HasForeignKey(x => x.ProductCode);
            });

            modelBuilder.Entity<ProductQueue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.Code)
                    .HasPrincipalKey(p => p.Code);
            });

            modelBuilder.Entity<ProductFamily>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.HasOne(e => e.Category)
                    .WithMany(e => e.Families)
                    .HasForeignKey(e => e.CategoryCode);
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.HasOne(e => e.Segment)
                    .WithMany(e => e.Categories)
                    .HasForeignKey(e => e.SegmentCode);
            });

            modelBuilder.Entity<ProductSegment>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.HasOne(e => e.Division)
                    .WithMany(e => e.Segments)
                    .HasForeignKey(e => e.DivisionCode);
            });

            modelBuilder.Entity<ProductDivision>(entity =>
            {
                entity.HasKey(e => e.Code);
            });

            modelBuilder.Entity<ProductBrand>(entity => { entity.HasKey(e => e.Code); });

            modelBuilder.Entity<ProductManufacturer>(entity => { entity.HasKey(e => e.Code); });

            modelBuilder.Entity<Receipt>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasMany(x => x.ReceiptProducts)
                    .WithOne(x => x.Receipt)
                    .HasForeignKey(x => x.ReceiptId);
                entity.HasOne(x => x.CustomerCard)
                    .WithMany()
                    .HasForeignKey(x => x.CardCode);

                // Indexes for analytics query performance
                entity.HasIndex(x => x.StoreCode);
                entity.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_Receipt_CreatedAt");
                entity.HasIndex(x => x.BranchId).HasDatabaseName("IX_Receipt_BranchId");
                entity.HasIndex(x => x.CardCode).HasDatabaseName("IX_Receipt_CardCode");

                // Composite index for common filter combinations in analytics
                entity.HasIndex(x => new { x.BranchId, x.CreatedAt }).HasDatabaseName("IX_Receipt_BranchId_CreatedAt");
                entity.HasIndex(x => new { x.StoreCode, x.CreatedAt }).HasDatabaseName("IX_Receipt_StoreCode_CreatedAt");
            });

            modelBuilder.Entity<ReceiptProduct>(entity =>
            {
                entity.HasKey(x => x.Id);
                
                // Ignore ProductCode property to avoid "Invalid column name" error if column doesn't exist
                // Configure relationship using shadow property instead
                entity.Ignore(x => x.ProductCode);
                
                // Configure Product relationship using shadow property for foreign key
                // This works even if ProductCode column doesn't exist in database
                entity.HasOne(x => x.Product)
                    .WithMany(x => x.ReceiptProducts)
                    .HasForeignKey("ProductCode") // Shadow property - not mapped to actual property
                    .HasPrincipalKey(x => x.Code)
                    .IsRequired(false); // Make optional to handle missing column gracefully
                
                entity.HasOne(x => x.Receipt)
                    .WithMany(x => x.ReceiptProducts)
                    .HasForeignKey(x => x.ReceiptId);
            });

            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever(); 
            });

            modelBuilder.Entity<PromotionCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Category)
                    .WithMany()
                    .HasForeignKey(e => e.CategoryCode);
            });


            modelBuilder.Entity<ProductsSupplier>(entity =>
            {
                entity.HasKey(x => x.Code);
                entity.Property(x => x.Code)
                    .HasColumnType("varchar(20)")
                    .IsRequired();
                entity.Property(x => x.Title)
                    .HasColumnType("varchar(150)")
                    .IsRequired();
            });

            modelBuilder.Entity<CouponCombinationPriceDiscount>(entity =>
            {
                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductCode)
                    .HasPrincipalKey(x => x.Code)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Supplier)
                    .WithMany()
                    .HasForeignKey(x => x.SupplierCode)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CouponCombinationFixedPriceActivator>(entity =>
            {
                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductCode)
                    .HasPrincipalKey(x => x.Code)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<CouponCombinationFixedPrice>()
                    .WithMany(x => x.Activators)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CouponCombinationProductActivator>(entity =>
            {
                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductCode)
                    .HasPrincipalKey(x => x.Code)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<CouponCombinationPriceDiscount>()
                    .WithMany(x => x.ProductActivators)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CouponCombinationBrandActivator>(entity =>
            {
                entity.HasOne(x => x.Brand)
                    .WithMany()
                    .HasForeignKey(x => x.BrandCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<CouponCombinationPriceDiscount>()
                    .WithMany(x => x.BrandActivators)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CouponCombinationCategoryActivator>(entity =>
            {
                entity.HasOne(x => x.Category)
                    .WithMany()
                    .HasForeignKey(x => x.CategoryCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<CouponCombinationPriceDiscount>()
                    .WithMany(x => x.CategoryActivators)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CouponCombinationSupplierActivator>(entity =>
            {
                entity.HasOne(x => x.Supplier)
                    .WithMany()
                    .HasForeignKey(x => x.SupplierCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<CouponCombinationPriceDiscount>()
                    .WithMany(x => x.SupplierActivators)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(x => x.SupplierCode)
                    .HasColumnType("varchar(20)")
                    .IsRequired();
            });

            modelBuilder.Entity<CouponCombinationManufacturerActivator>(entity =>
            {
                entity.HasOne(x => x.Manufacturer)
                    .WithMany()
                    .HasForeignKey(x => x.ManufacturerCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<CouponCombinationPriceDiscount>()
                    .WithMany(x => x.ManufacturerActivators)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuration for new CouponMultiplier activator entities
            modelBuilder.Entity<CouponMultiplierProductActivator>(entity =>
            {
                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductCode)
                    .HasPrincipalKey(x => x.Code)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CouponBonusMultiplier)
                    .WithMany(x => x.ProductActivators)
                    .HasForeignKey(x => x.CouponBonusMultiplierId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CouponMultiplierBrandActivator>(entity =>
            {
                entity.HasOne(x => x.Brand)
                    .WithMany()
                    .HasForeignKey(x => x.BrandCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CouponBonusMultiplier)
                    .WithMany(x => x.BrandActivators)
                    .HasForeignKey(x => x.CouponBonusMultiplierId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CouponMultiplierCategoryActivator>(entity =>
            {
                entity.HasOne(x => x.Category)
                    .WithMany()
                    .HasForeignKey(x => x.CategoryCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CouponBonusMultiplier)
                    .WithMany(x => x.CategoryActivators)
                    .HasForeignKey(x => x.CouponBonusMultiplierId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CouponMultiplierSupplierActivator>(entity =>
            {
                entity.HasOne(x => x.Supplier)
                    .WithMany()
                    .HasForeignKey(x => x.SupplierCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CouponBonusMultiplier)
                    .WithMany(x => x.SupplierActivators)
                    .HasForeignKey(x => x.CouponBonusMultiplierId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(x => x.SupplierCode)
                    .HasColumnType("varchar(20)")
                    .IsRequired();
            });

            modelBuilder.Entity<CouponMultiplierManufacturerActivator>(entity =>
            {
                entity.HasOne(x => x.Manufacturer)
                    .WithMany()
                    .HasForeignKey(x => x.ManufacturerCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CouponBonusMultiplier)
                    .WithMany(x => x.ManufacturerActivators)
                    .HasForeignKey(x => x.CouponBonusMultiplierId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //modelBuilder.Entity<CouponBonusMultiplier>(entity =>
            //{
            //    entity.HasOne(x => x.Product)
            //        .WithMany()
            //        .HasForeignKey(x => x.ProductCode)
            //        .HasPrincipalKey(x => x.Code)
            //        .IsRequired(false)
            //        .OnDelete(DeleteBehavior.Restrict);
            //});

            modelBuilder.Entity<CouponProductFixedPrice>(entity =>
            {
                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductCode)
                    .HasPrincipalKey(x => x.Code)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<LoyaltyProgramRestriction>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Division)
                    .WithMany()
                    .HasForeignKey(x => x.DivisionCode)
                    .IsRequired(false);

                entity.HasOne(x => x.Segment)
                    .WithMany()
                    .HasForeignKey(x => x.SegmentCode)
                    .IsRequired(false);

                entity.HasOne(x => x.Category)
                    .WithMany()
                    .HasForeignKey(x => x.CategoryCode)
                    .IsRequired(false);

                entity.HasOne(x => x.Family)
                    .WithMany()
                    .HasForeignKey(x => x.FamilyCode)
                    .IsRequired(false);

                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductCode)
                    .IsRequired(false);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(x => x.Id);

                // Foreign key relationship to Customer with cascade delete
                entity.HasOne(x => x.Customer)
                    .WithMany()
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index on token hash for fast lookups (unique for active tokens)
                entity.HasIndex(x => x.TokenHash)
                    .IsUnique()
                    .HasDatabaseName("IX_RefreshTokens_TokenHash");

                // Composite index for cleanup queries: find expired/revoked tokens by customer and branch
                entity.HasIndex(x => new { x.CustomerId, x.IsRevoked, x.ExpiresAt })
                    .HasDatabaseName("IX_RefreshTokens_CustomerId_IsRevoked_ExpiresAt");

                // Index on TokenFamily for token reuse attack detection
                entity.HasIndex(x => x.TokenFamily)
                    .HasDatabaseName("IX_RefreshTokens_TokenFamily");

                entity.HasIndex(x => x.TokenFamily)
                    .HasDatabaseName("IX_RefreshTokens_TokenFamily_Active")
                    .IsUnique()
                    .HasFilter("[IsRevoked] = 0");

                // Index on ExpiresAt for background cleanup jobs
                entity.HasIndex(x => new { x.ExpiresAt, x.IsRevoked })
                    .HasDatabaseName("IX_RefreshTokens_ExpiresAt_IsRevoked");

                // Column configurations
                entity.Property(x => x.TokenHash)
                    .IsRequired()
                    .HasMaxLength(64); // SHA256 produces 64 hex characters

                entity.Property(x => x.BranchId)
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.Property(x => x.ExpiresAt)
                    .IsRequired();

                entity.Property(x => x.IsRevoked)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(x => x.TokenFamily)
                    .IsRequired();

                entity.Property(x => x.IpAddress)
                    .HasMaxLength(45); // IPv6 max length

                entity.Property(x => x.UserAgent)
                    .HasMaxLength(500);

                entity.Property(x => x.RevokedReason)
                    .HasMaxLength(200);

                entity.Property(x => x.ReplacedByTokenHash)
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<EmailVerificationToken>(entity =>
            {
                entity.HasKey(x => x.Id);

                // Foreign key relationship to Customer with cascade delete
                entity.HasOne(x => x.Customer)
                    .WithMany()
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index on CustomerId for fast lookups
                entity.HasIndex(x => x.CustomerId)
                    .HasDatabaseName("IX_EmailVerificationTokens_CustomerId");

                // Index on Token for fast lookups
                entity.HasIndex(x => x.Token)
                    .HasDatabaseName("IX_EmailVerificationTokens_Token");
            });

            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.SetKindUtc(),
                v => v.SetKindUtc());

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.SetKindUtc() : v,
                v => v.HasValue ? v.SetKindUtc() : v);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
        }

        public async Task<List<LogMessage>> SaveChangesAsyncWithLogHistory(CancellationToken cancellationToken = new CancellationToken())
        {
            // Set CreatedAt and UpdatedAt for CustomerCard entities
            var now = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries<CustomerCard>())
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity.CreatedAt == null)
                    {
                        entry.Entity.CreatedAt = now;
                    }
                    if (entry.Entity.UpdatedAt == null)
                    {
                        entry.Entity.UpdatedAt = now;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }

            var events = new List<LogMessage>();

            try
            {
                events = await GenerateLogHistoryEvents();
                await base.SaveChangesAsync(cancellationToken);

                return events;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken, bool ignoreLogs)
        {
            // Set CreatedAt and UpdatedAt for CustomerCard entities
            var now = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries<CustomerCard>())
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity.CreatedAt == null)
                    {
                        entry.Entity.CreatedAt = now;
                    }
                    if (entry.Entity.UpdatedAt == null)
                    {
                        entry.Entity.UpdatedAt = now;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }

            if (!ignoreLogs)
            {
                var events = new List<LogMessage>();

                try
                {
                    events = await GenerateLogHistoryEvents();
                    var result = await base.SaveChangesAsync(cancellationToken);

                    if (events.Any())
                    {
                        var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"queue:{NotificationConstant.LogHistoryQueue}"));
                        await sendEndpoint.Send(new LogHistoryMessage()
                        {
                            Logs = events,
                        }, cancellationToken);
                    }

                    return result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return SaveChangesAsync(cancellationToken, false);
        }

        private async Task<List<LogMessage>> GenerateLogHistoryEvents()
        {
            var events = new List<LogMessage>();
            var currentUserId = _userContext.AdminId;
            var trackedEntities = ChangeTracker.Entries();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (ShouldAudit(entry, currentUserId))
                {
                    var auditEvent = await CreateAuditEventAsync(entry, currentUserId);

                    if (auditEvent != null)
                    {
                        events.Add(auditEvent);
                    }
                }
            }

            return events;
        }

        private bool ShouldAudit(EntityEntry entry, int? adminId)
        {
            if ((entry.State == EntityState.Unchanged && entry.Entity is not IRelatedAuditable)
                || entry.State == EntityState.Detached || entry.Entity is not IAuditable
                || adminId == null)
            {
                return false;
            }

            if (entry.Entity is Coupon { IsCardCoupon: true })
            {
                return false;
            }

            return true;
        }

        private async Task<LogMessage> CreateAuditEventAsync(EntityEntry entry, int? userId)
        {
            var tableName = entry.Metadata.GetTableName();
            var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString();

            var operation = entry.State switch
            {
                EntityState.Added => "Inserted",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                EntityState.Unchanged => "Updated",
                _ => throw new InvalidOperationException($"Unsupported entity state: {entry.State}")
            };

            int? entityId = GetEntityId(primaryKey, tableName);
            var logHistory = new LogMessage
            {
                AdminId = userId,
                EntityType = tableName,
                EntityId = entityId,
                Action = operation,
                Date = DateTime.UtcNow
            };

            var (originalValues, currentValues) = GetValues(entry);
            logHistory.UpdatedFrom = JsonConvert.SerializeObject(originalValues);
            logHistory.UpdatedTo = JsonConvert.SerializeObject(currentValues);

            return logHistory;
        }

        private (Dictionary<string, object>? OriginalValues, Dictionary<string, object>? CurrentValues) GetValues(EntityEntry entry)
        {
            return entry.State switch
            {
                EntityState.Added => GetValuesForAddedState(entry),
                EntityState.Modified or EntityState.Unchanged => GetValuesForModifiedState(entry),
                EntityState.Deleted => GetValuesForDeletedState(entry),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private (Dictionary<string, object>? OriginalValues, Dictionary<string, object>? CurrentValues) GetValuesForAddedState(EntityEntry entry)
        {
            Dictionary<string, object> currentValues = new Dictionary<string, object>();

            foreach (var property in entry.Properties)
            {
                currentValues[property.Metadata.Name] = property.CurrentValue;
            }

            return (null, currentValues);
        }

        private (Dictionary<string, object>? OriginalValues, Dictionary<string, object>? CurrentValues) GetValuesForDeletedState(EntityEntry entry)
        {
            Dictionary<string, object> originalValues = new Dictionary<string, object>();

            foreach (var property in entry.Properties)
            {
                originalValues[property.Metadata.Name] = property.OriginalValue;
            }

            return (originalValues, null);
        }

        private (Dictionary<string, object> OriginalValues, Dictionary<string, object> CurrentValues) GetValuesForModifiedState(EntityEntry entry)
        {
            Dictionary<string, object> originalValues = new Dictionary<string, object>();
            Dictionary<string, object> currentValues = new Dictionary<string, object>();

            foreach (var property in entry.Properties)
            {
                if (Equals(property.OriginalValue, property.CurrentValue) || !property.IsModified)
                {
                    continue;
                }

                originalValues[property.Metadata.Name] = property.OriginalValue;
                currentValues[property.Metadata.Name] = property.CurrentValue;
            }

            if (entry.Entity is Coupon coupon)
            {
                // Targets
                var allTargetsEntries = ChangeTracker.Entries<CouponToTarget>()
                    .Where(x => x.Entity.CouponId == coupon.Id)
                    .ToList();

                if (allTargetsEntries.Any(x => x.State is EntityState.Added or EntityState.Deleted))
                {
                    var currentTargets = allTargetsEntries
                        .Where(x => x.State != EntityState.Deleted)
                        .Select(x => new { x.Entity.TargetId });
                    
                    var originalTargets = allTargetsEntries
                        .Where(x => x.State != EntityState.Added)
                        .Select(x => new { x.Entity.TargetId });

                    currentValues[nameof(Entities.CouponToTarget)] = currentTargets;

                    originalValues[nameof(Entities.CouponToTarget)] = originalTargets;
                }

                // Stores
                var allStoreEntries = ChangeTracker.Entries<CouponToStore>()
                    .Where(x => x.Entity.CouponId == coupon.Id)
                    .ToList();

                if (allStoreEntries.Any(x => x.State is EntityState.Added or EntityState.Deleted))
                {
                    var currentTargets = allStoreEntries
                        .Where(x => x.State != EntityState.Deleted)
                        .Select(x => new { x.Entity.StoreCode });

                    var originalTargets = allStoreEntries
                        .Where(x => x.State != EntityState.Added)
                        .Select(x => new { x.Entity.StoreCode });

                    currentValues[nameof(Entities.CouponToStore)] = currentTargets;

                    originalValues[nameof(Entities.CouponToStore)] = originalTargets;
                }
            }
            else if (entry.Entity is AccumulationCard accumulationCard)
            {
                // Targets
                var allTargetsEntries = ChangeTracker.Entries<AccumulationCardToTarget>()
                    .Where(x => x.Entity.AccumulationCardId == accumulationCard.Id)
                    .ToList();

                if (allTargetsEntries.Any(x => x.State is EntityState.Added or EntityState.Deleted))
                {
                    var currentTargets = allTargetsEntries
                        .Where(x => x.State != EntityState.Deleted)
                        .Select(x => new { x.Entity.TargetId });

                    var originalTargets = allTargetsEntries
                        .Where(x => x.State != EntityState.Added)
                        .Select(x => new { x.Entity.TargetId });

                    currentValues[nameof(Entities.AccumulationCardToTarget)] = currentTargets;

                    originalValues[nameof(Entities.AccumulationCardToTarget)] = originalTargets;
                }

                // Coupons
                var allCoupons = ChangeTracker.Entries<Coupon>()
                    .Where(x => x.Entity.AccumulationCardId == accumulationCard.Id
                                || (int?)x.Property(nameof(Coupon.AccumulationCardId)).OriginalValue == accumulationCard.Id)
                    .ToList();

                if (allCoupons.Any(x => x.State is EntityState.Added) && allCoupons.Any(x => x.State is EntityState.Deleted))
                {
                    var originalCoupon = allCoupons.First(x => x.State == EntityState.Deleted).Entity;
                    var currentCoupon = allCoupons.First(x => x.State == EntityState.Added).Entity;

                    if (originalCoupon.Description != currentCoupon.Description)
                    {
                        originalValues[nameof(Shared.Models.LogHistory.AccumulationCardLogHistoryDTO.CouponDescription)] = originalCoupon.Description;
                        currentValues[nameof(Shared.Models.LogHistory.AccumulationCardLogHistoryDTO.CouponDescription)] = currentCoupon.Description;
                    }

                    if (originalCoupon.Image != currentCoupon.Image)
                    {
                        originalValues[nameof(Shared.Models.LogHistory.AccumulationCardLogHistoryDTO.Image)] = originalCoupon.Image;
                        currentValues[nameof(Shared.Models.LogHistory.AccumulationCardLogHistoryDTO.Image)] = currentCoupon.Image;
                    }

                    if (originalCoupon.StartDate != currentCoupon.StartDate)
                    {
                        originalValues[nameof(Shared.Models.LogHistory.AccumulationCardLogHistoryDTO.StartDate)] = originalCoupon.StartDate;
                        currentValues[nameof(Shared.Models.LogHistory.AccumulationCardLogHistoryDTO.StartDate)] = currentCoupon.StartDate;
                    }

                    if (originalCoupon.ExpirationDate != currentCoupon.ExpirationDate)
                    {
                        originalValues[nameof(Shared.Models.LogHistory.AccumulationCardLogHistoryDTO.ExpirationDate)] = originalCoupon.ExpirationDate;
                        currentValues[nameof(Shared.Models.LogHistory.AccumulationCardLogHistoryDTO.ExpirationDate)] = currentCoupon.ExpirationDate;
                    }
                }
            }

            return (originalValues, currentValues);
        }

        private int? GetEntityId(string primaryKey, string tableName)
        {
            int? entityId = null;
            if (!string.IsNullOrEmpty(primaryKey) && int.TryParse(primaryKey, out var id))
            {
                entityId = id;
            }

            return entityId;
        }
    }
}
