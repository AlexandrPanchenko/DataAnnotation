using JetFlight.ApplicationDataAccess.Entities.DataContext;

namespace JetFlight.ApplicationDataAccess;
using System.Text.Json;
using JetFlight.ApplicationDataAccess.DbFunctions;
using JetFlight.Shared.Constants;
using Microsoft.EntityFrameworkCore;
public class ApplicationDataContext : DbContext
{

    /// <summary>
    /// Authorization data
    /// </summary>

    public DbSet<LogHistory> LogsHistory { get; set; }
    public DbSet<RoleToPermission> RolePermission { get; set; }
    public DbSet<Volunteer> Volunteers { get; set; }
    public DbSet<ContactUs> ContactUs { get; set; }
    public DbSet<ContactUsAttachment> ContactUsAttachments { get; set; }
    public DbSet<SeoMeta> SeoMeta { get; set; }
    public DbSet<Page> Page { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostTag> PostsTags { get; set; }
    public DbSet<PostToTag> PostTag { get; set; }
    public DbSet<SiteSettings> SiteSettings { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<RolesPermission> RolesPermissions { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<SectionField> SectionField { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<FeedbackAttachment> FeedbacksAttachments { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Partner> Partners { get; set; }
    public DbSet<MediaFiles> MediaFiles { get; set; }
    public DbSet<AdminToRole> AdminRoles { get; set; }
    public DbSet<AdminRole> AdminRole { get; set; }
    public DbSet<Subscription> Subscription { get; set; }
    public DbSet<ResetPassword> ResetPassword { get; set; }
    public DbSet<City> Cities { get; set; }

    public DbSet<Store> Stores { get; set; }
    public DbSet<WorkingHours> WorkingHours { get; set; }
    public DbSet<RFM> RFMs { get; set; }
    public DbSet<Target> Targets { get; set; }



    /// Main information
    /// </summary>

    public ApplicationDataContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');

            var server = uri.Host;
            var port = uri.Port;
            var userId = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
            var database = uri.AbsolutePath.TrimStart('/');

            optionsBuilder.UseNpgsql(
                $"Server={server};Port={port};User Id={userId};Password={password};Database={database};sslmode=Prefer;Trust Server Certificate=true"
            );
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.General);


        modelBuilder
            .Entity<WorkingHours>()
            .HasIndex(x => x.WorkingHoursId)
            .IsUnique();

        modelBuilder
            .Entity<LogHistory>()
            .HasIndex(x => x.Id)
            .IsUnique();


        modelBuilder
            .Entity<City>()
            .HasIndex(x => x.Id)
            .IsUnique();


        modelBuilder
            .Entity<Store>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<RoleToPermission>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<RoleToPermission>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<Volunteer>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<ContactUs>()
            .HasIndex(x => x.Id)
            .IsUnique();


        modelBuilder
            .Entity<ContactUsAttachment>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<SeoMeta>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<Post>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<PostTag>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<PostToTag>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<RolesPermission>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasIndex(x => x.Id).IsUnique();

            entity.HasMany(x => x.SectionFields)
                .WithOne(x => x.Section)
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder
            .Entity<SectionField>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<Feedback>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<FeedbackAttachment>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<Topic>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<Partner>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<MediaFiles>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<AdminToRole>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<AdminRole>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<Subscription>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<Admin>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<ResetPassword>()
            .HasIndex(x => x.Id)
            .IsUnique();

        modelBuilder
            .Entity<SiteSettings>()
            .Property(x => x.Key)
            .HasConversion(
                x => x.ToString(),
                x => Enum.Parse<SiteSettingsKeys>(x));

        modelBuilder.Entity<RFM>()
            .HasKey(x => x.Id);
        
        modelBuilder.Entity<Target>()
            .HasKey(x => x.Id);

        modelBuilder.HasDbFunction(() => CustomFunctions.FuzzyContains(default, default))
            .HasName("fuzzy_contains")
            .HasSchema("public");

        modelBuilder.HasDbFunction(() => CustomFunctions.CalculateDistance(default, default, default, default))
            .HasName(nameof(CustomFunctions.CalculateDistance));

        modelBuilder.Entity<Page>(entity =>
        {
            entity.HasIndex(x => x.Id).IsUnique();

            entity.HasMany(x => x.Sections)
                .WithOne(x => x.Page)
                .HasForeignKey(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId);

            entity.HasOne(x => x.Origin)
                .WithMany(x => x.Versions)
                .HasForeignKey(x => x.OriginId);
        });
    }
}