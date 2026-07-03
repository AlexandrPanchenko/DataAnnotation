using JetFlight.ApplicationDataAccess.Entities.DataContext;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface IDataUnitOfWork : IDisposable
{

    IAdminRepository Admins { get; }
    IResetPasswordRepository ResetPasswords { get; }
    IPageRepository Pages { get; }
    IRoleRepository Roles { get; }
    IAdminToRoleRepository AdminRole { get; }
    IRolePolicyRepository RolePolicy { get; }
    IRolesPermissionRepository RolesPermission { get; }

    ILogHistoryRepository LogsHistory { get; }
    ISiteSettingsRepository SiteSettings { get; }
    IPostRepository Posts { get; }
    IPostToTagRepository PostToTags { get; }
    ITagRepository Tags { get; }
    ISectionsRepository Sections { get; }
    IStoreRepository Stores { get; }
    ICityRepository Cities { get; }
    IWorkingHoursRepository WorkingHours { get; }
    ISectionFieldRepository SectionFields { get; }
    public IContactUsRepository ContactUs { get; }
    public ITopicRepository Topic { get; }
    public ISeoMetaRepository SeoMeta { get; }
    public IMediaFilesRepository MediaFiles { get; }
    public IGenericDataRepository<Feedback> Feedbacks { get; }
    public IGenericDataRepository<RFM> RFMs { get; }
    public IGenericDataRepository<Target> Targets { get; }

    public IContactUsAttachmentRepository ContactUsAttachments { get; }


    Task<int> Save(bool? skipLogHistory = false);

}