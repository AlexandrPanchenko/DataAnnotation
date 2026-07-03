using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Shared;
using JetFlight.Shared.Models.AccumulationCard;
using JetFlight.Shared.Models.Coupons;

namespace JetFlight.Service.Services;

using Microsoft.EntityFrameworkCore;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.Shared.Models.LogHistory;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using static JetFlight.Shared.WorkingHoursComparer;
using JetFlight.Shared.Models.Posts;
using JetFlight.Shared.Models.Admins;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Extensions;
using System.Collections.Generic;
using JetFlight.Shared.UserContext;
using System.Linq;
using JetFlight.Shared.Models.Feedback;
using JetFlight.IntegrationDataAccess;
using static JetFlight.Service.Extensions.LogHistoryExtensions;

public interface ILogHistoryService
{
    Task<List<LogHistoryDTO>> GetAll(int adminId, DateTime? timeFrom, DateTime? timeTo);
    Task<List<LogHistoryDTO>> GetByStore(int storeId, DateTime? timeFrom, DateTime? timeTo);
    Task<List<LogHistoryDTO>> GetPageByPageId(int pageId, DateTime? timeFrom, DateTime? timeTo);
    Task<List<LogHistoryDTO>> GetByPost(int postId, DateTime? timeFrom, DateTime? timeTo);
    Task<List<LogHistoryDTO>> GetPageByRoleId(int roleId, DateTime? timeFrom, DateTime? timeTo);
    Task<List<LogHistoryDTO>> GetByContactUs(int contactUsId, DateTime? timeFrom, DateTime? timeTo);
    Task<List<LogHistoryDTO>> GetByFeedback(int feedbackId, DateTime? timeFrom, DateTime? timeTo);

    Task<List<LogHistoryDTO>> GetByCoupon(int couponId, DateTime? timeFrom, DateTime? timeTo);

    Task<List<LogHistoryDTO>> GetByAccumulationCard(int accumulationCardId, DateTime? timeFrom, DateTime? timeTo);

    Task AddRangeAsync(List<LogMessage> logMessages);

    Task AddAsync(LogMessage logMessage);
}

public class LogHistoryService : ILogHistoryService
{
    private readonly IDataUnitOfWork _unitOfWork;
    private readonly IntegrationDataContext _integrationDataContext;

    private static List<string> PageEntityTypes = new List<string>()
    {
        EntityTypesConstant.Log.Page,
        EntityTypesConstant.Log.SeoMeta,
        EntityTypesConstant.Log.Sections,
        EntityTypesConstant.Log.SectionField,
    };

    public LogHistoryService(IDataUnitOfWork unitOfWork, IntegrationDataContext integrationDataContext)
    {
        _unitOfWork = unitOfWork;
        _integrationDataContext = integrationDataContext;
    }

    public async Task<List<LogHistoryDTO>> GetAll(int adminId, DateTime? timeFrom, DateTime? timeTo)
    {
        var result = new List<LogHistoryDTO>();
        var logHistory = await _unitOfWork.LogsHistory.GetAll(adminId, timeFrom, timeTo).ToListAsync();

        //result.AddRange(await AddAdminLogHistory(logHistory.Where(x => x.EntityType == "Admins").ToList(), true));temp
        result.AddRange(await AddPostsLogHistory(logHistory.Where(x => x.EntityType == "Posts").ToList(), true, timeFrom, timeTo));
        result.AddRange(await AddPostTagLogHistory(logHistory.Where(x => x.EntityType == "PostTag").ToList(), true, timeFrom, timeTo));
        
        result.AddRange(await AddPagesLogHistoryForAdmin(logHistory.Where(x => PageEntityTypes.Contains(x.EntityType)).ToList(), true, timeFrom, timeTo)); 
        result.AddRange(await AddRolesLogHistory(logHistory.Where(x => x.EntityType == "AdminRole").ToList(), true, timeFrom, timeTo));
        result.AddRange(await AddStoreLogHistory(logHistory.Where(x => x.EntityType == "Stores").ToList(), true));
        result.AddRange(await AddMediaFilesLogHistory(logHistory.Where(x => x.EntityType == "MediaFiles").ToList(), true));
        result.AddRange(await AddWorkingHoursLogHistory(logHistory.Where(x => x.EntityType == "WorkingHours").ToList(), true));
        result.AddRange(await AddContactUsHistory(logHistory.Where(x => x.EntityType == "ContactUs").ToList(), true));
        result.AddRange(await AddFeedbackHistory(logHistory.Where(x => x.EntityType == "Feedbacks").ToList(), true));
        result.AddRange(await AddCouponHistory(logHistory.Where(x => x.EntityType == "Coupons").ToList(), true));
        result.AddRange(await AddAccumulationCardHistory(logHistory.Where(x => x.EntityType == "AccumulationCards").ToList(), true));
        return result.OrderByDescending(x => x.Date).ToList();

    }

    public async Task<List<LogHistoryDTO>> GetByPost(int postId, DateTime? timeFrom, DateTime? timeTo)
    {
        var result = new List<LogHistoryDTO>();
        var logHistory = await _unitOfWork.LogsHistory.GetByEntityId(postId, "Posts", timeFrom, timeTo)
            .ToListAsync();
        result.AddRange(await AddPostsLogHistory(logHistory, false, timeFrom, timeTo));
        var postTaglogHistory = await _unitOfWork.LogsHistory.GetByEntityId(postId, "PostTag", timeFrom, timeTo)
    .ToListAsync();
        result.AddRange(await AddPostTagLogHistory(postTaglogHistory, false, timeFrom, timeTo));
        return result.OrderByDescending(x => x.Date).ToList();
    }

    private async Task<List<LogHistoryDTO>> AddAdminLogHistory(List<LogHistory> logHistory, bool withEntity = false)
    {
        var result = new List<LogHistoryDTO>();
        foreach (var log in logHistory)
        {
            var logHistoryList = new List<LogHistoryField>();

            var UpdatedTo = !string.IsNullOrEmpty(log.UpdatedTo) ? JsonConvert.DeserializeObject<AdminUpdateDTO>(log.UpdatedTo) : new AdminUpdateDTO();
            var UpdateFrom = !string.IsNullOrEmpty(log.UpdatedFrom) ? JsonConvert.DeserializeObject<AdminUpdateDTO>(log.UpdatedFrom) : new AdminUpdateDTO();
            var userToUpdate = await _unitOfWork.Admins.GetById(log.EntityId.Value);
            var entity = withEntity ? $"{userToUpdate.FirstName} {userToUpdate.LastName}" : "";

            if (log.Action == ActionConstant.Updated && UpdatedTo.Password != null && UpdateFrom.Password != null && UpdatedTo.Password != UpdateFrom.Password)
            {

                logHistoryList.Add(new LogHistoryField($"оновив(ла) пароль користувача {entity} ", null, null));
            }
            if (log.Action == ActionConstant.Updated && UpdatedTo.Blocked != null && UpdateFrom.Blocked != null && UpdatedTo.Blocked != UpdateFrom.Blocked)
            {
                var message = UpdateFrom.Blocked == true ? $"заблокував(ла) користувача {entity}" : $"розблокував(ла) користувача {entity}";
                logHistoryList.Add(new LogHistoryField(message, null, null));
            }

            if (log.Action == ActionConstant.Updated && UpdatedTo.FirstName != null && UpdateFrom.FirstName != null && UpdatedTo.FirstName != UpdateFrom.FirstName)

            {
                logHistoryList.Add(new LogHistoryField($"змінив(ла) ім'я користувача {entity}", UpdatedTo.FirstName, UpdateFrom.FirstName));
            }
            if (log.Action == ActionConstant.Updated && UpdatedTo.LastName != null && UpdateFrom.LastName != null && UpdatedTo.LastName != UpdateFrom.LastName)
            {
                logHistoryList.Add(new LogHistoryField($"змінив(ла) призвіще користувача {entity}", UpdatedTo.LastName, UpdateFrom.LastName));
            }

            if (log.Action == ActionConstant.Updated && UpdatedTo.Email != null && UpdateFrom.Email != null && UpdatedTo.Email != UpdateFrom.Email)
            {
                logHistoryList.Add(new LogHistoryField($"змінив(ла) пошту користувача {entity}", UpdatedTo.Email, UpdateFrom.Email));
            }
            if (log.Action == ActionConstant.Inserted && !string.IsNullOrEmpty(entity))
            {
                logHistoryList.Add(new LogHistoryField { Title = $"створив(ла) користувача {entity}" });
            }
            if (logHistoryList.Any())
            {
                result.Add(new LogHistoryDTO
                {
                    Action = log.Action,
                    EntityId = log.EntityId,
                    EntityType = log.EntityType,
                    ActionBy = log.AdminId ?? null,
                    ActionByName = $"{log.Admin?.FirstName} {log.Admin?.LastName}",
                    Date = log.Date,
                    LogHistoryList = logHistoryList
                });
            }

        }
        return result;
    }

    private async Task<List<LogHistoryDTO>> AddPostTagLogHistory(List<LogHistory> postTags, bool withEntity = false, DateTime? timeFrom = null, DateTime? timeTo = null)
    {
        var result = new List<LogHistoryDTO>();

        foreach (var taglogs in postTags)
        {
            var logHistoryList = new List<LogHistoryField>();
            var postsToUpdate = await _unitOfWork.Posts.GetById(taglogs.EntityId.Value);
            var entity = withEntity ? $"{postsToUpdate?.Name}" : "";
            var UpdatedTo = !string.IsNullOrEmpty(taglogs.UpdatedTo) ? JsonConvert.DeserializeObject<PostsTagLogHistoryDTO>(taglogs.UpdatedTo) : new PostsTagLogHistoryDTO();
            var UpdatedFrom = !string.IsNullOrEmpty(taglogs.UpdatedFrom) ? JsonConvert.DeserializeObject<PostsTagLogHistoryDTO>(taglogs.UpdatedFrom) : new PostsTagLogHistoryDTO();
            if (taglogs.Action == ActionConstant.Inserted && UpdatedTo != null && UpdatedTo.CategoryId.HasValue)
            {
                var postsTag =  await _unitOfWork.PostToTags.GetCategory(UpdatedTo.CategoryId.Value);
                string pastname = postsTag != null ? postsTag.Name : "";
                logHistoryList.Add(new LogHistoryField($"Додав категорію {pastname} для статі {entity} ", null, null));
            }
            if (taglogs.Action == ActionConstant.Deleted && UpdatedFrom != null && UpdatedFrom.CategoryId.HasValue)
            {
                var postsTag = await _unitOfWork.PostToTags.GetCategory(UpdatedFrom.CategoryId.Value);
                string pastname = postsTag != null ? postsTag.Name : "";
                logHistoryList.Add(new LogHistoryField($"Видалив категорію {postsTag.Name} для статі {entity} ", null, null));
            }
            if (logHistoryList.Any())
            {
                result.Add(new LogHistoryDTO
                {
                    Action = taglogs.Action,
                    EntityId = taglogs.EntityId,
                    EntityType = taglogs.EntityType,
                    ActionBy = taglogs.AdminId ?? null,
                    ActionByName = $"{taglogs.Admin?.FirstName} {taglogs.Admin?.LastName}",
                    Date = taglogs.Date,
                    LogHistoryList = logHistoryList
                });
            }
        }
        return result;
    }

