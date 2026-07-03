using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared.Models.Promotion;
using JetFlight.Shared.Models.GlobalSearch;
using JetFlight.Shared.Models.PageManagement;
using JetFlight.Shared.Models.Posts;
using JetFlight.ApplicationDataAccess.DbFunctions;
using JetFlight.ApplicationDataAccess;
using JetFlight.IntegrationDataAccess;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.Service.Services
{
    public interface IGlobalSearchService
    {
        Task<GlobalSearchResult> GetGlobalSearchResult(string search, byte branchId, int? storeId, CancellationToken cancellationToken = default);
    }

    public class GlobalSearchService : IGlobalSearchService
    {
        private readonly IntegrationDataContext _dataContext;
        private readonly IDataUnitOfWork _unitOfWork;
        private readonly ApplicationDataContext _applicationDataContext;

        public GlobalSearchService(IntegrationDataContext dataContext, IDataUnitOfWork unitOfWork, ApplicationDataContext applicationDataContext)
        {
            _dataContext = dataContext;
            _unitOfWork = unitOfWork;
            _applicationDataContext = applicationDataContext;
        }

        public async Task<GlobalSearchResult> GetGlobalSearchResult(string search, byte branchId, int? storeId, CancellationToken cancellationToken = default)
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            var globalSearchResult = new GlobalSearchResult
            {
                PromotionSearchResult = new List<PromotionSearchDTO>(),
                PageSearchResult = new List<PageSearchDTO>(),
                PostSearchResult = new List<PostSearchDTO>()
            };

            // Early exit if search is too short or empty
            if (string.IsNullOrWhiteSpace(search) || search.Trim().Length < 2)
            {
                return globalSearchResult;
            }

            var trimmedSearch = search.Trim();
            var searchPattern = $"%{trimmedSearch}%";
            var searchPatternLower = $"%{trimmedSearch.ToLower()}%";

            // Base promotions query: only active, non-expired promotions
            // Вирівняно з клієнтською сторінкою акцій: фільтр по даті без урахування часу.
            var todayUtc = DateTime.UtcNow.Date;
            var promotionsQuery = _dataContext.Promotions
                .AsNoTracking()
                .Where(p => !p.InActive &&
                    p.ExpiredAt.Date >= todayUtc)
                // Вирівняно з клієнтською сторінкою акцій:
                // для звичайних акцій показуємо тільки ті, де PromoPrice < Price
                .Where(p => p.IsComplexPromotion || p.PromoPrice < p.Price);

            // Limit promotions to the current network (branch)
            // Include promotions with null StoreCode (available for all stores in the branch)
            if (branchId != 0)
            {
                var storeNumbers = _unitOfWork.Stores.GetAllStores()
                    .Where(st => st.BranchId == branchId)
                    .Select(x => x.Number)
                    .ToList();

                promotionsQuery = promotionsQuery.Where(p =>
                    p.StoreCode == null || storeNumbers.Contains(p.StoreCode));
            }

            // Optional: limit to a specific store within the branch
            // Include promotions with null StoreCode (available for all stores)
            if (storeId.HasValue)
            {
                var storeDb = _unitOfWork.Stores.GetAllStores()
                    .FirstOrDefault(st => st.Id == storeId.Value);

                if (storeDb != null)
                {
                    promotionsQuery = promotionsQuery.Where(p => 
                        p.StoreCode == null || p.StoreCode == storeDb.Number);
                }
            }

            // Include Product before applying search filter
            promotionsQuery = promotionsQuery.Include(p => p.Product);

            // Apply search on multiple fields using only EF.Functions.Like (FuzzyContains removed due to issues with longer search strings)
            promotionsQuery = promotionsQuery.Where(p =>
                (p.Title != null && EF.Functions.Like(p.Title.ToLower(), searchPatternLower)) ||
                (p.IsComplexPromotion && (
                    (p.Description != null && EF.Functions.Like(p.Description.ToLower(), searchPatternLower)) ||
                    (p.Offer != null && EF.Functions.Like(p.Offer.ToLower(), searchPatternLower)))) ||
                (p.Product != null && p.Product.Title != null && EF.Functions.Like(p.Product.Title.ToLower(), searchPatternLower)));

            // Materialize a subset and group in memory to avoid EF Core projection issues
            // Product is already included above
            var promotionEntities = await promotionsQuery
                .Take(200) // safety cap before grouping
                .ToListAsync(cancellationToken);

            var uniquePromotions = promotionEntities
                .GroupBy(p => new
                {
                    p.IsComplexPromotion,
                    p.Title,
                    Description = p.IsComplexPromotion ? p.Description : null,
                    p.Offer,
                    p.Price,
                    p.PromoPrice,
                    p.StartedAt,
                    p.ExpiredAt,
                    p.PromotionTypeId,
                    // Вирівнюємо групування з клієнтською сторінкою акцій:
                    // - для звичайних акцій враховуємо StoreCode та ProductCode,
                    //   щоб не зливати різні товари/магазини в один запис
                    StoreCode = p.IsComplexPromotion ? (string?)null : p.StoreCode,
                    ProductCode = p.IsComplexPromotion
                        ? null
                        : (p.ProductCode ?? (p.Product != null ? p.Product.Code : null))
                })
                .Select(g => g.First())
                .OrderByDescending(p => p.IsComplexPromotion)
                .ThenByDescending(p => p.StartedAt)
                .Take(30)
                .Select(p => new PromotionSearchDTO
                {
                    Id = p.Id,
                    // Match display title logic from FlightLoyaltyService:
                    // - For complex promotions: use Offer if present
                    // - Otherwise: prefer Product.Title, fallback to Promotion.Title
                    Title = p.IsComplexPromotion && !string.IsNullOrEmpty(p.Offer)
                        ? p.Offer
                        : (p.Product != null ? p.Product.Title : p.Title),
                    Image = !string.IsNullOrEmpty(p.Image)
                        ? p.Image
                        : p.Product != null
                            ? p.Product.ImagePath
                            : null,
                    // For complex promotions, Price and PromoPrice should be 0 (same as in FlightLoyaltyService)
                    Price = p.IsComplexPromotion ? 0 : Math.Round(p.Price, 2),
                    PromoPrice = p.IsComplexPromotion ? 0 : Math.Round(p.PromoPrice, 2),
                    StartedAt = p.StartedAt,
                    ExpiredAt = p.ExpiredAt
                })
                .ToList();

            var pages = await GetMatchingPagesAsync(trimmedSearch, searchPattern, branchId, cancellationToken);
            var posts = await GetMatchingPostsAsync(trimmedSearch, cancellationToken, branchId);

            globalSearchResult.PromotionSearchResult = uniquePromotions;
            globalSearchResult.PageSearchResult = pages;
            globalSearchResult.PostSearchResult = posts;

            return globalSearchResult;
        }

        private async Task<List<PageSearchDTO>> GetMatchingPagesAsync(string search, string searchPattern, byte branchId, CancellationToken cancellationToken = default)
        {
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            // CRITICAL FIX: Don't use GetAllPages() - it includes Sections and SectionFields which causes massive joins
            // Query Page table directly WITHOUT Includes for much better PostgreSQL performance
            var searchPatternLower = $"%{search.ToLower()}%";
            var pagesByNameQuery = _applicationDataContext.Page
                .AsNoTracking();

            if (branchId != 0)
            {
                pagesByNameQuery = pagesByNameQuery.Where(p => p.BranchId == branchId || p.BranchId == null);
            }

            var pagesByName = await pagesByNameQuery
                .Where(p => p.Published == true && p.ParentId.HasValue && 
                    p.Name != null && EF.Functions.Like(p.Name.ToLower(), searchPatternLower))
                .Select(p => new PageSearchDTO
                {
                    Id = p.Id,
                    Link = p.Link,
                    Name = p.Parent.Name,
                })
                .Take(30) // Reduced limit for PostgreSQL
                .ToListAsync(cancellationToken);

            // Check for cancellation between queries
            cancellationToken.ThrowIfCancellationRequested();

            // Skip section fields search entirely for now - it's causing PostgreSQL timeouts
            // The section fields search with FuzzyContains is too expensive on PostgreSQL
            // TODO: Consider implementing a separate endpoint or background job for section field search
            var pagesByFields = new List<PageSearchDTO>();

            // Only attempt section fields search if search is long enough AND we have few results from name search
            // This prevents expensive queries when we already have good results
            if (search.Length >= 3 && pagesByName.Count < 10)
            {
                try
                {
                    // Use ONLY fast LIKE for section fields - skip FuzzyContains to avoid timeout - case insensitive
                    var matchingSectionFieldIds = await _unitOfWork.SectionFields.GetAll()
                        .AsNoTracking()
                        .Where(sf => sf.Value != null && EF.Functions.Like(sf.Value.ToLower(), searchPatternLower))
                        .Select(sf => sf.SectionId)
                        .Distinct()
                        .Take(20) // Very small limit
                        .ToListAsync(cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    if (matchingSectionFieldIds.Any())
                    {
                        var pageIdsByFields = await _unitOfWork.Sections.GetAll()
                            .AsNoTracking()
                            .Where(s => matchingSectionFieldIds.Contains(s.Id) && s.PageId.HasValue)
                            .Select(s => s.PageId.Value)
                            .Distinct()
                            .Take(20) // Very small limit
                            .ToListAsync(cancellationToken);

                        cancellationToken.ThrowIfCancellationRequested();

                        if (pageIdsByFields.Any())
                        {
                            // Use direct Page access instead of GetAllPages() to avoid Includes
                            var pagesByFieldsQuery = _applicationDataContext.Page
                                .AsNoTracking();

                            if (branchId != 0)
                            {
                                pagesByFieldsQuery = pagesByFieldsQuery.Where(p => p.BranchId == branchId || p.BranchId == null);
                            }
                            
                            pagesByFields = await pagesByFieldsQuery
                                .Where(p => p.BranchId == branchId && p.Published == true && p.ParentId.HasValue && 
                                    pageIdsByFields.Contains(p.Id))
                                .Select(p => new PageSearchDTO
                                {
                                    Id = p.Id,
                                    Link = p.Link,
                                    Name = p.Parent.Name,
                                })
                                .Take(20)
                                .ToListAsync(cancellationToken);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // If cancelled, just return pages by name
                    throw;
                }
                catch
                {
                    // If section fields search fails/timeouts, just skip it and return pages by name
                    // This prevents the entire search from failing
                }
            }

            // Combine results and remove duplicates
            var allResults = pagesByName
                .Concat(pagesByFields)
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .Take(30) // Final limit
                .ToList();

            return allResults;
        }

        private async Task<List<PostSearchDTO>> GetMatchingPostsAsync(string search, CancellationToken cancellationToken, byte branchId)
        {
            var searchPatternLower = $"%{search.ToLower()}%";

            // Використовуємо опубліковані активні статті, аналогічно до Posts/GetPosts(published)
            var postsQuery = _applicationDataContext.Set<Post>()
                .AsNoTracking()
                .Where(p =>
                    p.OriginId == null &&
                    p.Status == true &&
                    p.isActive &&
                    p.PublishedAt.HasValue);

            if (branchId != 0)
            {
                postsQuery = postsQuery.Where(p => p.BranchId == null || p.BranchId == branchId);
            }

            postsQuery = postsQuery.Where(p =>
                (p.Name != null && EF.Functions.Like(p.Name.ToLower(), searchPatternLower)) ||
                (p.Subtitle != null && EF.Functions.Like(p.Subtitle.ToLower(), searchPatternLower)) ||
                (p.Text != null && EF.Functions.Like(p.Text.ToLower(), searchPatternLower)));

            var posts = await postsQuery
                .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .Take(30)
                .Select(p => new PostSearchDTO
                {
                    Id = p.Id,
                    Title = p.Name,
                    Subtitle = p.Subtitle,
                    PublishedAt = p.PublishedAt,
                    ImagePath = p.ImageName != null
                        ? new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
                        {
                            Path = $"{StorageConstants.AppPath}/{p.ImageName}"
                        }.ToString()
                        : string.Empty
                })
                .ToListAsync(cancellationToken);

            return posts;
        }
    }
}
