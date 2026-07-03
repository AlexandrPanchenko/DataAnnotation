using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.IntegrationDataAccess;
using JetFlight.Shared.Models.Product;
using JetFlight.Shared.Models.Shared;
using Microsoft.EntityFrameworkCore;
using JetFlight.Service.Extensions;
using JetFlight.ApplicationDataAccess.Repository.DataContext;


namespace JetFlight.Service.Services
{
    public interface IProductService
    {
        Task<PagedListDTO<ProductDTO>> GetAll(PagingDTO pagingDto, string? searchParam = null, int? branchId = null, string storeIds = null, DateTime? createdTime = null);
        Task<List<ProductDTO>> GetProductsByIdsAsync(string ids);
        Task<bool> UpdateProductAsync(string code, UpdateProductDTO updateProductDTO);
        Task<PagedListDTO<ManufacturerDTO>> GetManufacturersAsync(PagingDTO pagingDto, string? searchParam = null);
        Task<PagedListDTO<CategoryDTO>> GetCategoriesAsync(PagingDTO pagingDto, string? searchParam = null);
        Task<List<ManufacturerDTO>> GetManufacturersByIdsAsync(string ids);
        Task<List<CategoryDTO>> GetCategoriesByIdsAsync(string ids);
        Task<PagedListDTO<SupplierDTO>> GetSuppliersAsync(PagingDTO pagingDto, string? searchParam = null);
        Task<List<SupplierDTO>> GetSuppliersByIdsAsync(string ids);
        Task<PagedListDTO<BrandDTO>> GetBrandsAsync(PagingDTO pagingDto, string? searchParam = null);
        Task<List<BrandDTO>> GetBrandsByIdsAsync(string ids);
    }

    public class ProductService : IProductService
    {
        private readonly IntegrationDataContext _dataContext;
        private readonly IDataUnitOfWork _appUnitOfWork;
        private readonly IMediaService _mediaService;

        public ProductService(
            IntegrationDataContext dataContext,
            IDataUnitOfWork appUnitOfWork,
            IMediaService mediaService)
        {
            _appUnitOfWork = appUnitOfWork;
            _dataContext = dataContext;
            _mediaService = mediaService;
        }