    private async Task<List<LogHistoryDTO>> AddPostsLogHistory(List<LogHistory> posts, bool withEntity = false, DateTime? timeFrom = null, DateTime? timeTo = null)
    {
        var result = new List<LogHistoryDTO>();

        foreach (var log in posts)
        {
            var logHistoryList = new List<LogHistoryField>();
            var UpdatedTo = !string.IsNullOrEmpty(log.UpdatedTo) ? JsonConvert.DeserializeObject<PostLogHistoryDTO>(log.UpdatedTo) : new PostLogHistoryDTO();
            var UpdateFrom = !string.IsNullOrEmpty(log.UpdatedFrom) ? JsonConvert.DeserializeObject<PostLogHistoryDTO>(log.UpdatedFrom) : new PostLogHistoryDTO();

            var postsToUpdate = await _unitOfWork.Posts.GetById(log.EntityId.Value);
            var entity = withEntity ? $"{postsToUpdate?.Name}" : "";

            if (log.Action == ActionConstant.Updated && UpdatedTo.Name != null && UpdateFrom.Name != null && UpdatedTo.Name != UpdateFrom.Name)
            {
                logHistoryList.Add(new LogHistoryField($"оновив(ла) назву статі {entity} ", UpdateFrom.Name, UpdatedTo.Name));
            }

            if (log.Action == ActionConstant.Updated && UpdateFrom.PublishedAt != null && UpdatedTo.PublishedAt != null && UpdateFrom.PublishedAt != UpdatedTo.PublishedAt)
            {
                logHistoryList.Add(new LogHistoryField($"переопублікував(ла) статю {entity}", null, null));
            }
            else if (UpdateFrom.OriginId == null && UpdatedTo.OriginId != null)
            {
                logHistoryList.Add(new LogHistoryField($"опублікував(ла) статю {entity}", null, null));
            }

            if (log.Action == ActionConstant.Updated && UpdatedTo.OriginId == null && UpdateFrom.OriginId != null)
            {
                logHistoryList.Add(new LogHistoryField($"відмінив(ла) публікацію статті {entity} ", null, null));
            }
            if (log.Action == ActionConstant.Updated && UpdatedTo.Subtitle != null && UpdateFrom.Subtitle != null && UpdatedTo.Subtitle != UpdateFrom.Subtitle)
            {
                logHistoryList.Add(new LogHistoryField($"оновив(ла) підзаголовок статі {entity} ", UpdateFrom.Subtitle, UpdatedTo.Subtitle));
            }
            if (log.Action == ActionConstant.Updated && UpdatedTo.ImageAlt != null && UpdateFrom.ImageAlt != null && UpdatedTo.ImageAlt != UpdateFrom.ImageAlt)
            {
                logHistoryList.Add(new LogHistoryField($"оновив(ла) Alt значення картинки для статі {entity} ", UpdateFrom.ImageAlt, UpdatedTo.ImageAlt));
            }
            if (log.Action == ActionConstant.Updated && UpdatedTo.Text != null && UpdateFrom.Text != null && UpdatedTo.Text != UpdateFrom.Text)
            {
                logHistoryList.Add(new LogHistoryField($"оновив(ла) контeнт для статі {entity} ", null, null));
            }
            if (log.Action == ActionConstant.Updated && UpdatedTo.FixedPost != null && UpdateFrom.FixedPost != null && UpdatedTo.FixedPost != UpdateFrom.FixedPost)
            {
                var message = UpdatedTo.FixedPost == true ? $"прикрипив(ла) статю {entity}" : $"відкрипив(ла) статю {entity}";
                logHistoryList.Add(new LogHistoryField(message, null, null));
            }
            if (log.Action == ActionConstant.Updated && UpdatedTo.ReadDurationMin != null && UpdateFrom.ReadDurationMin != null && UpdatedTo.ReadDurationMin != UpdateFrom.ReadDurationMin)
            {
                logHistoryList.Add(new LogHistoryField($"оновив(ла) час прочитання для статі {entity} ", UpdateFrom.ReadDurationMin, UpdatedTo.ReadDurationMin));
            }

            if (log.Action == ActionConstant.Updated && UpdatedTo.BranchId != null && UpdateFrom.BranchId != null && UpdatedTo.BranchId != UpdateFrom.BranchId)
            {
                if (UpdatedTo.BranchId == (int)Branches.CatJet)
                {
                    logHistoryList.Add(new LogHistoryField($"оновив(ла) магазин для статті {entity} ", null, "CatJet"));
                }
                else if (UpdatedTo.BranchId == (int)Branches.BirdJet)
                {
                    logHistoryList.Add(new LogHistoryField($"оновив(ла) магазин для статті {entity} ", null, "BirdJet"));
                }
                else if (UpdatedTo.BranchId == 0)
                {
                    logHistoryList.Add(new LogHistoryField($"оновив(ла) магазин для статті {entity} ", null, "BirdJet і CatJet"));
                }
            }

            if (log.Action == ActionConstant.Updated && UpdatedTo.ImageName != null && UpdateFrom.ImageName != null && UpdatedTo.ImageName != UpdateFrom.ImageName)
            {
                logHistoryList.Add(new LogHistoryField($"оновив(ла) картинку для статі {entity} ", new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!) { Path = $"{StorageConstants.AppPath}/{UpdateFrom.ImageName}" }.ToString(), new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!) { Path = $"{StorageConstants.AppPath}/{UpdatedTo.ImageName}"}.ToString()));
            }

            if (UpdatedTo.IsActive != null && UpdateFrom.IsActive != null && UpdatedTo.IsActive != UpdateFrom.IsActive && UpdatedTo.IsActive == false)
            {
                logHistoryList.Add(new LogHistoryField { Title = $"видалив(ла) сторінку {entity}" });
            }

            if (log.Action == ActionConstant.Inserted)
            {
                logHistoryList.Add(new LogHistoryField { Title = $"створив(ла) сторінку {entity}" });
            }
                
