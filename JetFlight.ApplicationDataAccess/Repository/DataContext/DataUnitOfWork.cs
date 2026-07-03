using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared.UserContext;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public class DataUnitOfWork : IDataUnitOfWork
{
    private readonly ApplicationDataContext _dbContext;

    private readonly IUserContext _userContext;

    public IAdminRepository Admins { get; }
    public IResetPasswordRepository ResetPasswords { get; }
    public ILogHistoryRepository LogsHistory { get; }
    public IPageRepository Pages { get; }
    public IRoleRepository Roles { get; }
    public IPostRepository Posts { get; }
    public IStoreRepository Stores { get; }
    public IWorkingHoursRepository WorkingHours { get; }
    public ICityRepository Cities { get; }

    public IAdminToRoleRepository AdminRole { get; }

    public IRolePolicyRepository RolePolicy { get; }
    public ISiteSettingsRepository SiteSettings { get; }

    public IRolesPermissionRepository RolesPermission { get; }
    public IPostToTagRepository PostToTags { get; }
    public ITagRepository Tags { get; }

    public ISectionsRepository Sections { get; }
    public IMediaFilesRepository MediaFiles { get; }

    public ISectionFieldRepository SectionFields { get; }
    public IContactUsRepository ContactUs { get; }
    public IContactUsAttachmentRepository ContactUsAttachments { get; }
    public ITopicRepository Topic { get; }
    public ISeoMetaRepository SeoMeta { get; }
    public IGenericDataRepository<Feedback> Feedbacks { get; }
    public IGenericDataRepository<RFM> RFMs { get; }
    public IGenericDataRepository<Target> Targets { get; }

    public DataUnitOfWork(ApplicationDataContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;

        Admins = new AdminRepository(_dbContext);
        ResetPasswords = new ResetPasswordRepository(_dbContext);
        LogsHistory = new LogHistoryRepository(_dbContext);
        Pages = new PageRepository(_dbContext);
        Roles = new RoleRepository(_dbContext);
        AdminRole = new AdminToRoleRepository(_dbContext);
        RolePolicy = new RolePolicyRepository(_dbContext);
        RolesPermission = new RolesPermissionRepository(_dbContext);
        SiteSettings = new SiteSettingsRepository(_dbContext);
        Posts = new PostRepository(_dbContext);
        PostToTags = new PostToTagRepository(_dbContext);
        Tags = new TagRepository(_dbContext);
        Sections = new SectionsRepository(_dbContext);
        SectionFields = new SectionFieldRepository(_dbContext);
        Stores = new StoreRepository(_dbContext);
        Cities = new CityRepository(_dbContext);
        ContactUs = new ContactUsRepository(_dbContext);
        MediaFiles = new MediaFilesRepository(_dbContext);
        Topic = new TopicRepository(_dbContext);
        SeoMeta = new SeoMetaRepository(_dbContext);
        Feedbacks = new DataGenericRepository<Feedback>(_dbContext);
        RFMs = new DataGenericRepository<RFM>(_dbContext);
        Targets = new DataGenericRepository<Target>(_dbContext);
        ContactUsAttachments = new ContactUsAttachmentRepository(_dbContext);
    }

    public async Task<int> Save(bool? skipLogHistory = false)
    {
        if (skipLogHistory == true)
        {
            return await _dbContext.SaveChangesAsync();
        }

        var actionBy = _userContext.AdminId;

        var changeTracker = _dbContext.ChangeTracker;
        var logs = new List<LogHistory>();

        ProcessModifiedEntries(changeTracker, logs, actionBy);
        ProcessDeletedEntries(changeTracker, logs, actionBy);
        ProcessAddedEntries(changeTracker, logs, actionBy);

        if (logs.Any())
        {
            await LogsHistory.AddRange(logs);
        }

        return await _dbContext.SaveChangesAsync();
    }

    private void ProcessModifiedEntries(ChangeTracker changeTracker, List<LogHistory> logs, int? actionBy)
    {
        var modifiedEntries = changeTracker.Entries()
            .Where(e => e.State == EntityState.Modified && e.Entity is not ISkipLogHistory);

        foreach (var entry in modifiedEntries)
        {
            var tableName = entry.Metadata.GetTableName();
            var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString();

            var originalValues = new Dictionary<string, object>();
            var currentValues = new Dictionary<string, object>();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.Name == "UpdatedAt" || Equals(property.OriginalValue, property.CurrentValue) || (property.Metadata.Name == "ScheduledPublishDate" && actionBy == null))
                    continue;

                originalValues[property.Metadata.Name] = property.OriginalValue;
                currentValues[property.Metadata.Name] = property.CurrentValue;
            }

            if (entry.Entity is AdminRole admin)
            {
                var allPermissionEntries = changeTracker.Entries<RoleToPermission>()
                    .Where(x => x.Entity.RoleId == admin.Id)
                    .ToList();

                if (allPermissionEntries.Any(x => x.State == EntityState.Added || x.State == EntityState.Deleted))
                {
                    var currentPermissions = allPermissionEntries.Where(x => x.State != EntityState.Deleted)
                        .Select(x => new { x.Entity.PermissionsId });

                    var originalPermissions = allPermissionEntries.Where(x => x.State != EntityState.Added)
                        .Select(x => new { x.Entity.PermissionsId })
                        .ToList();

                    currentValues[nameof(Entities.DataContext.AdminRole.RoleToPermissions)] = currentPermissions;

                    originalValues[nameof(Entities.DataContext.AdminRole.RoleToPermissions)] = originalPermissions;
                }
            }

            if (originalValues.Any())
            {
                primaryKey = AdjustPrimaryKeyForSpecialTables(entry, tableName, primaryKey, originalValues, currentValues);
                int? entityId = GetEntityId(primaryKey, tableName);

                logs.Add(new LogHistory
                {
                    Action = "Updated",
                    AdminId = actionBy,
                    Date = DateTime.UtcNow,
                    EntityType = tableName,
                    EntityId = entityId,
                    UpdatedFrom = JsonConvert.SerializeObject(originalValues),
                    UpdatedTo = JsonConvert.SerializeObject(currentValues)
                });
            }
        }
    }

    private void ProcessAddedEntries(ChangeTracker changeTracker, List<LogHistory> logs, int? actionBy)
    {
        var addedEntries = changeTracker.Entries()
            .Where(e => e.State == EntityState.Added && e.Entity is not ISkipLogHistory);

        foreach (var entry in addedEntries)
        {
            var tableName = entry.Metadata.GetTableName();
            var currentValues = new Dictionary<string, object>();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.Name == "CreatedAt" || property.CurrentValue == null)
                    continue;

                currentValues[property.Metadata.Name] = property.CurrentValue;
            }

            if (entry.Entity is AdminRole admin)
            {
                currentValues[nameof(Entities.DataContext.AdminRole.RoleToPermissions)] = admin.RoleToPermissions.Select(x => new { x.PermissionsId }).ToList();
            }

            var primaryKey = currentValues.TryGetValue("Id", out var pk)
                ? pk?.ToString()
                : entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.OriginalValue?.ToString();

            primaryKey = AdjustPrimaryKeyForSpecialTables(entry, tableName, primaryKey, null, currentValues);
            int? entityId = GetEntityId(primaryKey, tableName);

            var logHistoty = new LogHistory
            {
                Action = "Inserted",
                AdminId = actionBy,
                Date = DateTime.UtcNow,
                EntityType = tableName,
                EntityId = entityId,
                UpdatedTo = JsonConvert.SerializeObject(currentValues)
            };
            logs.Add(logHistoty);
        }
    }

    private void ProcessDeletedEntries(ChangeTracker changeTracker, List<LogHistory> logs, int? actionBy)
    {
        var deletedEntries = changeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted && e.Entity is not ISkipLogHistory);

        foreach (var entry in deletedEntries)
        {
            var tableName = entry.Metadata.GetTableName();
            var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.OriginalValue?.ToString();

            var originalValues = new Dictionary<string, object>();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.Name == "DeletedAt" || property.OriginalValue == null)
                    continue;

                originalValues[property.Metadata.Name] = property.OriginalValue;
            }

            primaryKey = AdjustPrimaryKeyForSpecialTables(entry, tableName, primaryKey, originalValues, null);
            int? entityId = GetEntityId(primaryKey, tableName);

            logs.Add(new LogHistory
            {
                Action = "Deleted",
                AdminId = actionBy,
                Date = DateTime.UtcNow,
                EntityType = tableName,
                EntityId = entityId,
                UpdatedFrom = JsonConvert.SerializeObject(originalValues)
            });

        }
    }

    private string AdjustPrimaryKeyForSpecialTables(EntityEntry entry, string tableName, string primaryKey, Dictionary<string, object> originalValues, Dictionary<string, object> currentValues)
    {
        if (tableName == "WorkingHours")
        {
            var storeIdProp = entry.Properties.FirstOrDefault(x => x.Metadata.Name == "StoreId");
            if (storeIdProp?.OriginalValue == null && storeIdProp?.CurrentValue == null)
                return primaryKey;

            primaryKey = (storeIdProp?.OriginalValue?.ToString() ?? storeIdProp?.CurrentValue?.ToString()) ?? originalValues?["StoreId"]?.ToString();

            var dayOfWeek = entry.Properties.FirstOrDefault(x => x.Metadata.Name == "Day");
            var date = entry.Properties.FirstOrDefault(x => x.Metadata.Name == "Date");

            // Check if the entity is newly added
            if (entry.State == EntityState.Added)
            {
                if (dayOfWeek != null && !string.IsNullOrEmpty(dayOfWeek.CurrentValue?.ToString()))
                {
                    if (originalValues != null) originalValues["Day"] = null;
                    if (currentValues != null) currentValues["Day"] = dayOfWeek.CurrentValue;
                }
                if (date != null && !string.IsNullOrEmpty(date.CurrentValue?.ToString()))
                {
                    if (originalValues != null) originalValues["Date"] = null;
                    if (currentValues != null) currentValues["Date"] = date.CurrentValue;
                }
            }
            else
            {
                if (dayOfWeek != null && !dayOfWeek.IsModified && !string.IsNullOrEmpty(dayOfWeek.OriginalValue?.ToString()))
                {
                    if (originalValues != null) originalValues["Day"] = dayOfWeek.OriginalValue;
                    if (currentValues != null) currentValues["Day"] = dayOfWeek.CurrentValue;
                }
                else if (date != null && !date.IsModified && !string.IsNullOrEmpty(date.OriginalValue?.ToString()))
                {
                    if (originalValues != null) originalValues["Date"] = date.OriginalValue;
                    if (currentValues != null) currentValues["Date"] = date.CurrentValue;
                }
            }
        }
        else if (tableName == "SeoMeta")
        {
            var entityTypeProp = entry.Properties.FirstOrDefault(x => x.Metadata.Name == "EntityType");

            if (entityTypeProp?.CurrentValue?.ToString().ToLower() == "page" || entityTypeProp?.OriginalValue?.ToString().ToLower() == "page")
            {
                var pageEntityId = entry.Properties.FirstOrDefault(x => x.Metadata.Name == "EntityId");

                if (pageEntityId?.OriginalValue == null && pageEntityId?.CurrentValue == null)
                    return primaryKey;

                primaryKey = (pageEntityId?.CurrentValue?.ToString() ?? pageEntityId?.OriginalValue?.ToString()) ?? originalValues?["EntityId"]?.ToString();
            }
        }
        else if (tableName == "Sections")
        {
            var pageIdProp = entry.Properties.FirstOrDefault(x => x.Metadata.Name == "PageId");

            if (pageIdProp?.OriginalValue == null && pageIdProp?.CurrentValue == null)
                return primaryKey;

            primaryKey = (pageIdProp?.OriginalValue?.ToString() ?? pageIdProp?.CurrentValue?.ToString()) ?? originalValues?["PageId"]?.ToString();
        }
        else if (tableName == "PostTag")
        {
            var postIdProp = entry.Properties.FirstOrDefault(x => x.Metadata.Name == "PostId");

            if (postIdProp?.OriginalValue == null && postIdProp?.CurrentValue == null)
                return primaryKey;

            primaryKey = (postIdProp?.OriginalValue?.ToString() ?? postIdProp?.CurrentValue?.ToString()) ?? originalValues?["PostId"]?.ToString();
        }

        return primaryKey;
    }

    private int? GetEntityId(string primaryKey, string tableName)
    {
        int? entityId = null;
        if (!string.IsNullOrEmpty(primaryKey) && int.TryParse(primaryKey, out var id))
        {
            entityId = id;
        }

        if (tableName == "MediaFiles")
        {
            entityId = Stores.GetAllStores().SingleOrDefault(x => x.MediaFile.Id.ToString() == primaryKey)?.Id;
        }

        return entityId;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dbContext.Dispose();
        }
    }
}