        public async Task<PagedListDTO<ProductDTO>> GetAll(PagingDTO pagingDto, string? searchParam = null, int? branchId = null, string storeIds = null, DateTime? createdTime = null)
        {
            // Base query for filtering and paging
            var baseQuery = _dataContext.Products
                .Where(p => p.InActive != true || p.InActive == null)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(searchParam))
            {
                // Normalize spaces in searchParam
                var normalizedSearch = System.Text.RegularExpressions.Regex.Replace(searchParam.Trim(), "\\s+", " ", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1));
                // Normalize spaces in product title (SQL translation: replace double spaces with single, up to 5 times)
                baseQuery = baseQuery.Where(x =>
                    EF.Functions.Like(
                        x.Title.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " "),
                        $"%{normalizedSearch}%"
                    )
                );
            }

            if (createdTime.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.CreatedAt.Date == createdTime.Value.Date);
            }

            // Get total count before paging
            var totalItems = await baseQuery.CountAsync();

            // Paging and projection with explicit joins
            var pagedQuery = baseQuery
                .OrderBy(p => p.Code)
                .Skip(pagingDto.Skip)
                .Take(pagingDto.Take)
                .Select(p => new {
                    p.Code,
                    p.Title,
                    Image = p.ImagePath,
                    p.CreatedAt,
                    p.UpdatedAt,
                    FamilyCode = p.FamilyCode,
                    BrandCode = p.BrandCode
                });

            var items = await (
                from t in pagedQuery
                join fam in _dataContext.ProductFamilies on t.FamilyCode equals fam.Code into famJoin
                from fam in famJoin.DefaultIfEmpty()
                join cat in _dataContext.ProductCategories on fam.CategoryCode equals cat.Code into catJoin
                from cat in catJoin.DefaultIfEmpty()
                join brand in _dataContext.ProductBrands on t.BrandCode equals brand.Code into brandJoin
                from brand in brandJoin.DefaultIfEmpty()
                join manuf in _dataContext.ProductManufacturers on brand.ManufacturerCode equals manuf.Code into manufJoin
                from manuf in manufJoin.DefaultIfEmpty()
                where t.Title != null
                orderby t.Code
                select new ProductDTO
                {
                    StoreProducts = new List<StoreProductDTO>(),
                    Code = t.Code,
                    Title = t.Title,
                    Image = t.Image,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    GroupTitle = fam != null ? fam.Title : null,
                    CategoryTitle = cat != null ? cat.Title : null,
                    BrandTitle = brand != null ? brand.Title : null,
                    ManufacturerTitle = manuf != null ? manuf.Title : null
                }
            ).ToListAsync();

            return new PagedListDTO<ProductDTO>
            {
                TotalItems = totalItems,
                Items = items,
                Offset = pagingDto.Skip,
                Limit = pagingDto.Take
            };
        }

        public async Task<List<ProductDTO>> GetProductsByIdsAsync(string ids)
        {
            var idList = ids.Split(',').ToList();

            var productDTOs = await _dataContext.Products
                .Where(p => idList.Contains(p.Code) && p.InActive != true) // Show only active products
                .Select(product => new ProductDTO
                {
                    Code = product.Code,
                    Title = product.Title,
                    Image = product.ImagePath,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    GroupTitle = product.Family.Title,
                    CategoryTitle = product.Family.Category.Title,
                    BrandTitle = product.Brand.Title,
                    ManufacturerTitle = product.Brand.Manufacturer.Title,
                    //Suppliers = product.Suppliers.Select(x => new SupplierDTO
                    //{
                    //    Code = x.Code,
                    //    Title = x.Title
                    //}).ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            // Enrich each product
            //foreach (var productDTO in productDTOs)
            //{
            //    await EnrichProductAsync(productDTO);
            //}

            return productDTOs;
        }

        public async Task EnrichProductAsync(ProductDTO productDTO)
        {
            if (productDTO.StoreProducts != null)
            {
                foreach (var storeProduct in productDTO.StoreProducts)
                {
                    var store = await _appUnitOfWork.Stores.GetAllStores().FirstAsync(x => x.Id == storeProduct.StoreId);

                    storeProduct.Address = store.Address;
                    storeProduct.Address2 = store.Address2;
                    storeProduct.City = store.City.Name;
                    storeProduct.Region = store.Region;
                }
            }
        }

        public async Task<List<ManufacturerDTO>> GetManufacturersByIdsAsync(string ids)
        {
            var idList = ids.Split(',').ToList();
            var manufacturers = await _dataContext.ProductManufacturers
                .Where(m => idList.Contains(m.Code))
                .Select(m => new ManufacturerDTO
                {
                    Code = m.Code,
                    Title = m.Title
                })
                .ToListAsync();

            return manufacturers;
        }

        public async Task<List<CategoryDTO>> GetCategoriesByIdsAsync(string ids)
        {
            var idList = ids.Split(',').ToList();
            var categories = await _dataContext.ProductCategories
                .Where(c => idList.Contains(c.Code))
                .Select(c => new CategoryDTO
                {
                    Code = c.Code,
                    Title = c.Title,
                })
                .ToListAsync();

            return categories;
        }

        public async Task<PagedListDTO<CategoryDTO>> GetCategoriesAsync(PagingDTO pagingDto, string? searchParam = null)
        {
            var query = _dataContext.ProductCategories.AsQueryable();

            if (!string.IsNullOrEmpty(searchParam))
            {
                query = query.Where(x => x.Title.Contains(searchParam));
            }

            return await query.GetPagedListAsync(pagingDto,
                x => new CategoryDTO { Code = x.Code, Title = x.Title });
        }

        public async Task<PagedListDTO<ManufacturerDTO>> GetManufacturersAsync(PagingDTO pagingDto, string? searchParam = null)
        {
            var query = _dataContext.ProductManufacturers.AsQueryable();

            if (!string.IsNullOrEmpty(searchParam))
            {
                query = query.Where(x => x.Title.Contains(searchParam));
            }

            return await query.GetPagedListAsync(pagingDto,
                x => new ManufacturerDTO { Code = x.Code, Title = x.Title });
        }

        public async Task<bool> UpdateProductAsync(string code, UpdateProductDTO updateProductDTO)
        {
            var product = await _dataContext.Products.FindAsync(code);
            if (product == null)
            {
                return false;
            }

            if (updateProductDTO.file != null)
            {
                product.ImagePath = (await _mediaService.UploadAsync(updateProductDTO.file)).ToString();
                using var memoryStream = new MemoryStream();
                await updateProductDTO.file.CopyToAsync(memoryStream);
                product.Image = memoryStream.ToArray();
                product.OriginalFileName = updateProductDTO.file.FileName;
            }
            product.UpdatedAt = DateTime.UtcNow;


            _dataContext.Products.Update(product);
            await _dataContext.SaveChangesAsync();

            return true;
        }


        private async Task<ProductDTO> ToDTOAsync(Product product)
        {
            // Fetch store details for the product's StoreProducts
            //var storeProducts = new List<StoreProductDTO>();
            //if (product.StoreProducts != null)
            //{
            //    foreach (var storeProduct in product.StoreProducts)
            //    {
            //        var store = await _appUnitOfWork.Stores.GetAllStores()
            //            .FirstOrDefaultAsync(x => x.Number == storeProduct.StoreCode);

            //        if (store != null)
            //        {
            //            storeProducts.Add(new StoreProductDTO
            //            {
            //                Price = storeProduct.Price,
            //                StoreId = store.Id,
            //                Address = store.Address,
            //                Address2 = store.Address2,
            //                City = store.City.Name,
            //                Region = store.Region
            //            });
            //        }
            //    }
            //}

            return new ProductDTO
            {
                Code = product.Code,
                Title = product.Title,
                Image = product.ImagePath,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                GroupTitle = product.Family?.Title,
                CategoryTitle = product.Family?.Category?.Title,
                ManufacturerTitle = product.Brand?.Title,
                BrandTitle = product.Brand?.Manufacturer?.Title,

            };
        }

        public async Task<PagedListDTO<SupplierDTO>> GetSuppliersAsync(PagingDTO pagingDto, string? searchParam = null)
        {
            var query = _dataContext.ProductsSuppliers.AsQueryable();
            if (!string.IsNullOrEmpty(searchParam))
            {
                query = query.Where(x => x.Title.ToLower().Contains(searchParam.ToLower()));
            }

            return await query.GetPagedListAsync(pagingDto, ToSupplierDTO);
        }

        public async Task<List<SupplierDTO>> GetSuppliersByIdsAsync(string ids)
        {
            var idList = ids.Split(',').ToList();
            var suppliers = await _dataContext.ProductsSuppliers
                .Where(c => idList.Contains(c.Code))
                .Select(c => ToSupplierDTO(c))
                .ToListAsync();

            return suppliers;
        }

        private static SupplierDTO ToSupplierDTO(ProductsSupplier s)
            => new SupplierDTO
            {
                Code = s.Code,
                Title = s.Title,
            };

        public async Task<PagedListDTO<BrandDTO>> GetBrandsAsync(PagingDTO pagingDto, string? searchParam = null)
        {
            var query = _dataContext.ProductBrands.AsQueryable();

            if (!string.IsNullOrEmpty(searchParam))
            {
                query = query.Where(x => x.Title.Contains(searchParam));
            }

            return await query.GetPagedListAsync(pagingDto, ToBrandDTO);
        }

        public async Task<List<BrandDTO>> GetBrandsByIdsAsync(string ids)
        {
            var idList = ids.Split(',').ToList();
            var brands = await _dataContext.ProductBrands
                .Where(c => idList.Contains(c.Code))
                .Select(c => ToBrandDTO(c))
                .ToListAsync();

            return brands;
        }

        private static BrandDTO ToBrandDTO(ProductBrand s)
            => new BrandDTO
            {
                Code = s.Code,
                Title = s.Title,
                //Image = s.Image,
            };
    }
}
