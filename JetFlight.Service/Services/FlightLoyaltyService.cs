using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Extensions;
using JetFlight.Shared.Constants;
using JetFlight.Shared;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.Product;
using JetFlight.Shared.Models.Promotion;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.UserContext;
using Microsoft.EntityFrameworkCore;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.Shared.Models.Users;
using Microsoft.Extensions.Caching.Memory;
using JetFlight.Shared.Models.Store;
using MassTransit.Initializers;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.ApplicationDataAccess;
using JetFlight.Shared.Models.FlightLoyalty;
using Microsoft.Extensions.Options;

namespace JetFlight.Service.Services;

public interface IFlightLoyaltyService
{
    Task<PagedListDTO<PromotionDetailsAdminDTO>> GetAllPromotionsAdmin(
        PagingDTO pagingDto,
        string? searchParam = null,
        DateOnly? startedDate = null,
        string? cities = null,
        string? promotionTagIds = null,
        string? categoryCode = null,
        string? promotionTypeNavisionId = null);
    Task<bool> DeleteSavedPromotion(int promotionId);
    Task<bool> AddSavedPromotion(AddSavedPromotionDTO addSavedPromotionDto);
    Task<List<PromotionsTypeDTO>> GetAllPromotionTypes(bool all = false);
    Task<PagedListDTO<PromotionDetailsClientDTO>> GetAllSavedPromotions(PagingDTO pagingDto, string? searchParam = null, SortingEnum? sortOption = null);
    Task<List<PromotionsCategoryDTO>> GetAllPromotionCategories(bool all = false);
    Task<List<PromotionsTagDTO>> GetAllPromotionTags(bool all = false);
    Task<bool> UpdatePromotionType(int id, UpdatePromotionTypeDTO updateDto);
    Task<bool> UpdatePromotionCategory(string categoryCode, UpdatePromotionCategoryDTO updateDto);
    Task<bool> UpdatePromotionTag(int id, UpdatePromotionTagDTO updateDto);
    Task<bool> UpdatePromotion(int promotionId, UpdatePromotionDTO updatePromotionDto);
    Task<PagedListDTO<PromotionDetailsClientDTO>> GetAllPromotionsClient(
        byte branchId,
        RegistrationPlatform platform,
        PagingDTO pagingDto,
        string? searchParam = null,
        DateOnly? createdDate = null,
        int? store = null,
        SortingEnum? sortOption = null,
        string? promotionTagIds = null,
        string? categoryCode = null,
        string? promotionTypeNavisionId = null,
        string? token = null);

    Task<PromotionDetailsClientDTO?> GetPromotionByIdAsync(
        byte branchId,
        int promotionId);

    Task<PromotionDisplayRuleDTO> GetDisplayRuleAsync(Branches branchId);
    Task<List<PromotionDisplayRuleDTO>> GetDisplayRulesAsync();
    Task UpdateDisplayRuleAsync(PromotionDisplayRuleDTO model);
    Task<SavedPromotionNotificationResultDTO> TriggerSavedPromotionExpirationNotificationManuallyAsync(int? promotionId = null);

    decimal GetTicketDiscount(Promotion promotion, Ticket ticket);

    Task<decimal> GetTicketDiscountAsync(int loyaltyOfferId, Ticket ticket);
}

public class FlightLoyaltyService : IFlightLoyaltyService
{
    private readonly IMemoryCache _cache;
    private readonly IntegrationDataContext _context;
    IUserContext _userContext;
    private readonly IDataUnitOfWork _unitOfWork;
    private readonly IMediaService _mediaService;
    private readonly IJobSchedulerService _jobSchedulerService;
    private readonly ApplicationDataContext _applicationDataContext;
    private readonly IFirebaseService _firebaseService;
    private readonly INotificationService _notificationService;
    private readonly IHtmlGenerationService _htmlGenerationService;
    private readonly SmsSettings _smsSettings;
    private readonly Dictionary<string, List<PromotionItemDTO>> _complexPromotionItemsCache = new();

    public FlightLoyaltyService(IntegrationDataContext context,
        IUserContext userContext,
        IDataUnitOfWork unitOfWork,
        IMemoryCache cache,
        IGlobalSearchService elasticService,
        IMediaService mediaService,
        IJobSchedulerService jobSchedulerService,
        ApplicationDataContext applicationDataContext,
        IFirebaseService firebaseService,
        INotificationService notificationService,
        IHtmlGenerationService htmlGenerationService,
        IOptions<SmsSettings> smsSettings)
    {
        _context = context;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mediaService = mediaService;
        _jobSchedulerService = jobSchedulerService;
        _applicationDataContext = applicationDataContext;
        _firebaseService = firebaseService;
        _notificationService = notificationService;
        _htmlGenerationService = htmlGenerationService;
        _smsSettings = smsSettings.Value;
    }

    public decimal GetTicketDiscount(Promotion promotion, Ticket ticket)
    {
        if (string.IsNullOrWhiteSpace(promotion.EligibleAirportIds))
        {
            return 0m;
        }

        if (promotion.EligibleAirportIds.Contains(ticket.DestinationAirportId.ToString()))
        {
            return promotion.Price - promotion.PromoPrice;
        }

        return 0m;
    }

    public async Task<decimal> GetTicketDiscountAsync(int loyaltyOfferId, Ticket ticket)
    {
        var promotion = await _context.Promotions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == loyaltyOfferId && !p.InActive);

        if (promotion == null)
        {
            return 0m;
        }

