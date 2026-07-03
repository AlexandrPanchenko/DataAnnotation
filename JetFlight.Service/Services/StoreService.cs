
namespace JetFlight.Service.Services;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Models.Roles;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using System.Data;
using JetFlight.Shared;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Helpers;
using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;

public interface IStoreService
{
    Task<List<StoreResponseDTO>> GetAll(int? cityId, string? serachField, int? branchId);
    Task<List<StoreResponseDTO>> GetByIds(string ids, byte? branchId = null);
    Task<List<CityResponseDTO>> GetCities(byte? branchId = null);
    Task<StoreResponseDTO> GetStore(int storeId);
    Task<StoreUpdateResponse> UpdateStore(StoreUpdateRequest store);
    Task<List<StoreResponseDTO>> GetClosestStores(byte branchId, string latitude, string longitude, int? limit);
}
public class StoreService : IStoreService
{
    private readonly IDataUnitOfWork _unitOfWork;
    private readonly IntegrationDataContext _integrationDataContext;
    private readonly IMediaService _mediaService;
    private readonly IFirebaseService _firebaseService;

    public StoreService(
        IDataUnitOfWork unitOfWork, IntegrationDataContext integrationDataContext, IMediaService mediaService, IFirebaseService firebaseService)
    {
        _unitOfWork = unitOfWork;
        _integrationDataContext = integrationDataContext;
        _mediaService = mediaService;
        _firebaseService = firebaseService;
    }

    public async Task<List<StoreResponseDTO>> GetAll(int? cityId, string? searchField, int? branchId)
    {
        var stores = await GetStoresByBranchId(branchId);
        stores = FilterStoresByCity(stores, cityId);
        stores = FilterStoresBySearchField(stores, searchField);

        stores = SortStoresByAddress(stores);

        return stores.Select(MapToStoreResponseDTO).ToList();
    }

    private async Task<List<Store>> GetStoresByBranchId(int? branchId)
    {
        return branchId != null
            ? await _unitOfWork.Stores.GetAllStores().Where(x => x.BranchId == branchId).ToListAsync()
            : await _unitOfWork.Stores.GetAllStores().ToListAsync();
    }

    private List<Store> FilterStoresByCity(IEnumerable<Store> stores, int? cityId)
    {
        return cityId != null ? stores.Where(x => x.City.Id == cityId).ToList() : stores.ToList();
    }

    private List<Store> FilterStoresBySearchField(IEnumerable<Store> stores, string? searchField)
    {
        return !string.IsNullOrEmpty(searchField)
            ? stores.Where(x => x.Address.ToLower().Contains(searchField.ToLower())).ToList()
            : stores.ToList();
    }