            if (logHistoryList.Any())
            {
                result.Add(new LogHistoryDTO
                {
                    Action = log.Action,
                    EntityId = log.EntityId,
                    EntityType = log.EntityType,
                    ActionBy = log.AdminId ?? null,
                    ActionByName = $"{log.Admin?.FirstName} {log.Admin?.LastName}",
                    Date = log.Date,
                    LogHistoryList = logHistoryList
                });
            }
        }
        return result;
    }
    private async Task<List<LogHistoryDTO>> AddStoreLogHistory(List<LogHistory> logHistory , bool withEntity)
    {
        var result = new List<LogHistoryDTO>();
        foreach (var log in logHistory)
        {
            var store = await _unitOfWork.Stores.GetById(log.EntityId.Value);
            var entity = withEntity ? $"для магазина {store.Address}" : "";
            var logHistoryList = new List<LogHistoryField>();
            var UpdatedTo = !string.IsNullOrEmpty(log.UpdatedTo) ? JsonConvert.DeserializeObject<StoreLogHistoryDTO>(log.UpdatedTo) : new StoreLogHistoryDTO();
            var UpdateFrom = !string.IsNullOrEmpty(log.UpdatedFrom) ? JsonConvert.DeserializeObject<StoreLogHistoryDTO>(log.UpdatedFrom) : new StoreLogHistoryDTO();


            if (UpdatedTo.CityId != null && UpdateFrom.CityId != null && UpdatedTo.CityId != UpdateFrom.CityId)
            {
                var oldCity = await _unitOfWork.Cities.GetById(UpdateFrom.CityId.Value);
                var newCity = await _unitOfWork.Cities.GetById(UpdatedTo.CityId.Value);
                logHistoryList.Add(new LogHistoryField($"оновив(ла) місто {entity}", oldCity.Name, newCity.Name));
            }

            if (UpdatedTo.Address != null && UpdateFrom.Address != null && UpdatedTo.Address != UpdateFrom.Address)
            {
                logHistoryList.Add(new LogHistoryField($"оновив(ла) вулицю {entity}", UpdateFrom.Address, UpdatedTo.Address));
            }

            if (UpdatedTo.MapLink != null && UpdatedTo.MapLink != UpdateFrom.MapLink)
            {
                logHistoryList.Add(new LogHistoryField($"оновив(ла) посилання на карту {entity}", UpdateFrom.MapLink, UpdatedTo.MapLink));
            }


            if (UpdatedTo.Address2 != null && UpdateFrom.Address2 != null && UpdatedTo.Address2 != UpdateFrom.Address2)
            {
                logHistoryList.Add(new LogHistoryField($"оновив(ла) номер будинку {entity}", UpdateFrom.Address2, UpdatedTo.Address2));
            }

            if (UpdatedTo.Latitude != null && UpdateFrom.Latitude != null && UpdatedTo.Latitude != UpdateFrom.Latitude)
            {
                logHistoryList.Add(new LogHistoryField($"оновив(ла) широту {entity}", UpdateFrom.Latitude, UpdatedTo.Latitude));
            }

            if (UpdatedTo.Longitude != null && UpdateFrom.Longitude != null && UpdatedTo.Longitude != UpdateFrom.Longitude)
            {
                logHistoryList.Add(new LogHistoryField($"оновив(ла) довготу {entity}", UpdateFrom.Longitude, UpdatedTo.Longitude));
            }

            if (UpdatedTo.IsActive != null && UpdateFrom.IsActive != null && UpdatedTo.IsActive != UpdateFrom.IsActive)
            {
                logHistoryList.Add(
                    new LogHistoryField($"оновив(ла) статус {entity}",
                    UpdateFrom.IsActive.Value ? "Активний" : "Неактивний",
                    UpdatedTo.IsActive.Value ? "Активний" : "Неактивний"));
            }
            if (logHistoryList.Any())
            {
                result.Add(new LogHistoryDTO
                {
                    Action = log.Action,
                    EntityId = log.EntityId,
                    EntityType = log.EntityType,
                    ActionBy = log.AdminId ?? null,
                    ActionByName = $"{log.Admin?.FirstName} {log.Admin?.LastName}",
                    Date = log.Date,
                    LogHistoryList = logHistoryList
                });
            }
        }
        return result;
    }

    private async Task<List<LogHistoryDTO>> AddMediaFilesLogHistory(List<LogHistory> logHistory, bool withEntity)
    {
        var result = new List<LogHistoryDTO>();
        foreach (var logHistoryMediaFile in logHistory)
        {
            var store = await _unitOfWork.Stores.GetById(logHistoryMediaFile.EntityId.Value);
            var entity = withEntity ? $"для магазина {store.Address}" : "";
            var logHistoryMediaFilesList = new List<LogHistoryField>();

            var oldFile = !string.IsNullOrEmpty(logHistoryMediaFile.UpdatedFrom) ? JsonConvert.DeserializeObject<MediaFilesLogHistoryDTO>(logHistoryMediaFile.UpdatedFrom) : null;
            var newFile = !string.IsNullOrEmpty(logHistoryMediaFile.UpdatedTo) ? JsonConvert.DeserializeObject<MediaFilesLogHistoryDTO>(logHistoryMediaFile.UpdatedTo) : null;

            if (newFile != null && oldFile == null && newFile.Name != null)
            {
                logHistoryMediaFilesList.Add(new LogHistoryField($"оновив(ла) зображення {entity}", string.Empty, GetImagePathByName(newFile.Name)));
            }
            else if (newFile == null && oldFile != null && oldFile.Name != null)
            {
                logHistoryMediaFilesList.Add(new LogHistoryField($"видалив(ла) зображення {entity}", string.Empty, string.Empty));
            }
            else if (newFile?.Name != null && oldFile?.Name != null && oldFile.Name != newFile.Name)
            {
                logHistoryMediaFilesList.Add(new LogHistoryField($"оновив(ла) зображення {entity}", string.Empty, GetImagePathByName(newFile.Name)));
            }

            result.Add(new LogHistoryDTO
            {
                Action = logHistoryMediaFile.Action,
                EntityId = logHistoryMediaFile.EntityId,
                EntityType = logHistoryMediaFile.EntityType,
                ActionBy = logHistoryMediaFile.AdminId ?? null,
                ActionByName = $"{logHistoryMediaFile.Admin?.FirstName} {logHistoryMediaFile.Admin?.LastName}",
                Date = logHistoryMediaFile.Date,
                LogHistoryList = logHistoryMediaFilesList
            });
        }
        return result;
    }

    private async Task<List<LogHistoryDTO>> AddWorkingHoursLogHistory(List<LogHistory> logHistory, bool withEntity)
    {
        var result = new List<LogHistoryDTO>();
        foreach (var log in logHistory)
        {
            var oldHours = !string.IsNullOrEmpty(log.UpdatedFrom) ? JsonConvert.DeserializeObject<WorkingHoursLogHistoryDTO>(log.UpdatedFrom) : new WorkingHoursLogHistoryDTO();
            var newHours = !string.IsNullOrEmpty(log.UpdatedTo) ? JsonConvert.DeserializeObject<WorkingHoursLogHistoryDTO>(log.UpdatedTo) : new WorkingHoursLogHistoryDTO();
            var store = await _unitOfWork.Stores.GetById(log.EntityId.Value);
            var entity = withEntity ? $"для магазина {store.Address}" : "";
            var workingHoursCompareResult = CompareWorkingHours(oldHours, newHours, entity);

            if (workingHoursCompareResult.Count > 0)
            {
                Admin? actionBy = log.Admin ??
                    (log.AdminId != null
                        ? await _unitOfWork.Admins.GetById(log.AdminId.Value)
                        : null);

                if (workingHoursCompareResult.Any())
                {
                    result.Add(new LogHistoryDTO
                    {
                        Action = log.Action,
                        EntityId = log.EntityId,
                        EntityType = log.EntityType,
                        ActionBy = log.AdminId ?? null,
                        ActionByName = $"{actionBy?.FirstName} {actionBy?.LastName}",
                        Date = log.Date,
                        LogHistoryList = workingHoursCompareResult
                    });
                }
            }
        }
        return result;
    }

    public async Task<List<LogHistoryDTO>> GetByStore(int storeId, DateTime? timeFrom, DateTime? timeTo)
    {
        var result = new List<LogHistoryDTO>();

            var logHistory = await _unitOfWork.LogsHistory
            .GetByEntityId(storeId, "Stores", timeFrom, timeTo)
            .ToListAsync();

            result.AddRange(await AddStoreLogHistory(logHistory, false));
            
            // пошук зображень магазина
            var logHistoryMediaFiles = await _unitOfWork.LogsHistory
                .GetByEntityId(storeId, "MediaFiles", timeFrom, timeTo)
                .ToListAsync();

        result.AddRange(await AddMediaFilesLogHistory(logHistoryMediaFiles, false));

        // пошук робочих годин магазина
        var logHistoryWorkingHours = await _unitOfWork.LogsHistory
                    .GetByEntityId(storeId, "WorkingHours", timeFrom, timeTo)
                    .ToListAsync();

        result.AddRange(await AddWorkingHoursLogHistory(logHistoryWorkingHours, false));


        return result.OrderByDescending(x => x.Date).ToList();
    }

    public async Task<List<LogHistoryDTO>> GetPageByPageId(int pageId, DateTime? timeFrom, DateTime? timeTo)
    {
        var logHistory = await _unitOfWork.LogsHistory.GetByEntityId(pageId, PageEntityTypes, timeFrom, timeTo).ToListAsync();
        var result = new List<LogHistoryDTO>();

        result.AddRange(await AddPagesLogHistory(logHistory, false, timeFrom, timeTo));

        return result.OrderByDescending(x => x.Date).ToList();
    }

    private async Task<List<LogHistoryDTO>> AddPagesLogHistory(List<LogHistory> logHistory, bool withEntity = false, DateTime? timeFrom = null, DateTime? timeTo = null)
    {
        var entityIds = logHistory.Where(x =>PageEntityTypes.Contains(x.EntityType)).Select(log => log.EntityId.Value).ToList();

        var logHistorySeoMeta = await _unitOfWork.LogsHistory
            .GetAll()
            .Where(x => x.EntityType == "SeoMeta" && entityIds.Contains(x.EntityId.Value) &&
                        (!timeFrom.HasValue || x.Date >= timeFrom) &&
                        (!timeTo.HasValue || x.Date <= timeTo.Value.AddDays(1)))
            .ToListAsync();

        var logHistorySections = await _unitOfWork.LogsHistory
            .GetAll()
            .Where(x => x.EntityType == "Sections" && entityIds.Contains(x.EntityId.Value) &&
                        (!timeFrom.HasValue || x.Date >= timeFrom) &&
                        (!timeTo.HasValue || x.Date <= timeTo.Value.AddDays(1)))
            .ToListAsync();

        var logHistorySectionFields = await _unitOfWork.LogsHistory
            .GetAll()
            .Where(x => x.EntityType == "SectionField" && entityIds.Contains(x.EntityId.Value) &&
                        (!timeFrom.HasValue || x.Date >= timeFrom) &&
                        (!timeTo.HasValue || x.Date <= timeTo.Value.AddDays(1)))
            .ToListAsync();

        var result = new List<LogHistoryDTO>();

        await ProcessLogHistoryAsync<PageLogHistoryDTO>(logHistory, withEntity, result, DeserializePageLogHistory);
        await ProcessLogHistoryAsync<SeoMetaLogHistoryDTO>(logHistorySeoMeta, withEntity, result, DeserializeSeoMetaLogHistory);
        await ProcessLogHistoryAsync<SectionsLogHistoryDTO>(logHistorySections, withEntity, result, DeserializeSectionsLogHistory);
        await ProcessLogHistoryAsync<SectionFieldLogHistoryDTO>(logHistorySectionFields, withEntity, result, DeserializeSectionFieldLogHistory);

        return result.OrderByDescending(x => x.Date).ToList();
    }

    private async Task<List<LogHistoryDTO>> AddPagesLogHistoryForAdmin(List<LogHistory> logHistory, bool withEntity = false, DateTime? timeFrom = null, DateTime? timeTo = null)
    {
        var adminIds = logHistory.Where(x => PageEntityTypes.Contains(x.EntityType)).Select(log => log.AdminId.Value).ToList();

        var logHistorySeoMeta = await _unitOfWork.LogsHistory
            .GetAll()
            .Where(x => x.EntityType == "SeoMeta" && adminIds.Contains(x.AdminId.Value) &&
                        (!timeFrom.HasValue || x.Date >= timeFrom) &&
                        (!timeTo.HasValue || x.Date <= timeTo.Value.AddDays(1)))
            .ToListAsync();

        var logHistorySections = await _unitOfWork.LogsHistory
            .GetAll()
            .Where(x => x.EntityType == "Sections" && adminIds.Contains(x.AdminId.Value) &&
                        (!timeFrom.HasValue || x.Date >= timeFrom) &&
                        (!timeTo.HasValue || x.Date <= timeTo.Value.AddDays(1)))
            .ToListAsync();

        var logHistorySectionFields = await _unitOfWork.LogsHistory
            .GetAll()
            .Where(x => x.EntityType == "SectionField" && adminIds.Contains(x.AdminId.Value) &&
                        (!timeFrom.HasValue || x.Date >= timeFrom) &&
                        (!timeTo.HasValue || x.Date <= timeTo.Value.AddDays(1)))
            .ToListAsync();

        var result = new List<LogHistoryDTO>();

        await ProcessLogHistoryAsync<PageLogHistoryDTO>(logHistory, withEntity, result, DeserializePageLogHistory);
        await ProcessLogHistoryAsync<SeoMetaLogHistoryDTO>(logHistorySeoMeta, withEntity, result, DeserializeSeoMetaLogHistory);
        await ProcessLogHistoryAsync<SectionsLogHistoryDTO>(logHistorySections, withEntity, result, DeserializeSectionsLogHistory);
        await ProcessLogHistoryAsync<SectionFieldLogHistoryDTO>(logHistorySectionFields, withEntity, result, DeserializeSectionFieldLogHistory);

        return result.OrderByDescending(x => x.Date).ToList();
    }

    public async Task<List<LogHistoryDTO>> GetByContactUs(int contactUsId, DateTime? timeFrom, DateTime? timeTo)
    {
        var result = new List<LogHistoryDTO>();
        var logHistory = await _unitOfWork.LogsHistory
            .GetByEntityId(contactUsId, "ContactUs", timeFrom, timeTo)
            .ToListAsync();

        await ProcessLogHistoryAsync<ContactUsLogHistoryDTO>(logHistory, false, result, DeserializeContactUsLogHistory);

        result = result.OrderByDescending(x => x.Date).ToList();

        return result;
    }
    public async Task<List<LogHistoryDTO>> GetByFeedback(int feedbackId, DateTime? timeFrom, DateTime? timeTo)
    {
        var result = new List<LogHistoryDTO>();
        var logHistory = await _unitOfWork.LogsHistory
            .GetByEntityId(feedbackId, "Feedbacks", timeFrom, timeTo)
            .ToListAsync();

        await ProcessLogHistoryAsync<FeedbackLogHistoryDTO>(logHistory, false, result, DeserializeFeedbackLogHistory);

        result = result.OrderByDescending(x => x.Date).ToList();

        return result;
    }

    public async Task<List<LogHistoryDTO>> GetByCoupon(int couponId, DateTime? timeFrom, DateTime? timeTo)
    {
        var result = new List<LogHistoryDTO>();
        var logHistory = await _unitOfWork.LogsHistory
            .GetByEntityId(couponId, "Coupons", timeFrom, timeTo)
            .ToListAsync();

        await ProcessLogHistoryAsync<CouponLogHistoryDTO>(logHistory, false, result, DeserializeCouponLogHistory);

        result = result.OrderByDescending(x => x.Date).ToList();

        return result;
    }

    public async Task<List<LogHistoryDTO>> GetByAccumulationCard(int accumulationCardId, DateTime? timeFrom, DateTime? timeTo)
    {
        var result = new List<LogHistoryDTO>();
        var logHistory = await _unitOfWork.LogsHistory
            .GetByEntityId(accumulationCardId, "AccumulationCards", timeFrom, timeTo)
            .ToListAsync();

        await ProcessLogHistoryAsync<AccumulationCardLogHistoryDTO>(logHistory, false, result, DeserializeAccumulationCardLogHistory);

        result = result.OrderByDescending(x => x.Date).ToList();

        return result;
    }

    public async Task AddRangeAsync(List<LogMessage> logMessages)
    {
        await _unitOfWork.LogsHistory.AddRange(logMessages.Select(log => new LogHistory
        {
            Action = log.Action,
            EntityId = log.EntityId,
            EntityType = log.EntityType,
            AdminId = log.AdminId,
            UpdatedTo = log.UpdatedTo,
            UpdatedFrom = log.UpdatedFrom,
            Date = log.Date,
        }));

        await _unitOfWork.Save(skipLogHistory: true);
    }
    public async Task AddAsync(LogMessage log)
    {
        await _unitOfWork.LogsHistory.Add( new LogHistory
        {
            Action = log.Action,
            EntityId = log.EntityId,
            EntityType = log.EntityType,
            AdminId = log.AdminId,
            UpdatedTo = log.UpdatedTo,
            UpdatedFrom = log.UpdatedFrom,
            Date = log.Date,
        });

        await _unitOfWork.Save(skipLogHistory: true);
    }

    private async Task<(List<LogHistoryField>, FeedbackLogHistoryDTO, FeedbackLogHistoryDTO)> DeserializeFeedbackLogHistory(LogHistory log, bool withEntity)
    {
        var updatedTo = !string.IsNullOrEmpty(log.UpdatedTo)
            ? JsonConvert.DeserializeObject<FeedbackLogHistoryDTO>(log.UpdatedTo)
            : new FeedbackLogHistoryDTO();
        var updatedFrom = !string.IsNullOrEmpty(log.UpdatedFrom)
            ? JsonConvert.DeserializeObject<FeedbackLogHistoryDTO>(log.UpdatedFrom)
            : new FeedbackLogHistoryDTO();

        var feedback = await _unitOfWork.Feedbacks.GetById(log.EntityId.Value);
        var customer = await _integrationDataContext.Customers.Include(x => x.CustomerSettings)
    .FirstAsync(x => x.Id == feedback.CustomerId);

        var entity = (withEntity && feedback != null)
            ? $"для відгуку користувача {customer.Email}"
            : "";
        var logHistoryFields = new List<LogHistoryField>();


        if (log.Action == "Updated" && updatedTo.ResolveMessage != updatedFrom.ResolveMessage)
        {
            logHistoryFields.Add(new LogHistoryField(
                $"залишив(ла) відповідь {updatedTo.ResolveMessage} {entity}",
                string.Empty,
                string.Empty));
        }

        if (log.Action == "Updated" && updatedFrom.Status != updatedTo.Status)
        {
            var fromStatus = ((FeedbackStatus)updatedFrom.Status).ToString();
            var toStatus = ((FeedbackStatus)updatedTo.Status).ToString();

            logHistoryFields.Add(new LogHistoryField(
                $"змінив(ла) статус {entity}",
                fromStatus,
                toStatus));
        }

        if (log.Action == "Updated" && updatedFrom.ResolveSignature != updatedTo.ResolveSignature)
        {
            logHistoryFields.Add(new LogHistoryField(
                $"змінив(ла) підпис {entity}",
                updatedFrom.ResolveSignature,
                updatedTo.ResolveSignature));
        }

        return (logHistoryFields, updatedTo, updatedFrom);
    }

    private async Task<(List<LogHistoryField>, CouponLogHistoryDTO, CouponLogHistoryDTO)> DeserializeCouponLogHistory(LogHistory log, bool withEntity)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        var updatedTo = !string.IsNullOrEmpty(log.UpdatedTo) ? JsonConvert.DeserializeObject<CouponLogHistoryDTO>(log.UpdatedTo, settings) : new CouponLogHistoryDTO();
        var updatedFrom = !string.IsNullOrEmpty(log.UpdatedFrom) ? JsonConvert.DeserializeObject<CouponLogHistoryDTO>(log.UpdatedFrom, settings) : new CouponLogHistoryDTO();

        var coupon = await _integrationDataContext.Coupons.FirstOrDefaultAsync(x => x.Id == log.EntityId);
        var entity = (withEntity && coupon != null) ? $"ваучер {coupon.PrivateName}" : "ваучер";
        var logHistoryFields = new List<LogHistoryField>();

        if (log.Action == "Inserted")
        {
            logHistoryFields.Add(new LogHistoryField(){Title = $"створив(ла) {entity}"});
        }

        if (log.Action == "Deleted")
        {
            entity = withEntity ? $"ваучер {updatedFrom?.PrivateName}" : "ваучер";
            logHistoryFields.Add(new LogHistoryField() { Title = $"видалив(ла) {entity}" });
        }

        if (log.Action == "Updated")
        {

            if (updatedTo!.Status == CouponStatus.Active)
            {
                logHistoryFields.Add(new LogHistoryField() { Title = $"опубліковав(ла) {entity}" });
            }

            if (updatedTo.Status == CouponStatus.Archived)
            {
                logHistoryFields.Add(new LogHistoryField() { Title = $"архівував(ла) {entity}" });
            }

            if (updatedTo.Name != updatedFrom!.Name)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) назву для {entity}", updatedFrom.Name, updatedTo.Name));
            }

            if (updatedTo.PrivateName != updatedFrom.PrivateName)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) технічну назву для {entity}", updatedFrom.PrivateName, updatedTo.PrivateName));
            }

            if (updatedTo.StartDate != updatedFrom.StartDate)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) дату початку для {entity}", updatedFrom.StartDate?.ToString("o"), updatedTo.StartDate?.ToString("o")));
            }

            if (updatedTo.ExpirationDate != updatedFrom.ExpirationDate)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) дату кінця для {entity}", updatedFrom.ExpirationDate?.ToString("o"), updatedTo.ExpirationDate?.ToString("o")));
            }

            if (updatedTo.Description != updatedFrom.Description)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) умови використання для {entity}", updatedFrom.Description, updatedTo.Description));
            }

            if (updatedTo.PrivateDescription != updatedFrom.PrivateDescription)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) технічні умови використання для {entity}", updatedFrom.PrivateDescription, updatedTo.PrivateDescription));
            }

            if (updatedTo.Class != updatedFrom.Class)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) клас для {entity}", updatedFrom.Class.GetDisplayName(), updatedTo.Class.GetDisplayName()));
            }

            if (updatedTo.Type != updatedFrom.Type)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) тип для {entity}", updatedFrom.Type.GetDisplayName(), updatedTo.Type.GetDisplayName()));
            }

            if (updatedTo.Type == updatedFrom.Type && updatedTo.CouponDetails is CouponProductFixedPriceDTO or CouponCombinationFixedPriceDTO or CouponCombinationPriceDiscountDTO)
            {
                if (updatedFrom.CouponDetails is CouponProductFixedPriceDTO fromFixedPriceDetails && updatedTo.CouponDetails is CouponProductFixedPriceDTO toFixedPriceDetails)
                {
                    if (fromFixedPriceDetails.ProductCode != toFixedPriceDetails.ProductCode)
                    {
                        var products = await _integrationDataContext.Products.Where(x =>
                                x.Code == fromFixedPriceDetails.ProductCode ||
                                x.Code == toFixedPriceDetails.ProductCode)
                            .ToListAsync();

                        var fromProduct = products.FirstOrDefault(x => x.Code == fromFixedPriceDetails.ProductCode);
                        var toProduct = products.FirstOrDefault(x => x.Code == toFixedPriceDetails.ProductCode);

                        logHistoryFields.Add(new LogHistoryField($"змінив(ла) товар-активатор для {entity}", fromProduct?.Title, toProduct?.Title));
                    }
                }
                else if (updatedFrom.CouponDetails is CouponCombinationFixedPriceDTO fromCombinationFixedPriceDetails && updatedTo.CouponDetails is CouponCombinationFixedPriceDTO toCombinationFixedPriceDetails)
                {
                    if (!CollectionsHelper.AreCollectionsEqual(
                            fromCombinationFixedPriceDetails.Activators,
                            toCombinationFixedPriceDetails.Activators,
                            x => x.ProductCode,
                            new CouponProductActivatorDTOComparer()))
                    {
                        var productIds = fromCombinationFixedPriceDetails.Activators.Select(x => x.ProductCode)
                            .Union(toCombinationFixedPriceDetails.Activators.Select(x => x.ProductCode));
                        var products = await _integrationDataContext.Products.Where(x => productIds.Contains(x.Code))
                            .ToListAsync();

                        var fromProducts = fromCombinationFixedPriceDetails.Activators.Join(
                            products,
                            x => x.ProductCode,
                            x => x.Code,
                            (activator, product) => $"{product.Title} ({activator.Quantity} шт.)")
                            .OrderBy(x => x);
                        var toProducts = toCombinationFixedPriceDetails.Activators.Join(
                            products,
                            x => x.ProductCode,
                            x => x.Code,
                            (activator, product) => $"{product.Title} ({activator.Quantity} шт.)")
                            .OrderBy(x => x);

                        logHistoryFields.Add(new LogHistoryField($"змінив(ла) товари-активатори для {entity}", string.Join("; ", fromProducts), string.Join("; ", toProducts)));
                    }
                }
                else if (updatedFrom.CouponDetails is CouponCombinationPriceDiscountDTO fromDetails && updatedTo.CouponDetails is CouponCombinationPriceDiscountDTO toDetails)
                {
                    var fromCategoryActivators = fromDetails.Activators.Where(x => x is CouponCategoryActivatorDTO)
                        .Cast<CouponCategoryActivatorDTO>()
                        .ToList();
                    var toCategoryActivators = toDetails.Activators.Where(x => x is CouponCategoryActivatorDTO)
                        .Cast<CouponCategoryActivatorDTO>()
                        .ToList();

                    if (!CollectionsHelper.AreCollectionsEqual(
                            fromCategoryActivators,
                            toCategoryActivators,
                            x => x.CategoryCode))
                    {
                        var categoryIds = fromCategoryActivators.Select(x => x.CategoryCode)
                            .Union(toCategoryActivators.Select(x => x.CategoryCode));
                        var categories = await _integrationDataContext.ProductCategories.Where(x => categoryIds.Contains(x.Code))
                            .ToListAsync();

                        var fromCategories = fromCategoryActivators.Join(
                                categories,
                                x => x.CategoryCode,
                                x => x.Code,
                                (activator, category) => $"{category.Title} ({category.Code})")
                            .OrderBy(x => x);
                        var toCategories = toCategoryActivators.Join(
                                categories,
                                x => x.CategoryCode,
                                x => x.Code,
                                (activator, category) => $"{category.Title} ({category.Code})")
                            .OrderBy(x => x);

                        logHistoryFields.Add(new LogHistoryField($"змінив(ла) категорії-активатори для {entity}", string.Join("; ", fromCategories), string.Join("; ", toCategories)));
                    }

                    var fromProductActivators = fromDetails.Activators.Where(x => x is CouponProductActivatorDTO)
                        .Cast<CouponProductActivatorDTO>()
                        .ToList();
                    var toProductActivators = toDetails.Activators.Where(x => x is CouponProductActivatorDTO)
                        .Cast<CouponProductActivatorDTO>()
                        .ToList();


                    if (!CollectionsHelper.AreCollectionsEqual(
                            fromProductActivators,
                            toProductActivators,
                            x => x.ProductCode,
                            new CouponProductActivatorDTOComparer()))
                    {
                        var productIds = fromProductActivators.Select(x => x.ProductCode)
                            .Union(toProductActivators.Select(x => x.ProductCode));
                        var products = await _integrationDataContext.Products.Where(x => productIds.Contains(x.Code))
                            .ToListAsync();

                        var fromProducts = fromProductActivators.Join(
                                products,
                                x => x.ProductCode,
                                x => x.Code,
                                (activator, product) => $"{product.Title} ({activator.Quantity} шт.)")
                            .OrderBy(x => x);
                        var toProducts = toProductActivators.Join(
                                products,
                                x => x.ProductCode,
                                x => x.Code,
                                (activator, product) => $"{product.Title} ({activator.Quantity} шт.)")
                            .OrderBy(x => x);

                        logHistoryFields.Add(new LogHistoryField($"змінив(ла) товари-активатори для {entity}", string.Join("; ", fromProducts), string.Join("; ", toProducts)));
                    }

                    var fromBrandActivators = fromDetails.Activators.Where(x => x is CouponBrandActivatorDTO)
                        .Cast<CouponBrandActivatorDTO>()
                        .ToList();
                    var toBrandActivators = toDetails.Activators.Where(x => x is CouponBrandActivatorDTO)
                        .Cast<CouponBrandActivatorDTO>()
                        .ToList();

                    if (!CollectionsHelper.AreCollectionsEqual(
                            fromBrandActivators,
                            toBrandActivators,
                            x => x.BrandCode))
                    {
                        var brandIds = fromBrandActivators.Select(x => x.BrandCode)
                            .Union(toBrandActivators.Select(x => x.BrandCode));
                        var brands = await _integrationDataContext.ProductBrands.Where(x => brandIds.Contains(x.Code))
                            .ToListAsync();

                        var fromBrands = fromBrandActivators.Join(
                                brands,
                                x => x.BrandCode,
                                x => x.Code,
                                (activator, brand) => $"{brand.Title} ({brand.Code})")
                            .OrderBy(x => x);
                        var toBrands = toBrandActivators.Join(
                                brands,
                                x => x.BrandCode,
                                x => x.Code,
                                (activator, brand) => $"{brand.Title} ({brand.Code})")
                            .OrderBy(x => x);

                        logHistoryFields.Add(new LogHistoryField($"змінив(ла) бренди-активатори для {entity}", string.Join("; ", fromBrands), string.Join("; ", toBrands)));
                    }


                    var fromSupplierActivators = fromDetails.Activators.Where(x => x is CouponSupplierActivatorDTO)
                        .Cast<CouponSupplierActivatorDTO>()
                        .ToList();
                    var toSupplierActivators = toDetails.Activators.Where(x => x is CouponSupplierActivatorDTO)
                        .Cast<CouponSupplierActivatorDTO>()
                        .ToList();

                    if (!CollectionsHelper.AreCollectionsEqual(
                            fromSupplierActivators,
                            toSupplierActivators,
                            x => x.SupplierCode))
                    {
                        var supplierIds = fromSupplierActivators.Select(x => x.SupplierCode)
                            .Union(toSupplierActivators.Select(x => x.SupplierCode));
                        var suppliers = await _integrationDataContext.ProductsSuppliers.Where(x => supplierIds.Contains(x.Code))
                            .ToListAsync();

                        var fromSuppliers = fromSupplierActivators.Join(
                                suppliers,
                                x => x.SupplierCode,
                                x => x.Code,
                                (activator, supplier) => $"{supplier.Title} ({supplier.Code})")
                            .OrderBy(x => x);
                        var toSuppliers = toSupplierActivators.Join(
                                suppliers,
                                x => x.SupplierCode,
                                x => x.Code,
                                (activator, supplier) => $"{supplier.Title} ({supplier.Code})")
                            .OrderBy(x => x);

                        logHistoryFields.Add(new LogHistoryField($"змінив(ла) постачальників-активаторів для {entity}",
                            string.Join("; ", fromSuppliers), string.Join("; ", toSuppliers)));
                    }

                    var fromManufacturerActivators = fromDetails.Activators.Where(x => x is CouponManufacturerActivatorDTO)
                        .Cast<CouponManufacturerActivatorDTO>()
                        .ToList();
                    var toManufacturerActivators = toDetails.Activators.Where(x => x is CouponManufacturerActivatorDTO)
                        .Cast<CouponManufacturerActivatorDTO>()
                        .ToList();

                    if (!CollectionsHelper.AreCollectionsEqual(
                            fromManufacturerActivators,
                            toManufacturerActivators,
                            x => x.ManufacturerCode))
                    {
                        var manufacturerIds = fromManufacturerActivators.Select(x => x.ManufacturerCode)
                            .Union(toManufacturerActivators.Select(x => x.ManufacturerCode));
                        var manufacturers = await _integrationDataContext.ProductManufacturers.Where(x => manufacturerIds.Contains(x.Code))
                            .ToListAsync();

                        var fromManufacturers = fromManufacturerActivators.Join(
                                manufacturers,
                                x => x.ManufacturerCode,
                                x => x.Code,
                                (activator, manufacturer) => $"{manufacturer.Title} ({manufacturer.Code})")
                            .OrderBy(x => x);
                        var toManufacturers = toManufacturerActivators.Join(
                                manufacturers,
                                x => x.ManufacturerCode,
                                x => x.Code,
                                (activator, manufacturer) => $"{manufacturer.Title} ({manufacturer.Code})")
                            .OrderBy(x => x);

                        logHistoryFields.Add(new LogHistoryField($"змінив(ла) виробників-активаторів для {entity}", string.Join("; ", fromManufacturers), string.Join("; ", toManufacturers)));
                    }

                    if (fromDetails.AllRequired != toDetails.AllRequired)
                    {
                        logHistoryFields.Add(new LogHistoryField(
                            $"змінив(ла) умову активації для {entity}",
                            fromDetails.AllRequired ? "Обов'язковий кожен активатор" : "Обов'язковий один із активаторів",
                            toDetails.AllRequired ? "Обов'язковий кожен активатор" : "Обов'язковий один із активаторів"));
                    }
                }
            }

            var fromProductRewardCode = updatedFrom.CouponDetails is CouponCombinationPriceDiscountDTO fromTempDetails
                ? fromTempDetails.ProductCode
                : null;

            var toProductRewardCode = updatedTo.CouponDetails is CouponCombinationPriceDiscountDTO toTempDetails
                ? toTempDetails.ProductCode
                : null;
            var tempProducts = await _integrationDataContext.Products.Where(x => x.Code != null && (x.Code == fromProductRewardCode || x.Code == toProductRewardCode)).ToListAsync();

            (CouponRewardShortInfo, string Reward) fromReward = CouponService.ToReward(updatedFrom.CouponDetails, tempProducts?.FirstOrDefault(x => x.Code == fromProductRewardCode));
            (CouponRewardShortInfo, string Reward) toReward = CouponService.ToReward(updatedTo.CouponDetails, tempProducts?.FirstOrDefault(x => x.Code == toProductRewardCode));
            if (fromReward.Reward != toReward.Reward)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) нагороду для {entity}", fromReward.Reward, toReward.Reward));
            }

            if (updatedTo.UseTimes != updatedFrom.UseTimes)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) к-сть разів використання для {entity}", updatedFrom.UseTimes?.ToString(), updatedTo.UseTimes?.ToString()));
            }

            if (updatedTo.Emission != updatedFrom.Emission)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) емісію для {entity}", updatedFrom.Emission?.ToString(), updatedTo.Emission?.ToString()));
            }

            if (!CollectionsHelper.AreCollectionsEqual(updatedTo.CouponToTarget, updatedFrom.CouponToTarget, x => x.TargetId))
            {
                updatedTo.CouponToTarget ??= new();
                updatedFrom.CouponToTarget ??= new();

                var targetIds = updatedTo.CouponToTarget.UnionBy(updatedFrom.CouponToTarget, x => x.TargetId).Select(x => x.TargetId);

                var targets = await _unitOfWork.Targets.GetAll().Where(x => targetIds.Contains(x.Id)).ToListAsync();

                var from = targets.Where(x => updatedFrom.CouponToTarget.Select(c => c.TargetId).Contains(x.Id)).OrderBy(x => x.Id).ToList();
                var to = targets.Where(x => updatedTo.CouponToTarget.Select(c => c.TargetId).Contains(x.Id)).OrderBy(x => x.Id).ToList();

                logHistoryFields.Add(new LogHistoryField($"змінив(ла) обрані правила для {entity}", string.Join("; ", from.Select(x => x.Name)), string.Join("; ", to.Select(x => x.Name))));
            }

            if (updatedTo.EmissionBy != updatedFrom.EmissionBy)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) тип меж емісії для {entity}", updatedFrom.EmissionBy.GetDisplayName(), updatedTo.EmissionBy.GetDisplayName()));
            }
        }

        return (logHistoryFields, updatedTo, updatedFrom);
    }

    private async Task<(List<LogHistoryField>, AccumulationCardLogHistoryDTO, AccumulationCardLogHistoryDTO)>
        DeserializeAccumulationCardLogHistory(LogHistory log, bool withEntity)
    {
        var updatedTo = !string.IsNullOrEmpty(log.UpdatedTo)
            ? JsonConvert.DeserializeObject<AccumulationCardLogHistoryDTO>(log.UpdatedTo)
            : new AccumulationCardLogHistoryDTO();
        var updatedFrom = !string.IsNullOrEmpty(log.UpdatedFrom)
            ? JsonConvert.DeserializeObject<AccumulationCardLogHistoryDTO>(log.UpdatedFrom)
            : new AccumulationCardLogHistoryDTO();

        var accumulationCard =
            await _integrationDataContext.AccumulationCards.FirstOrDefaultAsync(x => x.Id == log.EntityId);
        var entity = (withEntity && accumulationCard != null) ? $"картка+1 {accumulationCard.Name}" : "картка+1";
        var logHistoryFields = new List<LogHistoryField>();

        if (log.Action == "Inserted")
        {
            logHistoryFields.Add(new LogHistoryField() { Title = $"створив(ла) {entity}" });
        }

        if (log.Action == "Deleted")
        {
            entity = withEntity ? $"картку+1 {updatedFrom?.Name}" : "картку+1";
            logHistoryFields.Add(new LogHistoryField() { Title = $"видалив(ла) {entity}" });
        }

        if (log.Action == "Updated")
        {

            if (updatedTo!.Status == AccumulationCardStatus.Active)
            {
                logHistoryFields.Add(new LogHistoryField() { Title = $"опубліковав(ла) {entity}" });
            }

            if (updatedTo.Status == AccumulationCardStatus.Archived)
            {
                logHistoryFields.Add(new LogHistoryField() { Title = $"архівував(ла) {entity}" });
            }

            if (updatedTo.Name != updatedFrom!.Name)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) назву для {entity}", updatedFrom.Name, updatedTo.Name));
            }

            if (updatedTo.Description != updatedFrom.Description)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) опис для {entity}", updatedFrom.Description, updatedTo.Description));
            }

            if (updatedTo.AllRequired != updatedFrom.AllRequired)
            {
                logHistoryFields.Add(new LogHistoryField(
                    $"змінив(ла) умову активації для {entity}",
                    (updatedFrom.AllRequired ?? false) ? "Обов'язковий кожен активатор" : "Обов'язковий один із активаторів",
                    (updatedTo.AllRequired ?? false) ? "Обов'язковий кожен активатор" : "Обов'язковий один із активаторів"));
            }

            if (updatedTo.CountToComplete != updatedFrom.CountToComplete)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) к-сть зібрання для бонусу для {entity}", updatedFrom.CountToComplete?.ToString(), updatedTo.CountToComplete?.ToString()));
            }

            if (!CollectionsHelper.AreCollectionsEqual(updatedTo.AccumulationCardToTarget, updatedFrom.AccumulationCardToTarget, x => x.TargetId))
            {
                updatedTo.AccumulationCardToTarget ??= new();
                updatedFrom.AccumulationCardToTarget ??= new();

                var targetIds = updatedTo.AccumulationCardToTarget.UnionBy(updatedFrom.AccumulationCardToTarget, x => x.TargetId).Select(x => x.TargetId);

                var targets = await _unitOfWork.Targets.GetAll().Where(x => targetIds.Contains(x.Id)).ToListAsync();

                var from = targets.Where(x => updatedFrom.AccumulationCardToTarget.Select(c => c.TargetId).Contains(x.Id)).OrderBy(x => x.Id);
                var to = targets.Where(x => updatedTo.AccumulationCardToTarget.Select(c => c.TargetId).Contains(x.Id)).OrderBy(x => x.Id);

                logHistoryFields.Add(new LogHistoryField($"змінив(ла) обрані правила для {entity}", string.Join("; ", from.Select(x => x.Name)), string.Join("; ", to.Select(x => x.Name))));
            }

            if (updatedTo.StartDate != updatedFrom.StartDate)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) дату початку для {entity}", updatedFrom.StartDate?.ToString("o"), updatedTo.StartDate?.ToString("o")));
            }

            if (updatedTo.ExpirationDate != updatedFrom.ExpirationDate)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) дату кінця для {entity}", updatedFrom.ExpirationDate?.ToString("o"), updatedTo.ExpirationDate?.ToString("o")));
            }

            if (updatedTo.CouponDescription != updatedFrom.CouponDescription)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) умови використання для {entity}", updatedFrom.CouponDescription, updatedTo.CouponDescription));
            }

            if (!CollectionsHelper.AreCollectionsEqual(updatedTo.ProductCodes, updatedFrom.ProductCodes, x => x))
            {
                updatedTo.ProductCodes ??= new ();
                updatedFrom.ProductCodes ??= new ();
                var productCodes = updatedFrom.ProductCodes.Union(updatedTo.ProductCodes);

                var products = await _integrationDataContext.Products.Where(x => productCodes.Contains(x.Code)).ToListAsync();
                var from = products.Where(x => updatedFrom.ProductCodes.Contains(x.Code)).OrderBy(x => x.Title);
                var to = products.Where(x => updatedTo.ProductCodes.Contains(x.Code)).OrderBy(x => x.Title);

                logHistoryFields.Add(new LogHistoryField($"змінив(ла) товари-активатори для {entity}", string.Join("; ", from.Select(x => x.Title)), string.Join("; ", to.Select(x => x.Title))));
            }
        }
        return (logHistoryFields, updatedTo, updatedFrom);
    }

    private async Task<List<LogHistoryDTO>> AddFeedbackHistory(List<LogHistory> feedbackLog, bool withEntity)
    {
        var result = new List<LogHistoryDTO>();
        await ProcessLogHistoryAsync<FeedbackLogHistoryDTO>(feedbackLog, false, result, DeserializeFeedbackLogHistory);

        return result;
    }

    private async Task<List<LogHistoryDTO>> AddContactUsHistory(List<LogHistory> contactUsLogs, bool withEntity)
    {
        var result = new List<LogHistoryDTO>();
        await ProcessLogHistoryAsync<ContactUsLogHistoryDTO>(contactUsLogs, withEntity, result, DeserializeContactUsLogHistory);

        return result;
    }

    private async Task<List<LogHistoryDTO>> AddCouponHistory(List<LogHistory> feedbackLog, bool withEntity)
    {
        var result = new List<LogHistoryDTO>();
        await ProcessLogHistoryAsync<CouponLogHistoryDTO>(feedbackLog, withEntity, result, DeserializeCouponLogHistory);

        return result;
    }

    private async Task<List<LogHistoryDTO>> AddAccumulationCardHistory(List<LogHistory> feedbackLog, bool withEntity)
    {
        var result = new List<LogHistoryDTO>();
        await ProcessLogHistoryAsync<AccumulationCardLogHistoryDTO>(feedbackLog, withEntity, result, DeserializeAccumulationCardLogHistory);

        return result;
    }

    private async Task<(List<LogHistoryField>, ContactUsLogHistoryDTO, ContactUsLogHistoryDTO)> DeserializeContactUsLogHistory(LogHistory log, bool withEntity)
    {
        var updatedTo = !string.IsNullOrEmpty(log.UpdatedTo) ? JsonConvert.DeserializeObject<ContactUsLogHistoryDTO>(log.UpdatedTo) : new ContactUsLogHistoryDTO();
        var updatedFrom = !string.IsNullOrEmpty(log.UpdatedFrom) ? JsonConvert.DeserializeObject<ContactUsLogHistoryDTO>(log.UpdatedFrom) : new ContactUsLogHistoryDTO();

        var contactUs = await _unitOfWork.ContactUs.GetById(log.EntityId.Value);
        var entity = (withEntity && contactUs != null) ? $"для заявки користувача {contactUs.FirstName}" : "";
        var logHistoryFields = new List<LogHistoryField>();

        if (log.Action == "Updated" && updatedTo.ResolveMessage != updatedFrom.ResolveMessage)
            logHistoryFields.Add(new LogHistoryField($"залишив(ла) відповідь {updatedTo.ResolveMessage} {entity}", string.Empty, string.Empty));

        if (log.Action == "Updated" && updatedFrom.Status != updatedTo.Status)
            logHistoryFields.Add(new LogHistoryField($"змінив(ла) статус {entity}", updatedFrom.Status.ToString(), updatedTo.Status.ToString()));

        if (log.Action == "Updated" && updatedFrom.AssigneeId != updatedTo.AssigneeId)
        {
            var fromAssignee = await GetAssigneeNameAsync(updatedFrom.AssigneeId);
            var toAssignee = await GetAssigneeNameAsync(updatedTo.AssigneeId);

            logHistoryFields.Add(new LogHistoryField($"змінив(ла) відповідального {entity}", fromAssignee, toAssignee));
        }

        if (log.Action == "Inserted")
        {
            var branch = (Branches)(updatedTo.BranchId ?? 1);

            logHistoryFields.Add(new LogHistoryField { Title = $"створила заявку від {branch.GetEnumMemberValue()} {entity}" });
        }

        return (logHistoryFields, updatedTo, updatedFrom);
    }

    private async Task<string> GetAssigneeNameAsync(int? id)
    {
        if (!id.HasValue)
            return string.Empty;
        
        var assignee = await _unitOfWork.Admins.GetById(id.Value);
        return assignee != null ? $"{assignee.FirstName} {assignee.LastName}" : "Видаленний користувач";
    }

    private async Task ProcessLogHistoryAsync<T>(
        IEnumerable<LogHistory> logHistories,
        bool withEntity,
        List<LogHistoryDTO> result,
        Func<LogHistory, bool, Task<(List<LogHistoryField>, T, T)>> deserializeFunc)
    {
        foreach (var log in logHistories)
        {
            var (logFields, updatedTo, updatedFrom) = await deserializeFunc(log, withEntity);

            if (logFields.Count > 0)
            {
                result.Add(new LogHistoryDTO
                {
                    Action = log.Action,
                    EntityId = log.EntityId,
                    EntityType = log.EntityType,
                    ActionBy = log.AdminId,
                    ActionByName = log.AdminId.HasValue ? $"{log.Admin?.FirstName} {log.Admin?.LastName}" : "Система",
                    Date = log.Date,
                    LogHistoryList = logFields
                });
            }
        }
    }

    private async Task<(List<LogHistoryField>, PageLogHistoryDTO, PageLogHistoryDTO)> DeserializePageLogHistory(LogHistory log, bool withEntity)
    {
        var updatedTo = !log.UpdatedTo.IsNullOrEmpty()
            ? JsonConvert.DeserializeObject<PageLogHistoryDTO>(log.UpdatedTo)
            : new PageLogHistoryDTO();
        var updatedFrom = !log.UpdatedFrom.IsNullOrEmpty()
            ? JsonConvert.DeserializeObject<PageLogHistoryDTO>(log.UpdatedFrom)
            : new PageLogHistoryDTO();

        var page = await _unitOfWork.Pages.GetAllPages().AsNoTracking().SingleOrDefaultAsync(x => x.Id == log.EntityId);

        string parentPageName;
        if (page?.ParentId == null)
        {
            parentPageName = page?.Name;
        }
        else
        {
            var parentPage = await _unitOfWork.Pages.GetAllPages().AsNoTracking().SingleOrDefaultAsync(x => x.Id == page.ParentId);
            parentPageName = parentPage?.Name;
        }

        var subPageName = page?.Name;

        var logHistoryFields = new List<LogHistoryField>();

        var pageName = log.EntityId is PageConstants.BirdJetHeaderAndFooterPageId or PageConstants.CatJetHeaderAndFooterPageId
            ? $"{parentPageName} ({((Branches?)page?.BranchId).GetDisplayName()})"
            : $"{parentPageName}/{subPageName}";

        if (log.Action == ActionConstant.Updated && updatedTo.Name != updatedFrom.Name)
            logHistoryFields.Add(new LogHistoryField($"змінив(ла) технічну назву в {pageName}", updatedFrom.Name, updatedTo.Name));

        if (log.Action == ActionConstant.Updated && updatedFrom.PublishedAt == null && updatedTo.PublishedAt != null)
        {
            logHistoryFields.Add(new LogHistoryField($"опублікував(ла) сторінку {pageName}", string.Empty, string.Empty));
        }

        if (log.Action == ActionConstant.Updated && updatedFrom.PublishedAt != null && updatedTo.PublishedAt != null && !updatedFrom.PublishedAt.Equals(updatedTo.PublishedAt))
        {
            logHistoryFields.Add(new LogHistoryField($"переопублікував(ла) сторінку {pageName}", string.Empty, string.Empty));
        }

        if (log.Action == ActionConstant.Updated && updatedTo.ScheduledPublishDate != updatedFrom.ScheduledPublishDate)
            logHistoryFields.Add(new LogHistoryField($"змінив(ла) дату запланованої публікації в {pageName}", updatedFrom.ScheduledPublishDate?.ToString("o"), updatedFrom.ScheduledPublishDate?.ToString("o")));


        if (log.Action == ActionConstant.Inserted && updatedTo.Id != null && updatedTo.Name != null && (updatedFrom == null || updatedFrom.Id == null))
            logHistoryFields.Add(new LogHistoryField { Title = $"створив(ла) сторінку {pageName}" });

        if (log.Action == ActionConstant.Deleted && updatedFrom.Published == false)
        {
            var parentPageId = updatedFrom.ParentId;
            var parent = parentPageId != null
                ? await _unitOfWork.Pages.GetAllPages().AsNoTracking().SingleOrDefaultAsync(x => x.Id == parentPageId)
                : null;

            logHistoryFields.Add(new LogHistoryField { Title = $"видалив(ла) сторінку {parent?.Name}/{updatedFrom.Name}" });
        }


        return (logHistoryFields, updatedTo, updatedFrom);
    }

    private async Task<(List<LogHistoryField>, SeoMetaLogHistoryDTO, SeoMetaLogHistoryDTO)> DeserializeSeoMetaLogHistory(LogHistory log, bool withEntity)
    {
        var updatedTo = !log.UpdatedTo.IsNullOrEmpty()
            ? JsonConvert.DeserializeObject<SeoMetaLogHistoryDTO>(log.UpdatedTo)
            : new SeoMetaLogHistoryDTO();
        var updatedFrom = !log.UpdatedFrom.IsNullOrEmpty()
            ? JsonConvert.DeserializeObject<SeoMetaLogHistoryDTO>(log.UpdatedFrom)
            : new SeoMetaLogHistoryDTO();

        var page = await _unitOfWork.Pages.GetAllPages().AsNoTracking().SingleOrDefaultAsync(x => x.Id == log.EntityId);

        string parentPageName;
        if (page?.ParentId == null)
        {
            parentPageName = page?.Name;
        }
        else
        {
            var parentPage = await _unitOfWork.Pages.GetAllPages().AsNoTracking().SingleOrDefaultAsync(x => x.Id == page.ParentId);
            parentPageName = parentPage?.Name;
        }

        var subPageName = page?.Name;

        var logHistoryFields = new List<LogHistoryField>();

        if (log.Action == ActionConstant.Updated && updatedTo.Title != updatedFrom.Title)
            logHistoryFields.Add(new LogHistoryField($"змінив(ла) СЕО заголовок в {parentPageName}/{subPageName}", updatedFrom.Title, updatedTo.Title));

        if (log.Action == ActionConstant.Updated && updatedTo.Description != updatedFrom.Description)
            logHistoryFields.Add(new LogHistoryField($"змінив(ла) СЕО опис в {parentPageName}/{subPageName}", updatedFrom.Description, updatedTo.Description));

        return (logHistoryFields, updatedTo, updatedFrom);
    }

    private async Task<(List<LogHistoryField>, SectionsLogHistoryDTO, SectionsLogHistoryDTO)> DeserializeSectionsLogHistory(LogHistory log, bool withEntity)
    {
        var updatedTo = !log.UpdatedTo.IsNullOrEmpty()
            ? JsonConvert.DeserializeObject<SectionsLogHistoryDTO>(log.UpdatedTo)
            : new SectionsLogHistoryDTO();
        var updatedFrom = !log.UpdatedFrom.IsNullOrEmpty()
            ? JsonConvert.DeserializeObject<SectionsLogHistoryDTO>(log.UpdatedFrom)
            : new SectionsLogHistoryDTO();

        var logHistoryFields = new List<LogHistoryField>();

        var page = await _unitOfWork.Pages.GetAllPages().AsNoTracking().SingleOrDefaultAsync(x => x.Id == log.EntityId);

        string parentPageName;
        if (page?.ParentId == null)
        {
            parentPageName = page?.Name;
        }
        else
        {
            var parentPage = await _unitOfWork.Pages.GetAllPages().AsNoTracking().SingleOrDefaultAsync(x => x.Id == page.ParentId);
            parentPageName = parentPage?.Name;
        }

        var subPageName = page?.Name;

        if (log.Action == ActionConstant.Updated && (updatedFrom != null || updatedTo != null))
        {
            var pageId = updatedTo.PageId ?? updatedFrom.PageId;

            var pageName = pageId is PageConstants.BirdJetHeaderAndFooterPageId or PageConstants.CatJetHeaderAndFooterPageId
                    ? $"{parentPageName} ({((Branches?)page?.BranchId).GetDisplayName()})"
                    : $"{parentPageName}/{subPageName}";

            var entity = withEntity
                ? $"{updatedTo.Name ?? updatedFrom.Name} в {pageName}"
                : $"{ updatedTo.Name ?? updatedFrom.Name}";

            if (updatedFrom.Name != updatedTo.Name)
            {
                logHistoryFields.Add(new LogHistoryField($"змінив(ла) назву секції {entity}", updatedFrom.Name, updatedTo.Name));
            }

            if (!CollectionsHelper.AreCollectionsEqual(
                    updatedFrom.SectionFields,
                    updatedTo.SectionFields,
                    x => x.Id,
                    new GenericEqualityComparer<SectionFieldLogHistoryDTO>()))
            {
                var sections = updatedFrom.SectionFields.FullOuterJoin(updatedTo.SectionFields, x => x.Id);

                foreach (var section in sections)
                {
                    if (section.Left != null && section.Right != null)
                    {
                        if (section.Left.Value != section.Right.Value)
                        {
                            var relatedFieldName = string.Empty;
                            var fullFieldName = $"\"{section.Right.Title}\"";
                            if (!string.IsNullOrEmpty(section.Right.RelatedTitle))
                            {
                                fullFieldName += string.IsNullOrEmpty(fullFieldName)
                                    ? section.Right.RelatedTitle
                                    : $" для {section.Right.RelatedTitle}";
                                relatedFieldName = section.Right.RelatedTitle;
                            }
                            if (!string.IsNullOrEmpty(section.Right.SubSectionTitle))
                            {
                                fullFieldName += $" в підсекції \"{section.Right.SubSectionTitle}\"";
                                relatedFieldName += $" в підсекції \"{section.Right.SubSectionTitle}\"";
                            }

                            if (section.Right.Type == "image")
                            {
                                logHistoryFields.Add(new LogHistoryField($"змінив(ла) фото для поля {fullFieldName} секції {entity}", section.Left.Value, section.Right.Value));
                            }
                            else if (section.Right.Type == "boolean" && section.Right.Key.Contains("isActive"))
                            {
                                var verb = section.Right.Value == "true" ? "активував(ла)" : "деактивув(ла)";
                                logHistoryFields.Add(new LogHistoryField($"{verb} {relatedFieldName} секції {entity}", null, null));
                            }
                            else
                            {
                                logHistoryFields.Add(new LogHistoryField($"змінив(ла) значення поля {fullFieldName} секції {entity}", section.Left.Value, section.Right.Value));
                            }

                        }
                    }
                    else if (section.Left != null)
                    {
                        var fullFieldName = $"\"{section.Left.Title}\"";
                        if (!string.IsNullOrEmpty(section.Left.RelatedTitle))
                        {
                            fullFieldName += $" для {section.Right.RelatedTitle}";
                        }
                        if (!string.IsNullOrEmpty(section.Left.SubSectionTitle))
                        {
                            fullFieldName += $" підсекції \"{section.Left.SubSectionTitle}\"";
                        }

                        logHistoryFields.Add(new LogHistoryField($"видалив поле {fullFieldName} з секції {entity}", section.Left.Value, null));
                    }
                    else if (section.Right != null)
                    {
                        var fullFieldName = $"\"{section.Right.Title}\"";
                        if (!string.IsNullOrEmpty(section.Right.RelatedTitle))
                        {
                            fullFieldName += $" для {section.Right.RelatedTitle}";
                        }
                        if (!string.IsNullOrEmpty(section.Right.SubSectionTitle))
                        {
                            fullFieldName += $" підсекції \"{section.Right.SubSectionTitle}\"";
                        }


                        logHistoryFields.Add(new LogHistoryField($"додав поле {fullFieldName} для секції {entity}", null, section.Right.Value));
                    }
                }
            }
        }

        return (logHistoryFields, updatedTo, updatedFrom);
    }

    private async Task<(List<LogHistoryField>, SectionFieldLogHistoryDTO, SectionFieldLogHistoryDTO)> DeserializeSectionFieldLogHistory(LogHistory log, bool withEntity)
    {
        var updatedTo = !log.UpdatedTo.IsNullOrEmpty()
            ? JsonConvert.DeserializeObject<SectionFieldLogHistoryDTO>(log.UpdatedTo)
            : new SectionFieldLogHistoryDTO();
        var updatedFrom = !log.UpdatedFrom.IsNullOrEmpty()
            ? JsonConvert.DeserializeObject<SectionFieldLogHistoryDTO>(log.UpdatedFrom)
            : new SectionFieldLogHistoryDTO();

        var logHistoryFields = new List<LogHistoryField>();

        var page = await _unitOfWork.Pages.GetAllPages().AsNoTracking().SingleOrDefaultAsync(x => x.Id == log.EntityId);

        string parentPageName;
        if (page?.ParentId == null)
        {
            parentPageName = page?.Name;
        }
        else
        {
            var parentPage = await _unitOfWork.Pages.GetAllPages().AsNoTracking().SingleOrDefaultAsync(x => x.Id == page.ParentId);
            parentPageName = parentPage?.Name;
        }

        var subPageName = page?.Name;

        logHistoryFields.Add(new LogHistoryField { Title = $"змінив(ла) секцію {updatedTo.Section ?? updatedFrom.Section} в {parentPageName}/{subPageName}" });

        return (logHistoryFields, updatedTo, updatedFrom);
    }


    public async Task<List<LogHistoryDTO>> GetPageByRoleId(int roleId, DateTime? timeFrom, DateTime? timeTo)
    {
        var result = new List<LogHistoryDTO>();
        var role = await _unitOfWork.Roles.GetById(roleId);
        if (role != null)
        {
            var logHistory = await _unitOfWork.LogsHistory
                .GetByEntityId(roleId, "AdminRole", timeFrom, timeTo)
                .ToListAsync();
            result.AddRange(await AddRolesLogHistory(logHistory, false, timeFrom, timeTo));
        }

        return result.OrderByDescending(x => x.Date).ToList();
    }
    private async Task<IEnumerable<LogHistoryDTO>> AddRolesLogHistory(List<LogHistory> logHistory, bool withEntity, DateTime? timeFrom, DateTime? timeTo)
    {
        var result = new List<LogHistoryDTO>();

        string? entity = "";
        foreach (var log in logHistory)
        {
            if (log != null && log.EntityId.HasValue)
            {
                var rolesToUpdate = await _unitOfWork.Roles.GetById(log.EntityId.Value);
                entity = (withEntity && rolesToUpdate != null) ? $"{rolesToUpdate.Name}" : "";
            }
            var logHistoryList = new List<LogHistoryField>();

            var UpdatedTo = !string.IsNullOrEmpty(log.UpdatedTo) ? JsonConvert.DeserializeObject<AdminRoleLogHistoryDTO>(log.UpdatedTo) : new AdminRoleLogHistoryDTO();
            var UpdateFrom = !string.IsNullOrEmpty(log.UpdatedFrom) ? JsonConvert.DeserializeObject<AdminRoleLogHistoryDTO>(log.UpdatedFrom) : new AdminRoleLogHistoryDTO();

            if (UpdatedTo.Name != null && UpdatedTo.Name != UpdateFrom.Name && log.Action == ActionConstant.Updated)
            {
                logHistoryList.Add(new LogHistoryField($"змінив(ла) назву ролі {entity}", UpdateFrom.Name, UpdatedTo.Name));
            }

            if (log.Action == ActionConstant.Inserted)
            {
                logHistoryList.Add(new LogHistoryField($"створив(ла) роль {entity}", string.Empty, string.Empty));
            }

            if (UpdatedTo.isActive == false || log.Action == ActionConstant.Deleted)
            {
                logHistoryList.Add(new LogHistoryField($"видалив(ла) роль {entity}", string.Empty, string.Empty));
            }

            if (log.Action != ActionConstant.Deleted)
            {
                var deletedPermissionIds = UpdateFrom.RoleToPermissions.Select(x => x.PermissionsId).Except(UpdatedTo.RoleToPermissions.Select(x => x.PermissionsId)).ToList();
                var deletedPermissionEntities = await _unitOfWork.RolesPermission.Find(p => deletedPermissionIds.Contains(p.Id)).ToListAsync();

                var addedPermissionIds = UpdatedTo.RoleToPermissions.Select(x => x.PermissionsId).Except(UpdateFrom.RoleToPermissions.Select(x => x.PermissionsId));
                var addedPermissionEntities = await _unitOfWork.RolesPermission.Find(p => addedPermissionIds.Contains(p.Id)).ToListAsync();

                var forEntityString = string.IsNullOrEmpty(entity)
                    ? string.Empty
                    : $" для ролі {entity}";

                var addedAccess = addedPermissionEntities.Where(x => !deletedPermissionEntities.Any(d => d.EntityType == x.EntityType))
                    .Select(x => new LogHistoryField($"додав(ла) доступ до {x.Title}{forEntityString}", string.Empty, ((PermissionLevel)x.Crud!).GetEnumMemberValue())).ToList();

                var deletedAccess = deletedPermissionEntities.Where(x => !addedPermissionEntities.Any(d => d.EntityType == x.EntityType))
                    .Select(x => new LogHistoryField($"видалив(ла) доступ до {x.Title}{forEntityString}", ((PermissionLevel)x.Crud!).GetEnumMemberValue(), string.Empty)).ToList(); ;

                var modifiedAccess = addedPermissionEntities.Join(deletedPermissionEntities, x => x.EntityType, x => x.EntityType, (x, y) =>
                    new LogHistoryField($"змінив(ла) доступ до {y.Title}{forEntityString}", ((PermissionLevel)y.Crud!).GetEnumMemberValue(), ((PermissionLevel)x.Crud!).GetEnumMemberValue())
                );

                logHistoryList.AddRange(addedAccess);
                logHistoryList.AddRange(deletedAccess);
                logHistoryList.AddRange(modifiedAccess);
            }

            if (logHistoryList.Any())
            {
                result.Add(new LogHistoryDTO
                {
                    Action = log.Action,
                    EntityId = log.EntityId,
                    EntityType = log.EntityType,
                    ActionBy = log.AdminId ?? null,
                    ActionByName = $"{log.Admin?.FirstName} {log.Admin?.LastName}",
                    Date = log.Date,
                    LogHistoryList = logHistoryList
                });
            }
        }
        return result.OrderByDescending(x => x.Date).ToList();
    }

    private string GetImagePathByName(string fileName)
    {
        return new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
        {
            Path = $"{StorageConstants.AppPath}/{fileName}"
        }.ToString();
    }
}