        return GetTicketDiscount(promotion, ticket);
    }

    public async Task<bool> DeleteSavedPromotion(int promotionId)
    {
        // Retrieve the CustomerId from the user context
        var customerId = _userContext.CustomerId;

        if (customerId == null)
        {
            return false; // CustomerId is not available
        }

        // Find the saved promotion
        var savedPromotion = await _context.SavedPromotions
            .FirstOrDefaultAsync(sp => sp.CustomerId == customerId && sp.PromotionId == promotionId);

        if (savedPromotion == null)
        {
            return false; // Saved promotion not found
        }

        // Remove the saved promotion
        _context.SavedPromotions.Remove(savedPromotion);
        await _context.SaveChangesAsync();

        // Check if there are any other saved promotions for this promotion
        var hasOtherSavedPromotions = await _context.SavedPromotions
            .AnyAsync(sp => sp.PromotionId == promotionId);

        // If no other users have saved this promotion, remove the scheduled notification
        if (!hasOtherSavedPromotions)
        {
            await _jobSchedulerService.RemoveSavedPromotionStartNotificationJobAsync(promotionId);
            await _jobSchedulerService.RemoveSavedPromotionDayBeforeExpirationJobAsync(promotionId);
        }

        return true;
    }

    public async Task<bool> AddSavedPromotion(AddSavedPromotionDTO addSavedPromotionDto)
    {
        // Retrieve the CustomerId from the user context
        var customerId = _userContext.CustomerId;

        if (customerId == null)
        {
            return false; 
        }

        var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.Id == addSavedPromotionDto.PromotionId);
        if (promotion == null)
        {
            return false; 
        }

        var savedPromotionExists = await _context.SavedPromotions
            .AnyAsync(sp => sp.CustomerId == customerId && sp.PromotionId == addSavedPromotionDto.PromotionId);
        if (savedPromotionExists)
        {
            return false; 
        }

        var savedPromotion = new SavedPromotion
        {
            CustomerId = customerId.Value,
            PromotionId = addSavedPromotionDto.PromotionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SavedPromotions.Add(savedPromotion);
        await _context.SaveChangesAsync();

        await ScheduleSavedPromotionNotificationsAsync(promotion);

        return true;
    }
    public async Task<PagedListDTO<PromotionDetailsClientDTO>> GetAllSavedPromotions(
        PagingDTO pagingDto,
        string? searchParam = null,
        SortingEnum? sortOption = null)
    {
        var customerId = GetCustomerId();

        var query = BuildSavedPromotionsQuery(customerId, searchParam);

        query = ApplySorting(query, sortOption);

        var savedPromotions = await query.GetPagedListAsync(pagingDto, sp => MapToPromotionDetailsClientDTO(sp));

        return savedPromotions;
    }

    private async Task ScheduleSavedPromotionNotificationsAsync(Promotion promotion)
    {
        var nowUtc = DateTime.UtcNow;
        Console.WriteLine(
            $"[ScheduleSavedPromotionNotifications] PromotionId={promotion.Id}, StartedAtUtc={promotion.StartedAt:o}, ExpiredAtUtc={promotion.ExpiredAt:o}, NowUtc={nowUtc:o}");

        if (promotion.StartedAt > DateTime.UtcNow)
        {
            Console.WriteLine(
                $"[ScheduleSavedPromotionNotifications] Scheduling START job for PromotionId={promotion.Id}");
            await _jobSchedulerService.SetSavedPromotionStartNotificationJobAsync(
                promotion.Id,
                promotion.StartedAt,
                checkIfExist: true);
        }
        else
        {
            Console.WriteLine(
                $"[ScheduleSavedPromotionNotifications] Skipping START job for PromotionId={promotion.Id} (StartedAt <= NowUtc)");
        }

        if (promotion.ExpiredAt > DateTime.UtcNow)
        {
            Console.WriteLine(
                $"[ScheduleSavedPromotionNotifications] Scheduling EXPIRES job for PromotionId={promotion.Id}");
            await _jobSchedulerService.SetSavedPromotionDayBeforeExpirationJobAsync(
                promotion.Id,
                promotion.ExpiredAt,
                checkIfExist: true);
        }
        else
        {
            Console.WriteLine(
                $"[ScheduleSavedPromotionNotifications] Skipping EXPIRES job for PromotionId={promotion.Id} (ExpiredAt <= NowUtc)");
        }
    }

    private async Task<PromotionDetailsClientDTO> MapToPromotionDetailsClientDTO(SavedPromotion sp)
    {
        return new PromotionDetailsClientDTO
        {
            Promotion = sp.Promotion != null ? await ToClientPromotionDTO(sp.Promotion) : null,
            PromotionStores = !string.IsNullOrEmpty(sp.Promotion?.StoreCode)
                ? new List<PromotionStoreDTO> { ToPromotionStoreDTO(sp.Promotion.StoreCode, sp.Promotion.Id) }
                : new List<PromotionStoreDTO>(),
            PromotionsTag = sp.Promotion?.Product?.ProductTags?.Select(tag => new PromotionsTagDTO
            {
                Title = tag.ProductsTag?.Title,
                IsActive = tag.ProductsTag?.IsActive ?? false,
                Icon = tag.ProductsTag?.Icon
            }).ToList() ?? new List<PromotionsTagDTO>(),
            PromotionsType = sp.Promotion?.PromotionType != null ? new PromotionsTypeDTO
            {
                Id = sp.Promotion.PromotionType.Id,
                IsActive = sp.Promotion.PromotionType.IsActive,
                Title = sp.Promotion.PromotionType.Title,
            } : null,
            PromotionItems = sp.Promotion != null
                ? GetPromotionItems(sp.Promotion).ToList()
                : new List<PromotionItemDTO>()
        };
    }

    private int GetCustomerId()
    {
        var customerId = _userContext.CustomerId;

        if (customerId == null)
        {
            throw new InvalidOperationException("CustomerId is not set in the user context.");
        }

        return customerId.Value;
    }


    private IQueryable<SavedPromotion> ApplySorting(IQueryable<SavedPromotion> query, SortingEnum? sortOption)
    {
        if (!sortOption.HasValue)
        {
            return query;
        }

        return sortOption.Value switch
        {
            SortingEnum.HighestPrice => query.OrderByDescending(sp => sp.Promotion != null ? sp.Promotion.PromoPrice : 0),
            SortingEnum.LowestPrice => query.OrderBy(sp => sp.Promotion != null ? sp.Promotion.PromoPrice : 0),
            SortingEnum.NewestPromotions => query.OrderByDescending(sp => sp.CreatedAt),
            // Display title: як на клієнті — complex: Offer ?? Product.Title ?? Title; regular: Product.Title ?? Title
            SortingEnum.AlphabeticalAsc => query
                .OrderBy(sp => sp.Promotion == null ? 1 : 0)
                .ThenBy(sp => ((sp.Promotion!.IsComplexPromotion ? (sp.Promotion.Offer ?? (sp.Promotion.Product != null ? sp.Promotion.Product.Title : null)) : (sp.Promotion.Product != null ? sp.Promotion.Product.Title : null)) ?? sp.Promotion.Title ?? "").ToLower()),
            SortingEnum.AlphabeticalDesc => query
                .OrderBy(sp => sp.Promotion == null ? 1 : 0)
                .ThenByDescending(sp => ((sp.Promotion!.IsComplexPromotion ? (sp.Promotion.Offer ?? (sp.Promotion.Product != null ? sp.Promotion.Product.Title : null)) : (sp.Promotion.Product != null ? sp.Promotion.Product.Title : null)) ?? sp.Promotion.Title ?? "").ToLower()),
            SortingEnum.StartDateDesc => query.OrderByDescending(sp => sp.Promotion != null ? sp.Promotion.StartedAt : DateTime.MinValue),
            SortingEnum.StartDateAsc => query.OrderBy(sp => sp.Promotion != null ? sp.Promotion.StartedAt : DateTime.MaxValue),
            _ => query
        };
    }

    private IQueryable<SavedPromotion> BuildSavedPromotionsQuery(int customerId, string? searchParam)
    {
        var query = _context.SavedPromotions
            .Include(sp => sp.Promotion)
                .ThenInclude(p => p.Product)
                    .ThenInclude(pt => pt.ProductTags)
                        .ThenInclude(pt => pt.ProductsTag)
            .Include(sp => sp.Promotion)
                .ThenInclude(p => p.Product)
                    .ThenInclude(prod => prod.Brand)
                        .ThenInclude(brand => brand.Manufacturer)
            .Include(sp => sp.Promotion)
                .ThenInclude(p => p.Product)
                    .ThenInclude(prod => prod.Family)
                        .ThenInclude(family => family.Category)
            .Include(sp => sp.Promotion)
                .ThenInclude(p => p.PromotionType)
            .Where(sp => sp.CustomerId == customerId)
            .AsQueryable();

        // Filter by user's branch if available
        if (_userContext.BranchId.HasValue)
        {
            var branchId = (byte)_userContext.BranchId.Value;
            var storeNumbers = _unitOfWork.Stores.GetAllStores()
                .Where(st => st.BranchId == branchId)
                .Select(x => x.Number)
                .ToList();

            query = query.Where(sp => sp.Promotion != null && storeNumbers.Contains(sp.Promotion.StoreCode));
        }

        if (!string.IsNullOrWhiteSpace(searchParam))
        {
            var term = searchParam.Trim().ToLower();
            var searchPatternLower = $"%{term}%";

            // Така ж логіка пошуку, як у ApplySearchFilter / GlobalSearchService:
            // Title, Product.Title, а для комплексних акцій — Offer та Description, регістронезалежно
            query = query.Where(sp => sp.Promotion != null && (
                (sp.Promotion.Title != null && EF.Functions.Like(sp.Promotion.Title.ToLower(), searchPatternLower)) ||
                (sp.Promotion.IsComplexPromotion && (
                    (sp.Promotion.Description != null && EF.Functions.Like(sp.Promotion.Description.ToLower(), searchPatternLower)) ||
                    (sp.Promotion.Offer != null && EF.Functions.Like(sp.Promotion.Offer.ToLower(), searchPatternLower))
                )) ||
                (sp.Promotion.Product != null &&
                 sp.Promotion.Product.Title != null &&
                 EF.Functions.Like(sp.Promotion.Product.Title.ToLower(), searchPatternLower))
            ));
        }

        return query;
    }


    public async Task<bool> UpdatePromotionType(int id, UpdatePromotionTypeDTO updateDto)
    {
        var promotionType = await _context.PromotionsType.FirstOrDefaultAsync(pt => pt.Id == id);

        if (promotionType == null)
        {
            return false; // Not found
        }

        if (!string.IsNullOrEmpty(updateDto.Title))
        {
            promotionType.Title = updateDto.Title;
        }
   
        // Update Position
        if (updateDto.position.HasValue)
        {
            promotionType.Position = updateDto.position.Value;
        }
        promotionType.IsActive = updateDto.IsActive ?? promotionType.IsActive;
        promotionType.UpdatedAt = DateTime.UtcNow;

        _context.PromotionsType.Update(promotionType);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdatePromotionTag(int id, UpdatePromotionTagDTO updateDto)
    {
        var promotionTag = await _context.ProductsTags.FirstOrDefaultAsync(pt => pt.Id == id);

        if (promotionTag == null)
        {
            return false; // Not found
        }
        // Update Position
        if (updateDto.position.HasValue)
        {
            promotionTag.Position = updateDto.position.Value;
        }

        if (updateDto.file != null)
        {
            promotionTag.Icon = (await _mediaService.UploadAsync(updateDto.file)).ToString();
        }
        promotionTag.IsActive = updateDto.isActive ?? promotionTag.IsActive;

        if (!string.IsNullOrEmpty(updateDto.title))
        {
            promotionTag.Title = updateDto.title;
        }

        promotionTag.UpdatedAt = DateTime.UtcNow;

        _context.ProductsTags.Update(promotionTag);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdatePromotionCategory(string categoryCode, UpdatePromotionCategoryDTO updateDto)
    {
        var category = await _context.WebProductCategories
            .FirstOrDefaultAsync(pc => pc.Code == categoryCode);

        if (category == null)
        {
            return false; // Not found
        }

        if (!string.IsNullOrEmpty(updateDto.title))
        {
            category.Title = updateDto.title;
        }

        if (updateDto.file != null)
        {
            category.Image = (await _mediaService.UploadAsync(updateDto.file)).ToString();
        }

        // Update Position and IsActive directly on WebProductCategory (new fields)
        // If isActive is null, set it to true by default
        category.IsActive = updateDto.isActive ?? true;
        if (updateDto.position.HasValue)
        {
            category.Position = updateDto.position.Value;
        }

        _context.WebProductCategories.Update(category);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdatePromotion(int promotionId, UpdatePromotionDTO updatePromotionDto)
    {
        var promotion = await GetPromotionWithDetails(promotionId);
        if (promotion == null)
        {
            return false; // Promotion not found
        }

        // Upload custom image once if provided
        string? customImageUrl = null;
        if (updatePromotionDto.file != null)
        {
            customImageUrl = (await _mediaService.UploadAsync(updatePromotionDto.file)).ToString();
        }

        // Get all related promotions that should be grouped together in admin view
        // For IsComplex: group by Description (old logic)
        // For regular: group by ProductCode + Price + PromoPrice (same discount percentage)
        // This allows updating the same promotion across different stores
        
        // Save original values BEFORE any updates to find all related promotions
        var originalTitle = promotion.Title;
        var originalDescription = promotion.Description;
        
        var nowUtc = DateTime.UtcNow;
        IQueryable<Promotion> baseQuery = _context.Promotions
            .Include(p => p.Product)
            .Include(p => p.PromotionType)
            .Where(p => p.IsComplexPromotion == promotion.IsComplexPromotion) // Match IsComplexPromotion flag
            .Where(p => p.PromotionTypeId == promotion.PromotionTypeId); // Match PromotionTypeId

        // For IsComplex promotions, match by Title, Description, and PromotionTypeId only
        // For complex promotions, we update ALL related promotions regardless of dates and prices
        // This allows updating all related complex promotions across different stores and price points
        // IMPORTANT: For complex promotions, don't filter by ExpiredAt because they may have different dates per store
        if (promotion.IsComplexPromotion)
        {
            baseQuery = baseQuery
                .Where(p => p.Title == originalTitle) // Use original title to find all related promotions
                .Where(p => (p.Description == null && originalDescription == null) || 
                           (p.Description != null && originalDescription != null && p.Description == originalDescription));
            // Note: We don't filter by Price/PromoPrice or ExpiredAt for complex promotions
            // because related promotions may have different prices and dates for different stores
        }
        else
        {
            // For regular promotions: match by ProductCode + same discount (Price/PromoPrice) + same date period
            // Also match by the same date period to ensure we're updating the exact same promotion period
            // Only update promotions that haven't expired yet to avoid modifying historical data
            baseQuery = baseQuery
                .Where(p => p.ExpiredAt >= nowUtc); // Only include promotions that haven't expired yet
            
            var productCode = promotion.Product?.Code ?? promotion.ProductCode;
            baseQuery = baseQuery
                .Where(p => p.StartedAt == promotion.StartedAt && p.ExpiredAt == promotion.ExpiredAt) // Match exact date period
                .Where(p => (p.Product != null && p.Product.Code == productCode) || 
                        (p.ProductCode == productCode && productCode != null))
                .Where(p => p.Price == promotion.Price) // Same original price
                .Where(p => p.PromoPrice == promotion.PromoPrice); // Same discount price (same discount %)
        }

        var relatedPromotions = await baseQuery.ToListAsync();

        // Always ensure the original promotion is included if it's not in the list
        // This handles cases where the promotion might be filtered out but still needs updating
        if (!relatedPromotions.Any(p => p.Id == promotion.Id))
        {
            relatedPromotions.Add(promotion);
        }

        // For complex promotions, ensure all related promotions get the same Description
        // even if description is not explicitly provided in the update DTO
        // This ensures they continue to group together after update
        string? synchronizedDescription = null;
        if (promotion.IsComplexPromotion)
        {
            // If description is provided, use it for all related promotions
            // Otherwise, use the original description from the first promotion to keep them grouped
            synchronizedDescription = updatePromotionDto.description ?? originalDescription;
        }

        // Update all related promotions
        // Note: All entities are already tracked from ToListAsync(), so EF Core will automatically track changes
        // No need to call Update() - it can cause issues with already-tracked entities
        foreach (var relatedPromotion in relatedPromotions)
        {
            await UpdatePromotionType(relatedPromotion, updatePromotionDto.promotionTypeId);
            UpdateBasicPromotionDetails(relatedPromotion, updatePromotionDto, customImageUrl);
            
            // For complex promotions, synchronize Description to ensure all related promotions stay grouped
            if (promotion.IsComplexPromotion)
            {
                relatedPromotion.Description = synchronizedDescription;
            }
            
            await UpdatePromotionProducts(relatedPromotion, updatePromotionDto.productCodes);
            relatedPromotion.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return true;
    }
    private async Task<Promotion?> GetPromotionWithDetails(int promotionId)
    {
        return await _context.Promotions
            .Include(p => p.Product)
            .Include(p => p.PromotionType)
            .FirstOrDefaultAsync(p => p.Id == promotionId);
    }
    private void UpdateBasicPromotionDetails(Promotion promotion, UpdatePromotionDTO updatePromotionDto, string? customImageUrl = null)
    {
        if (updatePromotionDto.isActive.HasValue)
        {
            promotion.InActive = !updatePromotionDto.isActive.Value;
        }

        if (!string.IsNullOrEmpty(updatePromotionDto.name))
        {
            promotion.Title = updatePromotionDto.name;
        }

        if (updatePromotionDto.description != null)
        {
            promotion.Description = updatePromotionDto.description;
        }

        if (updatePromotionDto.offer != null)
        {
            promotion.Offer = updatePromotionDto.offer;
        }

        if (updatePromotionDto.startDate.HasValue)
        {
            promotion.StartedAt = updatePromotionDto.startDate.Value;
        }

        if (updatePromotionDto.endDate.HasValue)
        {
            promotion.ExpiredAt = updatePromotionDto.endDate.Value;
        }

        // Admin can upload a custom image for the promotion
        // If not provided, the promotion will use the product's image as fallback
        if (customImageUrl != null)
        {
            promotion.Image = customImageUrl;
        }
    }

    //Can we add multiple product?
    private async Task UpdatePromotionProducts(Promotion promotion, IEnumerable<string>? productCodes)
    {
        if (productCodes != null && productCodes.Any())
        {
            var existingProducts = await _context.Products
                .Where(p => productCodes.Contains(p.Code))
                .ToListAsync();

            promotion.Product = existingProducts.FirstOrDefault();
        }
    }

    private async Task UpdatePromotionType(Promotion promotion, int? promotionTypeId)
    {
        if (promotionTypeId.HasValue)
        {
            var promotionType = await _context.PromotionsType.FirstOrDefaultAsync(pt => pt.Id == promotionTypeId.Value);
            if (promotionType == null)
            {
                throw new InvalidOperationException("Promotion type not found.");
            }
            promotion.PromotionType = promotionType;
        }
    }

    public async Task<PagedListDTO<PromotionDetailsAdminDTO>> GetAllPromotionsAdmin(
        PagingDTO pagingDto,
        string? searchParam = null,
        DateOnly? startedDate = null,
        string? cities = null,
        string? promotionTagIds = null,
        string? categoryCode = null,
        string? promotionTypeNavisionId = null)
    {
        var storeList = _unitOfWork.Stores.GetAllStores().ToList();

        var baseQuery = BuildPromotionsAdminQuery(
            searchParam,
            startedDate,
            promotionTagIds,
            categoryCode,
            promotionTypeNavisionId,
            cities,
            includeDetails: false);

        // Діагностика: перевірка базового запиту
        var baseCount = await baseQuery.CountAsync();
        
        // Діагностика: перевірка, чи є записи з Product
        var withProductCount = await baseQuery.Where(p => p.Product != null).CountAsync();
        var withProductCodeCount = await baseQuery.Where(p => p.ProductCode != null || (p.Product != null && p.Product.Code != null)).CountAsync();

        // Group promotions for admin list:
        // - For IsComplex: group by Description (old logic - don't group by StoreCode)
        // - For regular: group by ProductCode + discount (new logic - don't group by StoreCode, show addresses as list)
        var groupedKeysQuery = baseQuery
            .GroupBy(p => new
            {
                p.IsComplexPromotion,
                p.Title,
                Description = p.IsComplexPromotion ? p.Description : (string?)null,
                p.Offer,
                p.Price,
                p.PromoPrice,
                p.StartedAt,
                p.ExpiredAt,
                StoreCode = (string?)null, // Don't group by StoreCode for both IsComplex and regular (addresses shown as list)
                p.PromotionTypeId,
                ProductCode = p.IsComplexPromotion ? null : p.ProductCode
            })
            .Select(g => g.Key);

        var totalItems = await groupedKeysQuery.CountAsync();

        var keysPageRaw = await groupedKeysQuery
            .OrderByDescending(k => k.IsComplexPromotion)
            .ThenByDescending(k => k.StartedAt)
            .ThenBy(k => k.Title)
            .Skip(pagingDto.Skip)
            .Take(pagingDto.Take)
            .ToListAsync();

        var keysPage = keysPageRaw
            .Select(k => new PromotionGroupKey(
                k.IsComplexPromotion,
                k.Title,
                k.Description,
                k.Offer,
                k.Price,
                k.PromoPrice,
                k.StartedAt,
                k.ExpiredAt,
                null, // For both IsComplex and regular: don't key by StoreCode (addresses aggregated into list)
                k.PromotionTypeId,
                k.ProductCode))
            .ToList();

        if (keysPage.Count == 0)
        {
            return new PagedListDTO<PromotionDetailsAdminDTO>
            {
                Items = new List<PromotionDetailsAdminDTO>(),
                TotalItems = totalItems,
                Offset = pagingDto.Skip,
                Limit = pagingDto.Take
            };
        }

        var detailedQuery = BuildPromotionsAdminQuery(
            searchParam,
            startedDate,
            promotionTagIds,
            categoryCode,
            promotionTypeNavisionId,
            cities,
            includeDetails: true);

        var predicate = BuildPromotionGroupPredicate(keysPage);
        var promotionsPage = await detailedQuery
            .Where(predicate)
            .ToListAsync();

        var promotionsByKey = promotionsPage
            .GroupBy(p => new PromotionGroupKey(
                p.IsComplexPromotion,
                p.Title,
                p.IsComplexPromotion ? p.Description : null,
                p.Offer,
                p.Price,
                p.PromoPrice,
                p.StartedAt,
                p.ExpiredAt,
                null, // For IsComplex: old logic (group by Description, not StoreCode)
                       // For regular: new logic (group by ProductCode + discount, not StoreCode)
                p.PromotionTypeId,
                p.IsComplexPromotion ? null : (p.ProductCode ?? p.Product?.Code)))
            .ToDictionary(g => g.Key, g => g.ToList());

        var items = new List<PromotionDetailsAdminDTO>();
        foreach (var key in keysPage)
        {
            if (promotionsByKey.TryGetValue(key, out var promotions))
            {
                items.Add(BuildPromotionDetailsAdminDto(promotions, storeList));
            }
        }

        return new PagedListDTO<PromotionDetailsAdminDTO>
        {
            Items = items,
            TotalItems = totalItems,
            Offset = pagingDto.Skip,
            Limit = pagingDto.Take
        };
    }

    private IQueryable<Promotion> BuildPromotionsAdminQuery(
    string? searchParam,
    DateOnly? startedDate,
    string? promotionTagIds,
    string? categoryCode,
    string? promotionTypeNavisionId,
    string? cities,
    bool includeDetails = false)
    {
        var query = _context.Promotions.AsQueryable();

        if (includeDetails)
        {
            query = _context.Promotions
                  .Include(p => p.Product)
                          .ThenInclude(p => p.Brand)
                          .ThenInclude(p => p.Manufacturer)
                         .Include(p => p.Product)
                      .ThenInclude(p => p.Family)
                              .ThenInclude(p => p.Category)
                      .Include(p => p.PromotionType)
                .Include(p => p.WebProductCategory);
        }
        else
        {
            // Завантажуємо Product для групування (використовується в GroupBy для ProductCode)
            // Завантажуємо PromotionType для фільтрації, якщо потрібно
            query = query.Include(p => p.Product);
            if (!string.IsNullOrEmpty(promotionTypeNavisionId))
            {
                query = query.Include(p => p.PromotionType);
            }
        }

        query = query.AsNoTracking();

        query = ApplyCitiesFilter(query, cities);
        query = ApplySearchFilter(query, searchParam);
        query = ApplyDateFilter(query, startedDate);
        query = ApplyPromotionTagFilter(query, promotionTagIds);
        query = ApplyPromotionCategoryFilter(query, categoryCode);
        query = ApplyPromotionTypeFilter(query, promotionTypeNavisionId);

        return query;
    }
    private IQueryable<Promotion> ApplyCitiesFilter(IQueryable<Promotion> query, string? cities)
    {
        if (!string.IsNullOrEmpty(cities))
        {
            var cityIds = cities
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out var parsedId) ? parsedId : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToArray();

            var storeNumbers = _unitOfWork.Stores.GetAllStores()
                .Where(store => cityIds.Contains(store.CityId))
                .Select(store => store.Number)
                .ToList();

            query = query.Where(promotion => storeNumbers.Contains(promotion.StoreCode));
        }

        return query;
    }

    private IQueryable<Promotion> ApplySearchFilter(IQueryable<Promotion> query, string? searchParam)
    {
        if (string.IsNullOrWhiteSpace(searchParam))
            return query;

        var term = searchParam.Trim().ToLower();
        var searchPatternLower = $"%{term}%";

        // Узгоджено з GlobalSearchService: пошук за Title, Product.Title та для комплексних — Offer, Description (регістронезалежно)
        query = query.Where(p =>
            (p.Title != null && EF.Functions.Like(p.Title.ToLower(), searchPatternLower)) ||
            (p.IsComplexPromotion && (
                (p.Description != null && EF.Functions.Like(p.Description.ToLower(), searchPatternLower)) ||
                (p.Offer != null && EF.Functions.Like(p.Offer.ToLower(), searchPatternLower)))) ||
            (p.Product != null && p.Product.Title != null && EF.Functions.Like(p.Product.Title.ToLower(), searchPatternLower)));

        return query;
    }

    private IQueryable<Promotion> ApplyPromotionTypeFilter(IQueryable<Promotion> query, string? promotionTypeNavisionId)
    {
        if (!string.IsNullOrEmpty(promotionTypeNavisionId))
        {
            query = query.Where(x => x.PromotionType != null && x.PromotionType.NavisionId == promotionTypeNavisionId);
        }

        // Note: For admin queries, we don't filter complex promotions by PromoPrice < Price
        // to ensure all complex promotions are visible for management purposes.
        // The client query still filters to avoid duplicates.

        return query;
    }

    private IQueryable<Promotion> ApplyDateFilter(IQueryable<Promotion> query, DateOnly? startedDate)
    {
        if (startedDate.HasValue)
        {
            var dateTime = startedDate.Value.ToDateTime(TimeOnly.MinValue);
            var dateTimeFrom = dateTime.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
            var dateTimeTo = dateTime.AddDays(1).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
            query = query.Where(x => x.StartedAt >= dateTimeFrom && x.StartedAt < dateTimeTo);
        }

        return query;
    }

    private IQueryable<Promotion> ApplyPromotionTagFilter(IQueryable<Promotion> query, string? promotionTagIds)
    {
        if (!string.IsNullOrEmpty(promotionTagIds))
        {
            var parsedPromotionTagIds = ParseIds(promotionTagIds);
            query = query.Where(x => x.Product != null && x.Product.ProductTags.Any(pt => parsedPromotionTagIds.Contains(pt.TagId)));
        }

        return query;
    }

    private IQueryable<Promotion> ApplyPromotionCategoryFilter(IQueryable<Promotion> query, string? categoryCode = null)
    {
        if (!string.IsNullOrEmpty(categoryCode))
        {
            query = query.Where(x => x.WebProductCategory != null && x.WebProductCategory.Code == categoryCode);
        }

        return query;
    }


    public async Task<PagedListDTO<PromotionDetailsClientDTO>> GetAllPromotionsClient(
        byte branchId,
        RegistrationPlatform platform,
        PagingDTO pagingDto,
        string? searchParam = null,
        DateOnly? createdDate = null,
        int? store = null,
        SortingEnum? sortOption = null,
        string? promotionTagIds = null,
        string? categoryCode = null,
        string? promotionTypeNavisionId = null,
        string? token = null)
    {
        if (platform == RegistrationPlatform.Web)
        {
            // reCAPTCHA вмикаємо тільки для початкового завантаження списку,
            // щоб не валити запити при кожному натисканні клавіші/зміні сортування.
            var isInitialLoad =
                pagingDto.Skip == 0 &&
                string.IsNullOrEmpty(searchParam) &&
                sortOption == null &&
                string.IsNullOrEmpty(promotionTagIds) &&
                string.IsNullOrEmpty(categoryCode) &&
                string.IsNullOrEmpty(promotionTypeNavisionId);

            if (isInitialLoad)
            {
                await ValidateRecaptchaTokenAsync(token);
            }
        }

        if (_userContext.BranchId.HasValue && _userContext.BranchId != (Branches)branchId)
        {
            throw new ArgumentException("Мережа в запиті не відповідає мережі користувача");
        }

        var baseQuery = BuildPromotionsQuery(branchId, searchParam, createdDate, promotionTagIds, categoryCode, promotionTypeNavisionId, store);

        // Group promotions by key (same logic as admin method)
        // For IsComplex promotions, group only by Description (not StoreCode)
        var groupedKeysQuery = baseQuery
            .GroupBy(p => new
            {
                p.IsComplexPromotion,
                p.Title,
                Description = p.IsComplexPromotion ? p.Description : (string?)null,
                p.Offer,
                p.Price,
                p.PromoPrice,
                p.StartedAt,
                p.ExpiredAt,
                StoreCode = p.IsComplexPromotion ? (string?)null : p.StoreCode, // For IsComplex, don't group by StoreCode
                p.PromotionTypeId,
                ProductCode = p.IsComplexPromotion
                    ? null
                    : (p.ProductCode ?? (p.Product != null ? p.Product.Code : null)),
                DisplayTitle = p.Product != null ? p.Product.Title : p.Title
            })
            .Select(g => new
            {
                g.Key.IsComplexPromotion,
                g.Key.Title,
                g.Key.Description,
                g.Key.Offer,
                g.Key.Price,
                g.Key.PromoPrice,
                g.Key.StartedAt,
                g.Key.ExpiredAt,
                g.Key.StoreCode,
                g.Key.PromotionTypeId,
                g.Key.ProductCode,
                g.Key.DisplayTitle
            });

        // Apply sorting to grouped keys - IsComplex promotions first by default
        // When sortOption is null (no filter), IsComplex promotions are shown first
        // IMPORTANT: Sorting is applied BEFORE counting totalItems to ensure correct filtering
        var sortedKeysQuery = sortOption switch
        {
            null => groupedKeysQuery
                .OrderByDescending(k => k.IsComplexPromotion)
                .ThenByDescending(k => k.StartedAt),
            SortingEnum.HighestPrice => groupedKeysQuery
                .OrderByDescending(k => k.IsComplexPromotion)
                .ThenByDescending(k => k.PromoPrice),
            SortingEnum.LowestPrice => groupedKeysQuery
                .OrderByDescending(k => k.IsComplexPromotion)
                .ThenBy(k => k.PromoPrice),
            SortingEnum.NewestPromotions => groupedKeysQuery
                .OrderByDescending(k => k.IsComplexPromotion)
                .ThenByDescending(k => k.StartedAt),
            // For alphabetical sorting, use ToLower() on DisplayTitle for proper SQL translation
            // Note: If DisplayTitle has leading spaces, they need to be trimmed in SQL
            // IsComplexPromotion must be first in ORDER BY
            SortingEnum.AlphabeticalAsc => groupedKeysQuery
                .OrderByDescending(k => k.IsComplexPromotion)
                .ThenBy(k => (k.DisplayTitle ?? string.Empty).TrimStart().ToLower()),
            SortingEnum.AlphabeticalDesc => groupedKeysQuery
                .OrderByDescending(k => k.IsComplexPromotion)
                .ThenByDescending(k => (k.DisplayTitle ?? string.Empty).TrimStart().ToLower()),
            SortingEnum.StartDateDesc => groupedKeysQuery
                .OrderByDescending(k => k.IsComplexPromotion)
                .ThenByDescending(k => k.StartedAt),
            SortingEnum.StartDateAsc => groupedKeysQuery
                .OrderByDescending(k => k.IsComplexPromotion)
                .ThenBy(k => k.StartedAt),
            _ => groupedKeysQuery
                .OrderByDescending(k => k.IsComplexPromotion)
                .ThenByDescending(k => k.StartedAt)

        };

        // Count total items AFTER all filters and sorting, but BEFORE pagination
        var totalItems = await sortedKeysQuery.CountAsync();

        // Apply pagination AFTER all filtering and sorting
        var keysPageRaw = await sortedKeysQuery
            .Skip(pagingDto.Skip)
            .Take(pagingDto.Take)
            .ToListAsync();

        var keysPage = keysPageRaw
            .Select(k => new PromotionGroupKey(
                k.IsComplexPromotion,
                k.Title,
                k.Description,
                k.Offer,
                k.Price,
                k.PromoPrice,
                k.StartedAt,
                k.ExpiredAt,
                k.StoreCode,
                k.PromotionTypeId,
                k.ProductCode))
            .ToList();

        if (keysPage.Count == 0)
        {
            return new PagedListDTO<PromotionDetailsClientDTO>
            {
                Items = new List<PromotionDetailsClientDTO>(),
                TotalItems = totalItems,
                Offset = pagingDto.Skip,
                Limit = pagingDto.Take
            };
        }

        // Get detailed promotions for the grouped keys
        var detailedQuery = BuildPromotionsQuery(branchId, searchParam, createdDate, promotionTagIds, categoryCode, promotionTypeNavisionId, store);
        var predicate = BuildPromotionGroupPredicate(keysPage);
        var promotionsPage = await detailedQuery
            .Where(predicate)
            .ToListAsync();

        var promotionsByKey = promotionsPage
            .GroupBy(p => CreatePromotionGroupKey(p, includeStoreCode: !p.IsComplexPromotion))
            .ToDictionary(g => g.Key, g => g.ToList());

        var items = new List<PromotionDetailsClientDTO>();
        foreach (var key in keysPage)
        {
            if (promotionsByKey.TryGetValue(key, out var promotions))
            {
                items.Add(await BuildPromotionDetailsClientDtoAsync(promotions));
            }
        }

        // Деякі складні акції можуть потрапляти в items кілька разів з однаковим Id.
        // Щоб уникнути дублів в API для клієнта, додатково дедуплікуємо по promotion.Id.
        var distinctItems = items
            .GroupBy(i => i.Promotion?.Id)
            .Select(g => g.First())
            .ToList();

        return new PagedListDTO<PromotionDetailsClientDTO>
        {
            Items = distinctItems,
            TotalItems = totalItems,
            Offset = pagingDto.Skip,
            Limit = pagingDto.Take
        };
    }

    public async Task<PromotionDetailsClientDTO?> GetPromotionByIdAsync(
        byte branchId,
        int promotionId)
    {
        // Для детального перегляду застосовуємо ті самі базові фільтри,
        // що й у списку акцій: активна, не прострочена, у межах мережі.
        var baseQuery = BuildPromotionsQuery(
            branchId,
            searchParam: null,
            createdDate: null,
            promotionTagIds: null,
            categoryCode: null,
            promotionTypeNavisionId: null,
            store: null);

        var promotions = await baseQuery
            .Where(p => p.Id == promotionId)
            .ToListAsync();

        if (promotions.Count == 0)
        {
            return null;
        }

        return await BuildPromotionDetailsClientDtoAsync(promotions);
    }

    private async Task ValidateRecaptchaTokenAsync(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("reCAPTCHA token is required for WEB platform.");
        }

        var isValidCaptcha = await VerifyRecaptchaTokenAsync(token);
        if (!isValidCaptcha)
        {
            throw new InvalidOperationException("Invalid reCAPTCHA token.");
        }
    }

    private IQueryable<Promotion> BuildPromotionsQuery(
    byte branchId,
    string? searchParam,
    DateOnly? createdDate,
    string? promotionTagIds,
    string? categoryCode,
    string? promotionTypeNavisionId,
    int? store = null)
    {
        // Use current UTC date to determine if promotion is still relevant for the client (ignore time component)
        var todayUtc = DateTime.UtcNow.Date;

        var query = _context.Promotions
            .Where(x => x.InActive == false) // Filter active promotions
            .Where(x => x.ExpiredAt.Date >= todayUtc) // Keep promotions that are not expired (date-only check)
            .Include(t => t.Product) // Include related Product
                .ThenInclude(t => t.ProductTags) // Include related ProductTags
                .ThenInclude(pt => pt.ProductsTag) // Include related PromotionsTag
            .Include(t => t.Product) // Include Product for Brand
                .ThenInclude(p => p.Brand) // Include Brand
                .ThenInclude(b => b.Manufacturer) // Include Manufacturer
            .Include(t => t.PromotionType) // Include PromotionType
            .Include(t => t.WebProductCategory) // Include WebProductCategory
            .AsQueryable();

        if (branchId != 0)
        {
            var storeNumbers = _unitOfWork.Stores.GetAllStores()
                .Where(st => st.BranchId == branchId)
                .Select(x => x.Number)
                .ToList();

            // Include promotions with null StoreCode (e.g., complex promotions available for all stores in the branch)
            query = query.Where(p =>
                p.StoreCode == null || storeNumbers.Contains(p.StoreCode)
            );
        }


        if (createdDate.HasValue)
        {
            var dateTime = createdDate.Value.ToDateTime(TimeOnly.MinValue);
            var dateTimeFrom = dateTime.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
            var dateTimeTo = dateTime.AddDays(1).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
            query = query.Where(x => x.CreatedAt >= dateTimeFrom && x.CreatedAt < dateTimeTo);
        }

        if (store.HasValue)
        {
            var storeDb = _unitOfWork.Stores.GetAllStores()
               .FirstOrDefault(st => st.Id == store.Value);

            if (storeDb != null)
            {
                // For all promotions (regular and complex) apply store filter:
                // - Include promotions without explicit StoreCode (null) – available in all stores of the branch
                // - Include promotions assigned to the selected store only
                // This makes complex promotions respect the selected store as well.
                query = query.Where(x => x.StoreCode == null || x.StoreCode == storeDb.Number);
            }
        }

        if (!string.IsNullOrWhiteSpace(searchParam))
        {
            var term = searchParam.Trim().ToLower();
            var searchPatternLower = $"%{term}%";
            query = query.Where(p =>
                (p.Title != null && EF.Functions.Like(p.Title.ToLower(), searchPatternLower)) ||
                (p.IsComplexPromotion && (
                    (p.Description != null && EF.Functions.Like(p.Description.ToLower(), searchPatternLower)) ||
                    (p.Offer != null && EF.Functions.Like(p.Offer.ToLower(), searchPatternLower)))) ||
                (p.Product != null && p.Product.Title != null && EF.Functions.Like(p.Product.Title.ToLower(), searchPatternLower)));
        }

        if (!string.IsNullOrEmpty(promotionTagIds))
        {
            var parsedPromotionTagIds = ParseIds(promotionTagIds);
            query = query.Where(x => x.Product != null && x.Product.ProductTags.Any(pt => parsedPromotionTagIds.Contains(pt.TagId)));
        }

        if (!string.IsNullOrEmpty(categoryCode))
        {
            query = query.Where(x => x.WebProductCategory != null && x.WebProductCategory.Code == categoryCode);
        }

        if (!string.IsNullOrEmpty(promotionTypeNavisionId))
        {
            query = query.Where(x => x.PromotionType != null && x.PromotionType.NavisionId == promotionTypeNavisionId);
        }

        // For complex promotions (combo offers), return all positions.
        // Complex promotions are grouped by Description, so duplicates are handled by grouping logic.
        // We don't filter complex promotions by PromoPrice to ensure they are returned to the client.
        // For regular promotions, we still filter to show only those with actual discounts.
        query = query.Where(x => x.IsComplexPromotion || x.PromoPrice < x.Price);

        return query;
    }

    private int[] ParseIds(string ids)
    {
        return ids
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id, out var parsedId) ? parsedId : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .ToArray();
    }

    private async Task<IQueryable<Promotion>> ApplyClientSortingAsync(IQueryable<Promotion> query, SortingEnum? sortOption, byte branchId)
    {
        if (!sortOption.HasValue)
        {
            return query.OrderByDescending(x => x.StartedAt);
        }

        return sortOption switch
        {
            SortingEnum.HighestPrice => query.OrderByDescending(x => x.PromoPrice),
            SortingEnum.LowestPrice => query.OrderBy(x => x.PromoPrice),
            SortingEnum.NewestPromotions => query.OrderByDescending(x => x.CreatedAt),
            // Sort by Product.Title when available; fallback to Promotion.Title
            // Use explicit ordering with ThenBy to ensure proper EF Core translation and correct alphabetical sorting
            // Normalize to lowercase for case-insensitive sorting
            SortingEnum.AlphabeticalAsc => query
                .OrderBy(x => x.Product == null ? 1 : 0) // Put promotions with Product first
                .ThenBy(x => x.Product != null ? x.Product.Title.ToLower() : x.Title.ToLower()),
            SortingEnum.AlphabeticalDesc => query
                .OrderBy(x => x.Product == null ? 1 : 0) // Put promotions with Product first
                .ThenByDescending(x => x.Product != null ? x.Product.Title.ToLower() : x.Title.ToLower()),
            SortingEnum.StartDateDesc => query.OrderByDescending(x => x.StartedAt),
            SortingEnum.StartDateAsc => query.OrderBy(x => x.StartedAt),
            _ => query
        };
    }

    private async Task<IQueryable<Promotion>> GetDefaultClientSortingAsync(IQueryable<Promotion> query, byte branchId)
    {
        var rule = await GetDisplayRuleAsync((Branches) branchId);

        int month;

        if (rule.Period == PromotionRulePeriod.HalfYear)
        {
            month = DateTime.UtcNow.Month >= 6 ? 6 : 1;
        }
        else
        {
            month = (DateTime.UtcNow.Month - 1) / 3 * 3 + 1;
        }

        var startOfPeriod = new DateTime(DateTime.UtcNow.Year, month, 1, 0, 0 , 0, DateTimeKind.Utc);

        var relevantPromotionIds = new List<int>();

        if (rule.RelevantCount > 0 && _userContext.CustomerId.HasValue)
        {
            relevantPromotionIds = await query.Select(x => new
            {
                Promotion = x,
                Count = x.Product.ReceiptProducts
                    .Where(rp => 
                        rp.Receipt.CustomerCard.CustomerId == _userContext.CustomerId!.Value
                        && rp.Receipt.CreatedAt >= startOfPeriod
                        && rp.Receipt.BranchId == branchId)
                    .Select(rp => rp.ReceiptId)
                    .Distinct().Count()
            })
                .Where(x => x.Count != 0)
                .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.Promotion.CreatedAt)
                .Select(x => x.Promotion.Id)
                .Take(rule.RelevantCount)
                .ToListAsync();
        }

        var rulePromotions = new List<int>();

        if (rule.PerRuleCount > 0)
        {
            var queryForRulePromotions = query.Where(x => !relevantPromotionIds.Contains(x.Id));

            rulePromotions = rule.Rule switch
            {
                PromotionDisplayLocationRuleDTO locationRule => await GetPromotionIdsByLocationRule(
                        locationRule,
                        queryForRulePromotions,
                        startOfPeriod,
                        await _unitOfWork.Stores
                            .Find(x => locationRule.StoreIds.Contains(x.Id))
                            .Select(x => x.Number)
                            .ToListAsync()
                    )
                    .Take(rule.PerRuleCount)
                    .ToListAsync(),

                PromotionDisplayAgeRuleDTO ageRule => await queryForRulePromotions.Select(x => new
                {
                    Promotion = x,
                    Count = x.Product.ReceiptProducts
                        .Where(rp => 
                            rp.Receipt.CustomerCard.Customer.Birthday.HasValue
                            && EF.Functions.DateDiffYear(rp.Receipt.CustomerCard.Customer.Birthday, DateTime.UtcNow) >= ageRule.Age.From
                            && EF.Functions.DateDiffYear(rp.Receipt.CustomerCard.Customer.Birthday, DateTime.UtcNow) <= ageRule.Age.To
                            && rp.Receipt.CreatedAt >= startOfPeriod
                            && rp.Receipt.BranchId == branchId
                            )
                        .Select(rp => rp.ReceiptId)
                        .Distinct().Count()
                    })
                    .Where(x => x.Count != 0)
                    .OrderByDescending(x => x.Count)
                    .Select(x => x.Promotion.Id)
                    .Take(rule.PerRuleCount)
                    .ToListAsync(),

                PromotionDisplayTypeOfActivityRuleDTO activityTypeRule => await queryForRulePromotions.Select(x => new
                {
                    Promotion = x,
                    Count = x.Product.ReceiptProducts
                        .Where(rp =>
                            rp.Receipt.CustomerCard.Customer.TypeOfActivity.HasValue
                            && activityTypeRule.TypesOfActivity.Contains(rp.Receipt.CustomerCard.Customer.TypeOfActivity.Value)
                            && rp.Receipt.CreatedAt >= startOfPeriod
                            && rp.Receipt.BranchId == branchId
                            )
                        .Select(rp => rp.ReceiptId)
                        .Distinct().Count()
                })
                    .Where(x => x.Count != 0)
                    .OrderByDescending(x => x.Count)
                    .Select(x => x.Promotion.Id)
                    .Take(rule.PerRuleCount)
                    .ToListAsync(),

                PromotionDisplayAverageCheckRuleDTO averageCheckRule => await queryForRulePromotions.Select(x => new
                {
                    Promotion = x,
                    Count = x.Product.ReceiptProducts
                        .Where(rp => 
                            rp.Receipt.ReceiptProducts.Sum(x => x.Price * x.Quantity - x.Discount) >= averageCheckRule.Amount.From
                            && rp.Receipt.ReceiptProducts.Sum(x => x.Price * x.Quantity - x.Discount) <= averageCheckRule.Amount.To
                            && rp.Receipt.CreatedAt >= startOfPeriod
                            && rp.Receipt.BranchId == branchId
                            )
                        .Select(rp => rp.ReceiptId)
                        .Distinct().Count()
                })
                    .Where(x => x.Count != 0)
                    .OrderByDescending(x => x.Count)
                    .Select(x => x.Promotion.Id)
                    .Take(rule.PerRuleCount)
                    .ToListAsync(),

                _ => rulePromotions,
            };
        }

        return query
            .OrderByDescending(x => relevantPromotionIds.Contains(x.Id))
            .ThenByDescending(x => rulePromotions.Contains(x.Id))
            .ThenBy(x => x.CreatedAt);
    }

    private IQueryable<int> GetPromotionIdsByLocationRule(PromotionDisplayLocationRuleDTO rule, IQueryable<Promotion> query, DateTime startOfPeriod, List<string> storeCodes)
    {
        var promotionIds = query.Select(x => new
        {
            Promotion = x,
            Count = x.Product.ReceiptProducts
                    .Where(rp => storeCodes.Contains(rp.Receipt.StoreCode) && rp.Receipt.CreatedAt >= startOfPeriod)
                    .Select(rp => rp.ReceiptId)
                .Distinct().Count()
        })
        .Where(x => x.Count != 0)
        .OrderByDescending(x => x.Count)
        .Select(x => x.Promotion.Id);

        return promotionIds;
    }

    private async Task<bool> VerifyRecaptchaTokenAsync(string token)
{
        var cacheKey = $"Recaptcha_{token}";

        // Check if the result is already cached
        if (_cache.TryGetValue(cacheKey, out bool cachedResult))
        {
            return cachedResult; // Return the cached result
        }
        var url = "https://www.google.com/recaptcha/api/siteverify";
    var secretKey = Environment.GetEnvironmentVariable("GOOGLE_RECAPTCHA_KEY");

    using var client = new HttpClient();
    var content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("secret", secretKey),
        new KeyValuePair<string, string>("response", token)
    });

    var response = await client.PostAsync(url, content);

    if (!response.IsSuccessStatusCode)
    {
        return false;
    }

    var responseBody = await response.Content.ReadAsStringAsync();
      
        var recaptchaResponse = System.Text.Json.JsonSerializer.Deserialize<RecaptchaResponse>(responseBody);
        if (!recaptchaResponse.success)
        {
            throw new InvalidOperationException(responseBody);
        }
        _cache.Set(cacheKey, recaptchaResponse.success, TimeSpan.FromMinutes(2));
        return recaptchaResponse != null && recaptchaResponse.success;
    }

    private async Task<PromotionDTO> ToClientPromotionDTO(Promotion promotion)
    {
        var customerId = _userContext.CustomerId;

        // Check if the promotion is saved for the current user
        var isSaved = customerId.HasValue && await _context.SavedPromotions
            .AnyAsync(sp => sp.CustomerId == customerId.Value && sp.PromotionId == promotion.Id);

        var isComplex = promotion.IsComplexPromotion;
        // Комбо-акція з одним товаром: IsComplexPromotion == true і є прив'язка до товару
        var isSingleProductCombo = isComplex && promotion.Product != null;

        // Використовуємо лише промо-зображення; для single-product combo не підтягуємо картинку з картки товару
        var promotionImage = !string.IsNullOrEmpty(promotion.Image)
            ? promotion.Image
            : (isSingleProductCombo ? null : promotion.Product?.ImagePath);

        var (startedAtLocal, expiredAtLocal) = NormalizePromotionWindow(promotion.StartedAt, promotion.ExpiredAt);

        // Для комплексних акцій заголовок будуємо з Offer, а не з назви товару;
        // для single-product combo ніколи не підставляємо Product.Title в Title.
        string title;
        if (isComplex)
        {
            title = !string.IsNullOrEmpty(promotion.Offer)
                ? promotion.Offer
                : promotion.Title;
        }
        else
        {
            title = promotion.Product?.Title ?? promotion.Title;
        }

        var promotionsName = isSingleProductCombo
            ? promotion.Title
            : (promotion.Product?.Title ?? promotion.Title);

        return new PromotionDTO
        {
            Id = promotion.Id,
            ItemUnit = promotion.ItemUnit.ToDisplayString(),
            Title = title,
            Description = promotion.Description,
            Image = promotionImage,
            Price = isComplex ? 0 : Math.Round(promotion.Price, 2),
            PromoPrice = isComplex ? 0 : Math.Round(promotion.PromoPrice, 2),
            DiscountPercent = isComplex ? 0 : (promotion.Price > 0 && promotion.PromoPrice >= 0 && promotion.PromoPrice < promotion.Price
                ? Math.Round(((promotion.Price - promotion.PromoPrice) / promotion.Price) * 100, 0)
                : 0),
            StartedAt = startedAtLocal,
            ExpiredAt = expiredAtLocal,
            IsActive = !promotion.InActive,
            CreatedAt = promotion.CreatedAt,
            UpdatedAt = promotion.UpdatedAt,
            PromotionsName = promotionsName,
            IsSaved = isSaved,
            IsComplex = isComplex,
            Offer = promotion.Offer
        };
    }

    private PromotionDTO ToDTO(Promotion promotion)
    {
        var promotionImage = !string.IsNullOrEmpty(promotion.Image) 
            ? promotion.Image 
            : promotion.Product?.ImagePath;

        var (startedAtLocal, expiredAtLocal) = NormalizePromotionWindow(promotion.StartedAt, promotion.ExpiredAt);

        var isComplex = promotion.IsComplexPromotion;
        return new PromotionDTO
        {
            Id = promotion.Id,
            ItemUnit = promotion.ItemUnit.ToDisplayString(),
            //NavisionId = promotion.NavisionId,
            Title = isComplex && !string.IsNullOrEmpty(promotion.Offer) ? promotion.Offer : promotion.Title,
            Description = promotion.Description,
            Price = isComplex ? 0 : Math.Round(promotion.Price, 2),
            PromoPrice = isComplex ? 0 : Math.Round(promotion.PromoPrice, 2),
            StartedAt = startedAtLocal,
            ExpiredAt = expiredAtLocal,
            IsActive = !promotion.InActive,
            Image = promotionImage,
            CreatedAt = promotion.CreatedAt,
            UpdatedAt = promotion.UpdatedAt,
            PromotionsName = promotion.PromotionType?.Title,
            DiscountPercent = isComplex ? 0 : (promotion.Price > 0 && promotion.PromoPrice >= 0 && promotion.PromoPrice < promotion.Price
            ? Math.Round(((promotion.Price - promotion.PromoPrice) / promotion.Price) * 100, 0)
            : 0),
            IsComplex = isComplex,
            Offer = promotion.Offer
            //City = promotion.City
        };
    }

    private (DateTimeOffset StartedAt, DateTimeOffset ExpiredAt) NormalizePromotionWindow(DateTime startedAtUtc, DateTime expiredAtUtc)
    {
        var startedLocalBase = startedAtUtc.FromUtcToTimezoneOffset(TimeZoneConstants.UATimezone);
        var expiredLocalBase = expiredAtUtc.FromUtcToTimezoneOffset(TimeZoneConstants.UATimezone);

        var standardizedStart = new DateTimeOffset(
            startedLocalBase.Year,
            startedLocalBase.Month,
            startedLocalBase.Day,
            6, 0, 0,
            startedLocalBase.Offset);

        var standardizedEnd = new DateTimeOffset(
            expiredLocalBase.Year,
            expiredLocalBase.Month,
            expiredLocalBase.Day,
            21, 0, 0,
            expiredLocalBase.Offset);

        if (standardizedEnd < standardizedStart)
        {
            standardizedEnd = standardizedStart;
        }

        return (standardizedStart, standardizedEnd);
    }
    private PromotionDetailsAdminDTO ToPromotionDetailsAdminDTO(Promotion promotion)
    {
        var promotionItems = GetPromotionItems(promotion).ToList();

        return new PromotionDetailsAdminDTO
        {
            Promotion = ToDTO(promotion),
            PromotionStores = !string.IsNullOrEmpty(promotion.StoreCode)
                ? new List<PromotionStoreDTO> { ToPromotionStoreDTO(promotion.StoreCode, promotion.Id) }
                : new List<PromotionStoreDTO>(),
            PromotionItems = promotionItems,
            PromotionsType = promotion.PromotionType != null ? new PromotionsTypeDTO
            {
                Id = promotion.PromotionType.Id,
                CreatedAt = promotion.PromotionType.CreatedAt,
                UpdatedAt = promotion.PromotionType.UpdatedAt,
                IsActive = promotion.PromotionType.IsActive,
                Title = promotion.PromotionType.Title,
                NavisionId = promotion.PromotionType.NavisionId
            } : null,
            NumberOfProducts = promotionItems.Count
        };
    }

    private PromotionDetailsAdminDTO BuildPromotionDetailsAdminDto(List<Promotion> promotions, List<Store> storeList)
    {
        var promotionWithImage = promotions.FirstOrDefault(p => !string.IsNullOrEmpty(p.Image)) ?? promotions.First();

        var promotionItems = GetPromotionItems(promotionWithImage)
            .Select(item =>
            {
                if (item.Product?.BrandTitle != null)
                {
                    item.Product.BrandTitle = System.Text.RegularExpressions.Regex.Replace(
                        item.Product.BrandTitle.Trim(),
                        "\\s+",
                        " ",
                        System.Text.RegularExpressions.RegexOptions.None,
                        TimeSpan.FromSeconds(1));
                }

                return item;
            })
            .ToList();

        var promotionStores = promotions
            .Select(p => storeList.FirstOrDefault(s => s.Number == p.StoreCode))
            .Where(s => s != null)
            .Select(store => new PromotionStoreDTO
            {
                Id = store.Id,
                Store = store.Number,
                BranchId = store.BranchId,
                Address = store.Address != null ? System.Text.RegularExpressions.Regex.Replace(store.Address.Trim(), "\\s+", " ", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1)) : null,
                Address2 = store.Address2 != null ? System.Text.RegularExpressions.Regex.Replace(store.Address2.Trim(), "\\s+", " ", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1)) : null,
                City = store.City?.Name != null ? System.Text.RegularExpressions.Regex.Replace(store.City.Name.Trim(), "\\s+", " ", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1)) : null,
                Region = store.Region != null ? System.Text.RegularExpressions.Regex.Replace(store.Region.Trim(), "\\s+", " ", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1)) : null
            })
            .GroupBy(s => new { s.Address, s.Store, s.BranchId, s.City, s.Region })
            .Select(x => x.First())
            .ToList();

        var storesListString = string.Join(", ",
            promotionStores
                .Where(s => !string.IsNullOrWhiteSpace(s.Store))
                .Select(s => s.Store));

        return new PromotionDetailsAdminDTO
        {
            Promotion = ToDTO(promotionWithImage),
            PromotionStores = promotionStores,
            StoresList = storesListString,
            PromotionItems = promotionItems,
            PromotionsType = promotionWithImage.PromotionType != null ? new PromotionsTypeDTO
            {
                Id = promotionWithImage.PromotionType.Id,
                CreatedAt = promotionWithImage.PromotionType.CreatedAt,
                UpdatedAt = promotionWithImage.PromotionType.UpdatedAt,
                IsActive = promotionWithImage.PromotionType.IsActive,
                Title = promotionWithImage.PromotionType.Title,
                NavisionId = promotionWithImage.PromotionType.NavisionId
            } : null,
            NumberOfProducts = promotionItems.Count
        };
    }

    private PromotionItemDTO ToPromotionItemDTO(Promotion promotion)
    {
        var product = promotion.Product;

        return new PromotionItemDTO
        {
            PromotionId = promotion.Id,
            BrandCode = product?.BrandCode,
            ManufacturerCode = product?.Brand?.ManufacturerCode,
            ItemCategoryCode = product?.Family?.CategoryCode,
            GroupCode = product?.FamilyCode,
            Quantity = 1,
            CreatedAt = promotion.CreatedAt,
            UpdatedAt = promotion.UpdatedAt,
            Product = product != null ? new ProductDTO
            {
                Code = product.Code,
                Title = product.Title,
                Image = product.ImagePath,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                GroupTitle = product.Family?.Title,
                CategoryTitle = product.Family?.Category?.Title,
                BrandTitle = product.Brand?.Title,
                ManufacturerTitle = product.Brand?.Manufacturer?.Title,
            } : null
        };
    }

    private IEnumerable<PromotionItemDTO> GetPromotionItems(Promotion promotion)
    {
        if (!promotion.IsComplexPromotion)
        {
            return promotion.Product != null
                ? new[] { ToPromotionItemDTO(promotion) }
                : Enumerable.Empty<PromotionItemDTO>();
        }

        var cacheKey = GetComplexPromotionCacheKey(promotion);

        if (_complexPromotionItemsCache.TryGetValue(cacheKey, out var cachedItems))
        {
            return cachedItems;
        }

        var storeCode = promotion.StoreCode;
        var promotionTypeId = promotion.PromotionTypeId;
        var description = promotion.Description;

        var relatedPromotions = _context.Promotions
            .Where(p =>
                p.Title == promotion.Title &&
                ((p.Description == null && description == null) || p.Description == description) &&
                p.Price == promotion.Price &&
                p.PromoPrice == promotion.PromoPrice &&
                p.StartedAt == promotion.StartedAt &&
                p.ExpiredAt == promotion.ExpiredAt &&
                ((p.StoreCode == null && storeCode == null) || p.StoreCode == storeCode) &&
                ((p.PromotionTypeId == null && promotionTypeId == null) || p.PromotionTypeId == promotionTypeId))
            .Include(p => p.Product)
                .ThenInclude(prod => prod.Brand)
                    .ThenInclude(brand => brand.Manufacturer)
            .Include(p => p.Product)
                .ThenInclude(prod => prod.Family)
                    .ThenInclude(family => family.Category)
            .AsNoTracking()
            .ToList();

        cachedItems = relatedPromotions
            .Select(ToPromotionItemDTO)
            .Where(item => item.Product != null)
            .ToList();

        _complexPromotionItemsCache[cacheKey] = cachedItems;

        return cachedItems;
    }

    private static string GetComplexPromotionCacheKey(Promotion promotion)
    {
        var storeCode = promotion.StoreCode ?? string.Empty;
        var promotionTypeId = promotion.PromotionTypeId ?? string.Empty;
        var description = promotion.Description ?? string.Empty;

        return string.Join("|",
            promotion.Title ?? string.Empty,
            description,
            promotion.Price.ToString(CultureInfo.InvariantCulture),
            promotion.PromoPrice.ToString(CultureInfo.InvariantCulture),
            promotion.StartedAt.ToString("O", CultureInfo.InvariantCulture),
            promotion.ExpiredAt.ToString("O", CultureInfo.InvariantCulture),
            storeCode,
            promotionTypeId);
    }

    private sealed record PromotionGroupKey(
        bool IsComplexPromotion,
        string? Title,
        string? Description,
        string? Offer,
        decimal Price,
        decimal PromoPrice,
        DateTime StartedAt,
        DateTime ExpiredAt,
        string? StoreCode,
        string? PromotionTypeId,
        string? ProductCode);

    private static PromotionGroupKey CreatePromotionGroupKey(Promotion promotion)
        => CreatePromotionGroupKey(promotion, includeStoreCode: true);

    private static PromotionGroupKey CreatePromotionGroupKey(Promotion promotion, bool includeStoreCode)
        => new PromotionGroupKey(
            promotion.IsComplexPromotion,
            promotion.Title,
            promotion.IsComplexPromotion ? promotion.Description : null,
            promotion.Offer,
            promotion.Price,
            promotion.PromoPrice,
            promotion.StartedAt,
            promotion.ExpiredAt,
            includeStoreCode ? promotion.StoreCode : null,
            promotion.PromotionTypeId,
            promotion.IsComplexPromotion ? null : (promotion.ProductCode ?? promotion.Product?.Code));

    private static Expression<Func<Promotion, bool>> BuildPromotionGroupPredicate(IEnumerable<PromotionGroupKey> keys)
    {
        var keyList = keys.ToList();

        if (keyList.Count == 0)
        {
            return p => false;
        }

        var parameter = Expression.Parameter(typeof(Promotion), "p");
        Expression? body = null;

        foreach (var key in keyList)
        {
            Expression keyExpression = Expression.AndAlso(
                Expression.Equal(
                    Expression.Property(parameter, nameof(Promotion.IsComplexPromotion)),
                    Expression.Constant(key.IsComplexPromotion)),
                Expression.AndAlso(
                    Expression.Equal(
                        Expression.Property(parameter, nameof(Promotion.Title)),
                        Expression.Constant(key.Title, typeof(string))),
                    Expression.AndAlso(
                        Expression.Equal(
                            Expression.Property(parameter, nameof(Promotion.Price)),
                            Expression.Constant(key.Price)),
                        Expression.AndAlso(
                            Expression.Equal(
                                Expression.Property(parameter, nameof(Promotion.PromoPrice)),
                                Expression.Constant(key.PromoPrice)),
                            Expression.AndAlso(
                                Expression.Equal(
                                    Expression.Property(parameter, nameof(Promotion.StartedAt)),
                                    Expression.Constant(key.StartedAt)),
                                Expression.AndAlso(
                                    Expression.Equal(
                                        Expression.Property(parameter, nameof(Promotion.ExpiredAt)),
                                        Expression.Constant(key.ExpiredAt)),
                                    Expression.Equal(
                                        Expression.Property(parameter, nameof(Promotion.PromotionTypeId)),
                                        Expression.Constant(key.PromotionTypeId, typeof(string)))
                                ))))));

            // For complex promotions, include Description in the match
            if (key.IsComplexPromotion)
            {
                var descriptionProperty = Expression.Property(parameter, nameof(Promotion.Description));
                Expression descriptionMatch;

                if (key.Description != null)
                {
                    descriptionMatch = Expression.Equal(
                        descriptionProperty,
                        Expression.Constant(key.Description, typeof(string)));
                }
                else
                {
                    // Match null or empty string
                    descriptionMatch = Expression.OrElse(
                        Expression.Equal(descriptionProperty, Expression.Constant(null, typeof(string))),
                        Expression.Equal(descriptionProperty, Expression.Constant(string.Empty, typeof(string))));
                }

                keyExpression = Expression.AndAlso(keyExpression, descriptionMatch);
            }

            // Include Offer in the match (null-safe, applies to all promotion types)
            {
                var offerProperty = Expression.Property(parameter, nameof(Promotion.Offer));
                Expression offerMatch;

                if (key.Offer != null)
                {
                    offerMatch = Expression.Equal(
                        offerProperty,
                        Expression.Constant(key.Offer, typeof(string)));
                }
                else
                {
                    offerMatch = Expression.OrElse(
                        Expression.Equal(offerProperty, Expression.Constant(null, typeof(string))),
                        Expression.Equal(offerProperty, Expression.Constant(string.Empty, typeof(string))));
                }

                keyExpression = Expression.AndAlso(keyExpression, offerMatch);
            }

            // Handle StoreCode matching
            // For IsComplex promotions, don't filter by StoreCode (group all with same Description)
            // For regular promotions, filter by StoreCode if provided
            if (!key.IsComplexPromotion && key.StoreCode != null)
            {
                keyExpression = Expression.AndAlso(
                    keyExpression,
                    Expression.Equal(
                        Expression.Property(parameter, nameof(Promotion.StoreCode)),
                        Expression.Constant(key.StoreCode, typeof(string))));
            }
            // For IsComplex promotions or non-complex promotions with null StoreCode, don't filter by StoreCode

            if (key.ProductCode == null)
            {
                keyExpression = Expression.AndAlso(
                    keyExpression,
                    Expression.Equal(
                        Expression.Property(parameter, nameof(Promotion.IsComplexPromotion)),
                        Expression.Constant(true)));
            }
            else
            {
                var productCodeProperty = Expression.Property(parameter, nameof(Promotion.ProductCode));
                var productNavigation = Expression.Property(parameter, nameof(Promotion.Product));
                var productNotNull = Expression.NotEqual(
                    productNavigation,
                    Expression.Constant(null, typeof(Product)));
                var productNavigationCode = Expression.Property(productNavigation, nameof(Product.Code));

                var productCodeMatches = Expression.Equal(
                    productCodeProperty,
                    Expression.Constant(key.ProductCode, typeof(string)));

                var productNavigationMatches = Expression.AndAlso(
                    productNotNull,
                    Expression.Equal(
                        productNavigationCode,
                        Expression.Constant(key.ProductCode, typeof(string))));

                keyExpression = Expression.AndAlso(
                    keyExpression,
                    Expression.OrElse(productCodeMatches, productNavigationMatches));
            }

            body = body == null ? keyExpression : Expression.OrElse(body, keyExpression);
        }

        return Expression.Lambda<Func<Promotion, bool>>(body!, parameter);
    }

    private async Task<PromotionDetailsClientDTO> BuildPromotionDetailsClientDtoAsync(List<Promotion> promotions)
    {
        var promotionWithImage = promotions.FirstOrDefault(p => !string.IsNullOrEmpty(p.Image)) ?? promotions.First();
        var promotionItems = GetPromotionItems(promotionWithImage).ToList();

        var isComplex = promotionWithImage.IsComplexPromotion;
        var distinctProductCount = promotionItems
            .Where(item => item.Product != null)
            .Select(item => item.Product.Code)
            .Distinct()
            .Count();
        // Комбо-акція з одним товаром: одна унікальна позиція в складі акції
        var isSingleProductCombo = isComplex && distinctProductCount == 1;

        if (isSingleProductCombo)
        {
            foreach (var item in promotionItems)
            {
                if (item.Product != null)
                {
                    item.Product.Title = null;
                }
            }
        }

        string promotionTitle;
        if (isComplex)
        {
            if (isSingleProductCombo)
            {
                promotionTitle = promotionWithImage.Title;
            }
            else
            {
                var productTitles = promotionItems
                    .Where(item => item.Product != null && !string.IsNullOrEmpty(item.Product.Title))
                    .Select(item => item.Product.Title)
                    .Distinct()
                    .OrderBy(title => title)
                    .ToList();

                promotionTitle = productTitles.Any()
                    ? string.Join(", ", productTitles)
                    : promotionWithImage.Title;
            }
        }
        else
        {
            promotionTitle = promotionWithImage.Product?.Title ?? promotionWithImage.Title;
        }

        var promotionStores = promotions
            .Select(p => !string.IsNullOrEmpty(p.StoreCode) ? ToPromotionStoreDTO(p.StoreCode, p.Id) : null)
            .Where(s => s != null)
            .GroupBy(s => new { s.Store, s.BranchId, s.Address, s.City })
            .Select(g => g.First())
            .ToList();

        // Get tags from all products in the promotion
        var allTags = promotions
            .Where(p => p.Product != null)
            .SelectMany(p => p.Product.ProductTags ?? Enumerable.Empty<ProductTag>())
            .Select(tag => new PromotionsTagDTO
            {
                Id = tag.Id,
                Title = tag.ProductsTag?.Title,
                Position = tag.ProductsTag?.Position ?? 0,
                CreatedAt = tag.ProductsTag?.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = tag.ProductsTag?.UpdatedAt ?? DateTime.MinValue,
                IsActive = tag.ProductsTag?.IsActive ?? false,
                Icon = tag.ProductsTag?.Icon
            })
            .DistinctBy(t => t.Id)
            .OrderBy(t => t.Position)
            .ThenBy(t => t.Title)
            .ToList();

        var customerId = _userContext.CustomerId;
        var promotionIds = promotions.Select(p => p.Id).ToList();
        var isSaved = customerId.HasValue &&
                      promotionIds.Count != 0 &&
                      await _context.SavedPromotions
                          .AnyAsync(sp =>
                              sp.CustomerId == customerId.Value &&
                              promotionIds.Contains(sp.PromotionId));

        // Для single-product combo не підтягуємо зображення з картки товару
        var promotionImage = !string.IsNullOrEmpty(promotionWithImage.Image)
            ? promotionWithImage.Image
            : (isSingleProductCombo ? null : promotionWithImage.Product?.ImagePath);

        var (startedAtLocal, expiredAtLocal) = NormalizePromotionWindow(promotionWithImage.StartedAt, promotionWithImage.ExpiredAt);

        var displayTitle = isComplex && !string.IsNullOrEmpty(promotionWithImage.Offer) 
            ? promotionWithImage.Offer 
            : promotionTitle;
        
        return new PromotionDetailsClientDTO
        {
            Promotion = new PromotionDTO
            {
                Id = promotionWithImage.Id,
                ItemUnit = promotionWithImage.ItemUnit.ToDisplayString(),
                Title = displayTitle,
                Description = promotionWithImage.Description,
                Image = promotionImage,
                Price = isComplex ? 0 : Math.Round(promotionWithImage.Price, 2),
                PromoPrice = isComplex ? 0 : Math.Round(promotionWithImage.PromoPrice, 2),
                DiscountPercent = isComplex ? 0 : (promotionWithImage.Price > 0 && promotionWithImage.PromoPrice >= 0 && promotionWithImage.PromoPrice < promotionWithImage.Price
                    ? Math.Round(((promotionWithImage.Price - promotionWithImage.PromoPrice) / promotionWithImage.Price) * 100, 0)
                    : 0),
                StartedAt = startedAtLocal,
                ExpiredAt = expiredAtLocal,
                IsActive = !promotionWithImage.InActive,
                CreatedAt = promotionWithImage.CreatedAt,
                UpdatedAt = promotionWithImage.UpdatedAt,
                PromotionsName = isSingleProductCombo ? promotionWithImage.Title : promotionTitle,
                IsSaved = isSaved,
                IsComplex = isComplex,
                Offer = promotionWithImage.Offer
            },
            PromotionStores = promotionStores,
            PromotionsTag = allTags,
            PromotionsType = promotionWithImage.PromotionType != null ? new PromotionsTypeDTO
            {
                Id = promotionWithImage.PromotionType.Id,
                CreatedAt = promotionWithImage.PromotionType.CreatedAt,
                UpdatedAt = promotionWithImage.PromotionType.UpdatedAt,
                IsActive = promotionWithImage.PromotionType.IsActive,
                Title = promotionWithImage.PromotionType.Title,
                NavisionId = promotionWithImage.PromotionType.NavisionId
            } : null,
            PromotionItems = promotionItems
        };
    }

    private PromotionDetailsClientDTO ToPromotionDetailsClientDTOAsync(Promotion promotion)
    {
        return new PromotionDetailsClientDTO
        {
            Promotion = ToClientPromotionDTO(promotion).Result, // Use the async method to include the isSaved check
            PromotionStores = !string.IsNullOrEmpty(promotion.StoreCode)
                ? new List<PromotionStoreDTO> { ToPromotionStoreDTO(promotion.StoreCode, promotion.Id) }
                : new List<PromotionStoreDTO>(),
            PromotionsTag = promotion.Product?.ProductTags?.Select(tag => new PromotionsTagDTO
            {
                Id = tag.Id,
                Title = tag.ProductsTag?.Title,
                Position = tag.ProductsTag?.Position ?? 0,
                CreatedAt = tag.ProductsTag?.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = tag.ProductsTag?.UpdatedAt ?? DateTime.MinValue,
                IsActive = tag.ProductsTag?.IsActive ?? false,
                Icon = tag.ProductsTag?.Icon
            }).ToList() ?? new List<PromotionsTagDTO>(),
            PromotionsType = promotion.PromotionType != null ? new PromotionsTypeDTO
            {
                Id = promotion.PromotionType.Id,
                CreatedAt = promotion.PromotionType.CreatedAt,
                UpdatedAt = promotion.PromotionType.UpdatedAt,
                IsActive = promotion.PromotionType.IsActive,
                Title = promotion.PromotionType.Title,
                NavisionId = promotion.PromotionType.NavisionId
            } : null,
            PromotionItems = GetPromotionItems(promotion).ToList()
        };
    }

    // Helper method to create PromotionStoreDTO from StoreCode and PromotionId
    private PromotionStoreDTO ToPromotionStoreDTO(string storeCode, int promotionId)
    {
        var store = _unitOfWork.Stores.GetAllStores()
            .FirstOrDefault(x => x.Number == storeCode);

        if (store == null)
        {
            throw new InvalidOperationException($"Store with code {storeCode} not found.");
        }

        return new PromotionStoreDTO
        {
            Id = store.Id,
            Store = storeCode,
            PromotionId = promotionId,
            BranchId = store.BranchId,
            Address = store.Address,
            Address2 = store.Address2,
            City = store.City?.Name,
            Region = store.Region
        };
    }

    public async Task<List<PromotionsTypeDTO>> GetAllPromotionTypes(bool all = false)
    {
        var promotionTypes = await _context.PromotionsType
            .Where(p => all || p.IsActive)
            .Select(promotion => new PromotionsTypeDTO
            {
                Id = promotion.Id,
                NavisionId = promotion.NavisionId,
                Title = promotion.Title,
                IsActive = promotion.IsActive,
                Position = promotion.Position,
                CreatedAt = promotion.CreatedAt,
                UpdatedAt = promotion.UpdatedAt,
            })
            .ToListAsync();

        return promotionTypes;
    }

    public async Task<List<PromotionsCategoryDTO>> GetAllPromotionCategories(bool all = false)
    {
        var categories = await _context.WebProductCategories
            .Select(x => new PromotionsCategoryDTO
            {
                Code = x.Code,
                Title = x.Title,
                Icon = x.Image,
                IsActive = x.IsActive,
                Position = x.Position
            })
            .ToListAsync();

        if (!all)
        {
            categories = categories.Where(c => c.IsActive).ToList();
        }

        return categories
            .OrderBy(c => c.Position)
            .ThenBy(c => c.Title)
            .ToList();
    }
    public async Task<List<PromotionsTagDTO>> GetAllPromotionTags(bool all = false)
    {
        var promotionTags = await _context.ProductsTags
            .Where(p => all || p.IsActive)
            .Select(tag => new PromotionsTagDTO
            {
                Id = tag.Id,
                Title = tag.Title,
                NavisionId = tag.Code,
                Position = tag.Position.GetValueOrDefault(),
                CreatedAt = tag.CreatedAt.GetValueOrDefault(),
                UpdatedAt = tag.UpdatedAt.GetValueOrDefault(),
                IsActive = tag.IsActive,
                Icon = tag.Icon,
            })
            .ToListAsync();

        return promotionTags;
    }
        

    private IQueryable<PromotionDisplayRule> GetPromotionDisplayRuleQuery()
        => _context.PromotionDisplayRules
            .Include(x => x.TypesOfActivity)
            .Include(x => x.Stores);

    public async Task<PromotionDisplayRuleDTO> GetDisplayRuleAsync(Branches branchId)
    {
        var rule = await GetPromotionDisplayRuleQuery().FirstAsync(x => x.BranchId == (byte) branchId);
        var dto = await ToPromotionDisplayRuleDTOAsync(rule);
        return dto;
    }

    public async Task<List<PromotionDisplayRuleDTO>> GetDisplayRulesAsync()
    {
        var rules = await GetPromotionDisplayRuleQuery().ToListAsync();

        var dtos = new List<PromotionDisplayRuleDTO>();
        foreach (var rule in rules)
        {
            var dto = await ToPromotionDisplayRuleDTOAsync(rule);
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<PromotionDisplayRuleDTO> ToPromotionDisplayRuleDTOAsync(PromotionDisplayRule entity)
    {
        var storeCodes = entity.Stores.Select(x => x.StoreCode).ToList();
        var stores = await _unitOfWork.Stores.GetAll()
            .Where(x => storeCodes.Contains(x.Number))
            .ToListAsync();

        var storeIds = stores.Select(x => x.Id).ToList();

        return new PromotionDisplayRuleDTO
        {
            BranchId = (Branches)entity.BranchId,
            PerRuleCount = entity.PerRuleCount,
            RelevantCount = entity.RelevantCount,
            Period = entity.Period,
            Rule = entity.Type switch
            {
                PromotionDisplayRuleType.Location => new PromotionDisplayLocationRuleDTO
                {
                    StoreIds = storeIds,
                },
                PromotionDisplayRuleType.TypeOfActivity => new PromotionDisplayTypeOfActivityRuleDTO
                {
                    TypesOfActivity = entity.TypesOfActivity.Select(x => x.TypeOfActivity).ToHashSet(),
                },
                PromotionDisplayRuleType.Age => new PromotionDisplayAgeRuleDTO
                {
                    Age = new RangeDTO<int>
                    {
                        From = entity.AgeFrom!.Value,
                        To = entity.AgeTo!.Value,
                    }
                },
                PromotionDisplayRuleType.AverageCheck => new PromotionDisplayAverageCheckRuleDTO
                {
                    Amount = new RangeDTO<decimal>
                    {
                        From = entity.CheckAmountFrom!.Value,
                        To = entity.CheckAmountTo!.Value,
                    }
                },
                _ => throw new ArgumentOutOfRangeException(nameof(entity.Type), $"Unsupported rule type: {entity.Type}")
            },
        };
    }

    public async Task UpdateDisplayRuleAsync(PromotionDisplayRuleDTO model)
    {
        var entity = await GetPromotionDisplayRuleQuery().FirstAsync(x => x.BranchId == (byte)model.BranchId);

        entity.Stores.Clear();
        entity.TypesOfActivity.Clear();

        entity.CheckAmountFrom = null;
        entity.CheckAmountTo = null;

        entity.AgeFrom = null;
        entity.AgeTo = null;

        entity.PerRuleCount = model.PerRuleCount;
        entity.RelevantCount = model.RelevantCount;
        entity.Period = model.Period;

        if (model.Rule is PromotionDisplayLocationRuleDTO locationRule)
        {
            entity.Type = PromotionDisplayRuleType.Location;
            var storeNumbers = new List<string>();

            foreach (var storeId in locationRule.StoreIds)
            {
                var storeNumber = await _unitOfWork.Stores.Find(x => x.Id == storeId && x.BranchId == (byte)model.BranchId).Select(x => x.Number).FirstOrDefaultAsync();
                if (storeNumber == null)
                {
                    throw new ArgumentException($"Магазин з Id {storeId} не існує в мережі ${model.BranchId}");
                }
                storeNumbers.Add(storeNumber);
            }

            entity.Stores = storeNumbers.Select(x => new PromotionDisplayRuleToStore
            {
                StoreCode = x,
            }).ToList();
        }

        if (model.Rule is PromotionDisplayTypeOfActivityRuleDTO typeOfActivitiesRule)
        {
            entity.Type = PromotionDisplayRuleType.TypeOfActivity;
            entity.TypesOfActivity = typeOfActivitiesRule.TypesOfActivity.Select(x => new PromotionDisplayRuleToActivityType
            {
                TypeOfActivity = x,
            }).ToList();
        }

        if (model.Rule is PromotionDisplayAgeRuleDTO ageRule)
        {
            entity.Type = PromotionDisplayRuleType.Age;
            entity.AgeFrom = ageRule.Age.From;
            entity.AgeTo = ageRule.Age.To;
        }

        if (model.Rule is PromotionDisplayAverageCheckRuleDTO averageCheckRule)
        {
            entity.Type = PromotionDisplayRuleType.AverageCheck;
            entity.CheckAmountFrom = averageCheckRule.Amount.From;
            entity.CheckAmountTo = averageCheckRule.Amount.To;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<SavedPromotionNotificationResultDTO> TriggerSavedPromotionExpirationNotificationManuallyAsync(int? promotionId = null)
    {
        var result = new SavedPromotionNotificationResultDTO
        {
            ExecutionStartedAt = DateTime.UtcNow
        };

        try
        {
            // Get all saved promotions for promotions that expire today (in local timezone)
            var nowLocal = DateTime.UtcNow.FromUtcToTimezone(TimeZoneConstants.UATimezone);
            var todayLocal = nowLocal.Date;
            var tomorrowLocal = todayLocal.AddDays(1);

            // Convert back to UTC for database comparison
            var todayUtc = todayLocal.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
            var tomorrowUtc = tomorrowLocal.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);

            result.DateRangeUsed = $"Today UTC: {todayUtc:yyyy-MM-dd HH:mm:ss}, Tomorrow UTC: {tomorrowUtc:yyyy-MM-dd HH:mm:ss}";

            // Query saved promotions that expire today or tomorrow
            var savedPromotionsQuery = _context.SavedPromotions
                .Include(x => x.Customer)
                    .ThenInclude(x => x.CustomerSettings)
                .Include(x => x.Promotion)
                    .ThenInclude(x => x.Product)
                .Where(x => x.Promotion.ExpiredAt >= todayUtc
                    && x.Promotion.ExpiredAt < tomorrowUtc
                    && x.Customer != null && !x.Customer.IsDeleted && !x.Customer.IsBlocked);

            // If specific promotionId provided, filter by it
            if (promotionId.HasValue)
            {
                savedPromotionsQuery = savedPromotionsQuery.Where(x => x.PromotionId == promotionId.Value);
            }

            var savedPromotions = await savedPromotionsQuery.ToListAsync();

            result.TotalSavedPromotionsFound = savedPromotions.Count;
            result.TotalCustomersProcessed = savedPromotions.Select(x => x.CustomerId).Distinct().Count();

            // Group by PromotionId to process each promotion separately
            var promotionsGrouped = savedPromotions.GroupBy(x => x.PromotionId).ToList();

            result.PromotionsProcessed = promotionsGrouped.Count;

            foreach (var promotionGroup in promotionsGrouped)
            {
                var promotionResult = new PromotionNotificationResultDTO
                {
                    PromotionId = promotionGroup.Key
                };

                try
                {
                    var firstSavedPromotion = promotionGroup.First();
                    var promotion = firstSavedPromotion.Promotion;

                    promotionResult.PromotionTitle = promotion.IsComplexPromotion && !string.IsNullOrEmpty(promotion.Offer)
                        ? promotion.Offer
                        : promotion.Product?.Title ?? promotion.Title;
                    promotionResult.PromotionExpiredAt = promotion.ExpiredAt;
                    promotionResult.SavedPromotionsFound = promotionGroup.Count();
                    promotionResult.CustomersProcessed = promotionGroup.Select(x => x.CustomerId).Distinct().Count();

                    foreach (var savedPromotion in promotionGroup)
            {
                var expirationLocal = savedPromotion.Promotion.ExpiredAt.FromUtcToTimezone(TimeZoneConstants.UATimezone);
                var expiresToday = expirationLocal.Date == todayLocal;

                var settings = savedPromotion.Customer.CustomerSettings.ToList();

                foreach (var setting in settings)
                {
                    var customerDetail = new CustomerNotificationDetailDTO
                    {
                        SavedPromotionId = savedPromotion.Id,
                        CustomerId = savedPromotion.CustomerId,
                        CustomerEmail = savedPromotion.Customer?.Email,
                        CustomerPhoneNumber = savedPromotion.Customer?.PhoneNumber,
                        BranchId = setting.BranchId,
                        ExpiresToday = expiresToday,
                        PushNotificationsEnabled = setting.EnablePushNotifications,
                        HasPushToken = !string.IsNullOrEmpty(setting.PushNotificationToken),
                        EmailNotificationsEnabled = setting.EnableEmailNotifications,
                        SmsNotificationsEnabled = setting.EnableSmsNotifications
                    };

                    // Process push notifications
                    try
                    {
                        var promotionDisplayTitle = GetPromotionDisplayTitle(savedPromotion.Promotion);
                        var title = expiresToday
                            ? "Збережена акція закінчується сьогодні"
                            : "Збережена акція закінчується завтра";
                        var body = expiresToday
                            ? $"Ваша збережена акція {promotionDisplayTitle} закінчується сьогодні. Не забудьте скористатися нею!"
                            : $"Ваша збережена акція {promotionDisplayTitle} закінчується завтра. Не забудьте скористатися нею!";

                        await _firebaseService.SendMessageAsync(title, body, "savedPromotions", setting.BranchId, setting.CustomerId);

                        if (setting.EnablePushNotifications && !string.IsNullOrEmpty(setting.PushNotificationToken))
                        {
                            customerDetail.PushNotificationStatus = NotificationStatus.Sent;
                            promotionResult.PushNotificationsSent++;
                        }
                        else
                        {
                            customerDetail.PushNotificationStatus = NotificationStatus.AddedToHistory;
                            promotionResult.PushNotificationsAddedToHistory++;
                        }

                        // Check if notification was added to history (always should be)
                        var latestNotification = await _context.NotificationHistories
                            .Where(x => x.CustomerId == setting.CustomerId
                                && x.BranchId == setting.BranchId
                                && x.Type == "savedPromotions")
                            .OrderByDescending(x => x.CreatedAt)
                            .FirstOrDefaultAsync();

                        if (latestNotification != null && latestNotification.CreatedAt >= result.ExecutionStartedAt)
                        {
                            customerDetail.PushNotificationMessageId = latestNotification.MessageId;
                        }
                    }
                    catch (Exception ex)
                    {
                        customerDetail.PushNotificationStatus = NotificationStatus.Failed;
                        customerDetail.ErrorMessage = ex.Message;
                        promotionResult.Errors.Add($"Push notification failed for CustomerId {setting.CustomerId}, BranchId {setting.BranchId}: {ex.Message}");
                    }

                    // Process email notifications
                    try
                    {
                        if (setting.EnableEmailNotifications && savedPromotion.Customer != null && !string.IsNullOrEmpty(savedPromotion.Customer.Email))
                        {
                            var emailBody = await _htmlGenerationService.GenerateSavedPromotionExpiresTomorrowEmail(
                                savedPromotion.Customer.FirstName ?? string.Empty,
                                savedPromotion.Promotion.Title,
                                setting.BranchId);

                            EmailFrom emailFrom;
                            if (!Enum.IsDefined(typeof(EmailFrom), (int)setting.BranchId))
                            {
                                throw new ArgumentException($"Invalid BranchId for EmailFrom: {setting.BranchId}");
                            }
                            emailFrom = (EmailFrom)(int)setting.BranchId;

                            var subject = expiresToday
                                ? $"Збережена акція {savedPromotion.Promotion.Title} закінчується сьогодні"
                                : $"Збережена акція {savedPromotion.Promotion.Title} закінчується завтра";

                            await _notificationService.SendEmailAsync(new EmailMessage
                            {
                                From = emailFrom,
                                Subject = subject,
                                Body = emailBody,
                                To = new List<string> { savedPromotion.Customer.Email }
                            });

                            customerDetail.EmailNotificationStatus = NotificationStatus.Sent;
                            promotionResult.EmailNotificationsSent++;
                        }
                        else
                        {
                            customerDetail.EmailNotificationStatus = NotificationStatus.Skipped;
                        }
                    }
                    catch (Exception ex)
                    {
                        customerDetail.EmailNotificationStatus = NotificationStatus.Failed;
                        customerDetail.ErrorMessage = customerDetail.ErrorMessage != null
                            ? $"{customerDetail.ErrorMessage}; Email error: {ex.Message}"
                            : $"Email error: {ex.Message}";
                        promotionResult.Errors.Add($"Email notification failed for CustomerId {setting.CustomerId}, BranchId {setting.BranchId}: {ex.Message}");
                    }

                    // Process SMS notifications (only if push notifications are disabled)
                    if (!setting.EnablePushNotifications)
                    {
                        try
                        {
                            if (setting.EnableSmsNotifications && savedPromotion.Customer != null && !string.IsNullOrEmpty(savedPromotion.Customer.PhoneNumber))
                            {
                                var message = expiresToday
                                    ? $"Ваша збережена акція {savedPromotion.Promotion.Title} закінчується сьогодні. Не забудьте скористатися нею!"
                                    : $"Ваша збережена акція {savedPromotion.Promotion.Title} закінчується завтра. Не забудьте скористатися нею!";

                                var smsMessage = new SmsMessage
                                {
                                    Recievers = new List<string> { savedPromotion.Customer.PhoneNumber },
                                    Message = message
                                };

                                switch (setting.BranchId)
                                {
                                    case 1:
                                        smsMessage.Sender = _smsSettings.From.BirdJet.Id;
                                        break;
                                    case 2:
                                        smsMessage.Sender = _smsSettings.From.CatJet.Id;
                                        break;
                                    default:
                                        throw new ArgumentException("Invalid branch ID");
                                }

                                await _notificationService.SendSmsAsync(smsMessage);

                                customerDetail.SmsNotificationStatus = NotificationStatus.Sent;
                                promotionResult.SmsNotificationsSent++;
                            }
                            else
                            {
                                customerDetail.SmsNotificationStatus = NotificationStatus.Skipped;
                            }
                        }
                        catch (Exception ex)
                        {
                            customerDetail.SmsNotificationStatus = NotificationStatus.Failed;
                            customerDetail.ErrorMessage = customerDetail.ErrorMessage != null
                                ? $"{customerDetail.ErrorMessage}; SMS error: {ex.Message}"
                                : $"SMS error: {ex.Message}";
                            promotionResult.Errors.Add($"SMS notification failed for CustomerId {setting.CustomerId}, BranchId {setting.BranchId}: {ex.Message}");
                        }
                    }
                    else
                    {
                        customerDetail.SmsNotificationStatus = NotificationStatus.Skipped;
                    }

                        promotionResult.CustomerDetails.Add(customerDetail);
                    }
                    } // End foreach (var setting in settings)

                    // Aggregate statistics for total result
                    result.TotalPushNotificationsSent += promotionResult.PushNotificationsSent;
                    result.TotalPushNotificationsAddedToHistory += promotionResult.PushNotificationsAddedToHistory;
                    result.TotalEmailNotificationsSent += promotionResult.EmailNotificationsSent;
                    result.TotalSmsNotificationsSent += promotionResult.SmsNotificationsSent;
                } // End foreach (var savedPromotion in promotionGroup)
                catch (Exception ex)
                {
                    promotionResult.Errors.Add($"Error processing promotion {promotionGroup.Key}: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        promotionResult.Errors.Add($"Inner exception: {ex.InnerException.Message}");
                    }
                    result.Errors.Add($"Error processing promotion {promotionGroup.Key}: {ex.Message}");
                }

                result.PromotionResults.Add(promotionResult);
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Unexpected error: {ex.Message}");
            if (ex.InnerException != null)
            {
                result.Errors.Add($"Inner exception: {ex.InnerException.Message}");
            }
        }
        finally
        {
            result.ExecutionCompletedAt = DateTime.UtcNow;
        }

        return result;
    }

    private static string GetPromotionDisplayTitle(Promotion promotion)
    {
        // For complex promotions: use Offer if present, otherwise Product.Title or Promotion.Title
        // For regular promotions: use Product.Title or Promotion.Title
        if (promotion.IsComplexPromotion && !string.IsNullOrEmpty(promotion.Offer))
        {
            return promotion.Offer;
        }
        return promotion.Product?.Title ?? promotion.Title;
    }
}