    private static List<Store> SortStoresByAddress(IEnumerable<Store> stores)
    {
        return stores
            .OrderBy(s => StoreAddressSortHelper.GetStreetTypeOrder(s.Address))
            .ThenBy(s => StoreAddressSortHelper.GetStreetNamePart(s.Address), StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private StoreResponseDTO MapToStoreResponseDTO(Store store)
    {
        return new StoreResponseDTO
        {
            Address = store.Address,
            Address2 = store.Address2,
            City = store.City?.Name ?? string.Empty,
            Region = store.Region,
            Latitude = store.Latitude,
            Longitude = store.Longitude,
            Title = store.Title,
            IsActive = store.isActive,
            CityId = store.City?.Id,
            Id = store.Id,
            BranchId = store.BranchId,
            ImagePath = GetStoreImagePath(store),
            WorkingHours = MapToWorkingHoursDTO(store.WorkingHours),
            MapLink = store.MapLink,
            StoreCode = store.Number
        };
    }

    private string GetStoreImagePath(Store store)
    {
        if (store.MediaFile != null && store.MediaFile.Name != null)
        {
            return GetImagePathByName(store.MediaFile.Name);
        }

        return store.BranchId == Convert.ToByte(Branches.CatJet)
            ? GetImagePathByName(BranchImageConstants.CatJet)
            : GetImagePathByName(BranchImageConstants.BirdJet);
    }

    private List<WorkingHoursDTO> MapToWorkingHoursDTO(IEnumerable<WorkingHours> workingHours)
    {
        return workingHours != null
            ? workingHours.OrderBy(x => x.Day).Select(wh => new WorkingHoursDTO
            {
                ClosingTime = wh.ClosingTime,
                OpeningTime = wh.OpeningTime,
                WorkingHoursId = wh.WorkingHoursId,
                Day = wh.Day,
                Date = wh.Date,
                Note = wh.Note,
                IsActive = wh.IsActive
            }).ToList()
            : new List<WorkingHoursDTO>();
    }


    private string GetImagePathByName(string fileName)
    {
        return new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
        {
            Path = $"{StorageConstants.AppPath}/{fileName}"
        }.ToString();
    }

    public async Task<StoreResponseDTO> GetStore(int storeId)
    {
        var store = await _unitOfWork.Stores.GetAllStores().Where(x => x.Id == storeId).FirstOrDefaultAsync();
        if (store == null) return new StoreResponseDTO();
        var res = new StoreResponseDTO
        {
            Address = store.Address,
            Address2 = store.Address2,
            City = store.City.Name,
            Region = store.Region,
            Latitude = store.Latitude,
            Longitude = store.Longitude,
            Title = store.Title,
            IsActive = store.isActive,
            CityId = store.City?.Id,
            Id = store.Id,
            StoreCode = store.Number,
            BranchId = store.BranchId,
            ImagePath = store.MediaFile != null && store.MediaFile.Name != null ? GetImagePathByName(store.MediaFile.Name) : store.BranchId == Convert.ToByte(Branches.CatJet) ? GetImagePathByName(BranchImageConstants.CatJet) : GetImagePathByName(BranchImageConstants.BirdJet),
            WorkingHours = store.WorkingHours != null ? store.WorkingHours.OrderBy(x => x.WorkingHoursId).Select(wh => new WorkingHoursDTO
            {
                ClosingTime = wh.ClosingTime,
                OpeningTime = wh.OpeningTime,
                WorkingHoursId = wh.WorkingHoursId,
                Day = wh.Day.HasValue ? wh.Day.Value : null,
                Date = wh.Date.HasValue ? wh.Date.Value : null,
                Note = wh.Note,
                IsActive = wh.IsActive
            }).ToList() : new List<WorkingHoursDTO>(),
            MapLink = store.MapLink,
        };

        return res;
    }

    public async Task<List<CityResponseDTO>> GetCities(byte? branchId = null)
    {
        var query = _unitOfWork.Cities.GetAll();

        if (branchId.HasValue)
        {
            query = query.Where(s => s.Stores.Any(x => x.BranchId == branchId));
        }

        var res = await query.Select(x => new CityResponseDTO
        {
            Id = x.Id,
            Name = x.Name,

        }).ToListAsync();

        return res;
    }

    public async Task<StoreUpdateResponse> UpdateStore(StoreUpdateRequest store)
    {
        var existingResult = await _unitOfWork.Stores.GetAllStores().Where(x => x.Id == store.id).FirstOrDefaultAsync();
        var response = new RoleUpdateResponse();

        if (store.workingHours == null || !store.workingHours.Any() || store.workingHours.Count < 7)
        {
            response.Errors.Add("Необхідно додати робочі дні");
            return new StoreUpdateResponse
            {
                Errors = response.Errors
            };
        }

        if (existingResult == null)
        {
            response.Errors.Add("Магазин не знайдено");
            return new StoreUpdateResponse
            {
                Errors = response.Errors
            };
        }

        var removeStoreFromCustomerSettings = false;
        bool isChanged = false;
        if (!string.IsNullOrEmpty(store.latitude)) existingResult.Latitude = store.latitude;
        if (!string.IsNullOrEmpty(store.longitude)) existingResult.Longitude = store.longitude;
        if (!string.IsNullOrEmpty(store.title)) existingResult.Title = store.title;
        if (!string.IsNullOrEmpty(store.address)) existingResult.Address = store.address;
        if (!string.IsNullOrEmpty(store.address2)) existingResult.Address2 = store.address2;
        if (store?.cityId != null) existingResult.CityId = store.cityId.Value;
        if (store?.region != null) existingResult.Region = store.region;
        if (store?.isActive != null)
        {
            removeStoreFromCustomerSettings = existingResult.isActive && !store.isActive.Value;
            existingResult.isActive = store.isActive.Value;
        }
        if (store?.mapLink != null) existingResult.MapLink = store.mapLink;

        if (store?.workingHours != null && store.workingHours.Count != 0)
        {
            // Identify working hours to be removed
            var workingHoursIdsToKeep = store.workingHours.Select(wh => wh.WorkingHoursId).ToList();
            var workingHoursToRemove = existingResult.WorkingHours
                .Where(wh => !workingHoursIdsToKeep.Contains(wh.WorkingHoursId))
                .ToList();

            // Remove working hours that are not in the branch.workingHours
            foreach (var workingHour in workingHoursToRemove)
            {
                existingResult.WorkingHours.Remove(workingHour);
            }
            // Update or add working hours
            foreach (var workingHoursDTO in store.workingHours)
            {

                var existingWorkingHours = existingResult.WorkingHours
                    .FirstOrDefault(wh => wh.WorkingHoursId == workingHoursDTO.WorkingHoursId);

                 isChanged = existingWorkingHours != null &&
                        (existingWorkingHours.OpeningTime != workingHoursDTO.OpeningTime ||
                        existingWorkingHours.ClosingTime != workingHoursDTO.ClosingTime ||
                        existingWorkingHours.Day != workingHoursDTO.Day ||
                        existingWorkingHours.Date != workingHoursDTO.Date);

                if (existingWorkingHours != null)
                {
                    existingWorkingHours.OpeningTime = workingHoursDTO.OpeningTime;
                    existingWorkingHours.ClosingTime = workingHoursDTO.ClosingTime;
                    existingWorkingHours.Note = workingHoursDTO.Note;
                    existingWorkingHours.IsActive = workingHoursDTO.IsActive;
                    existingWorkingHours.Day = workingHoursDTO.Day;
                    existingWorkingHours.Date = workingHoursDTO.Date;
                }
                else
                {
                    existingResult.WorkingHours.Add(new WorkingHours
                    {
                        Day = workingHoursDTO.Day,
                        OpeningTime = workingHoursDTO.OpeningTime,
                        ClosingTime = workingHoursDTO.ClosingTime,
                        Note = workingHoursDTO.Note,
                        IsActive = workingHoursDTO.IsActive,
                        Date = workingHoursDTO.Date,
                        StoreId = store.id
                    });
                    isChanged = true;
                }

                if (isChanged)
                {
                    var customerSettings = await _integrationDataContext.CustomerSettings
                        .Where(cs => cs.ActiveStoreId == store.id).ToListAsync();

                    foreach (var cs in customerSettings)
                    {
                        try
                        {
                            var title = "Змінено графік роботи магазину";
                            var body = "Графік роботи магазину було оновлено. Перевірте новий розклад.";
                            await _firebaseService.SendMessageAsync(title, body, "working_hours", cs.BranchId, cs.CustomerId);
                        }
                        catch
                        {
                            continue;
                        }

                    }
                }

                await _unitOfWork.Save();
            }
        }

        if (store?.file != null)
        {
            var newFilePath = await _mediaService.UploadAsync(store.file);
            var newFileName = Path.GetFileName(newFilePath.ToString());

            if (existingResult.MediaFile != null)
            {
                existingResult.MediaFile.MimeType = store.file.ContentType;
                existingResult.MediaFile.Name = newFileName;
                existingResult.MediaFile.Size = store.file.Length.ToString();
                existingResult.MediaFile.UpdatedAt = DateTime.UtcNow.SetKindUtc();
            }
            else
            {
                existingResult.MediaFile = new MediaFiles
                {
                    MimeType = store.file.ContentType,
                    Name = newFileName,
                    Size = store.file.Length.ToString(),
                    CreatedAt = DateTime.UtcNow.SetKindUtc()

                };
            }
        }
        await _unitOfWork.Save();

        if (removeStoreFromCustomerSettings)
        {
            await _integrationDataContext.CustomerSettings
                    .Where(x =>
                    x.ActiveStoreId == store!.id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(v => v.ActiveStoreId, v => null));
        }

        await _integrationDataContext.Set<QuestionarySelectOption>()
            .Where(x => x.Key == store!.id.ToString()
                && x.QuestionaryField.Name == PersonalDataQuestionaryConstants.Name
                && x.QuestionaryField.QuestionaryId.HasValue && x.QuestionaryField.Questionary!.IsLocked
                && x.QuestionaryField.Questionary!.Name == PersonalDataQuestionaryConstants.Name)
            .ExecuteUpdateAsync(s =>
                    s.SetProperty(v => v.Value, v => $"{existingResult.Address} {existingResult.Address2}"));
                
        return new StoreUpdateResponse
        {
            Item = new StoreResponseDTO { Id = existingResult.Id }
        };

    }

    public async Task<List<StoreResponseDTO>> GetClosestStores(byte branchId, string latitude, string longitude, int? limit)
    {
        var stores = await _unitOfWork.Stores.GetClosestStores(branchId, latitude, longitude, limit);
        return stores.Select(MapToStoreResponseDTO).ToList();
    }

    public async Task<List<StoreResponseDTO>> GetByIds(string ids, byte? branchId = null)
    {
        var idList = ids.Split(',').Select(int.Parse).ToList();
        var query = _unitOfWork.Stores.GetAllStores().Where(x => idList.Contains(x.Id));
        if (branchId.HasValue)
        {
            query = query.Where(x => x.BranchId == branchId.Value);
        }

        var stores = await query.ToListAsync();
        var result = stores.Select(MapToStoreResponseDTO).ToList();
        return result;
    }
}

