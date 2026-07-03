using Microsoft.AspNetCore.Mvc;
using JetFlight.Service.Services;
using JetFlight.Shared.Models.Shared;
using JetFlight.Service.Validators;
using JetFlight.Shared.Models.Product;
using JetFlight.Shared.Constants;
using JetFlight.Shared.UserContext;
using Microsoft.AspNetCore.Authorization;
using JetFlight.WebApi.Controllers;


namespace JetFlight.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = UserRole.Admin)]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(PagedListDTO<ProductDTO>), 200)]
        public async Task<IActionResult> GetProducts(string? searchParam = null, int? branchId = null, string storeIds = null, int? offset = null, int? limit = null, DateTime? createdTime = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(ProductDTO), "CreatedAt", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            var productList = await _productService.GetAll(validatePagingParamsDTO.PagingDTO, searchParam, branchId, storeIds, createdTime);
            return Ok(productList);
        }

        [HttpPut("[action]")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProduct(string code, [FromForm] UpdateProductDTO updateProductDTO)
        {
            var result = await _productService.UpdateProductAsync(code, updateProductDTO);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(List<ProductDTO>), 200)]
        public async Task<IActionResult> GetProductsById(string ids)
        {
            var result = await _productService.GetProductsByIdsAsync(ids);

            return Ok(result);
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(PagedListDTO<CategoryDTO>), 200)]
        public async Task<IActionResult> GetCategories(int? offset = null, int? limit = null, string? searchParam = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(CategoryDTO), "Code", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }

            return Ok(await _productService.GetCategoriesAsync(validatePagingParamsDTO.PagingDTO, searchParam));
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(PagedListDTO<ManufacturerDTO>), 200)]
        public async Task<IActionResult> GetManufacturers(string? searchParam = null, int? offset = null, int? limit = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(ManufacturerDTO), "Code", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }

            return Ok(await _productService.GetManufacturersAsync(validatePagingParamsDTO.PagingDTO, searchParam));
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(List<ManufacturerDTO>), 200)]
        public async Task<ActionResult<List<ManufacturerDTO>>> GetManufacturersByIds([FromQuery] string ids)
        {
            var manufacturers = await _productService.GetManufacturersByIdsAsync(ids);
            return Ok(manufacturers);
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(List<CategoryDTO>), 200)]
        public async Task<ActionResult<List<CategoryDTO>>> GetCategoriesByIds([FromQuery] string ids)
        {
            var categories = await _productService.GetCategoriesByIdsAsync(ids);
            return Ok(categories);
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(PagedListDTO<SupplierDTO>), 200)]
        public async Task<IActionResult> GetSuppliers(string? searchParam = null, int? offset = null, int? limit = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(SupplierDTO), "Code", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            var suppliers = await _productService.GetSuppliersAsync(validatePagingParamsDTO.PagingDTO, searchParam);
            return Ok(suppliers);
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(List<SupplierDTO>), 200)]
        public async Task<ActionResult<List<SupplierDTO>>> GetSuppliersByIds([FromQuery] string ids)
        {
            var suppliers = await _productService.GetSuppliersByIdsAsync(ids);
            return Ok(suppliers);
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(PagedListDTO<BrandDTO>), 200)]
        public async Task<IActionResult> GetBrands(string? searchParam = null, int? offset = null, int? limit = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(BrandDTO), "Code", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            var brands = await _productService.GetBrandsAsync(validatePagingParamsDTO.PagingDTO, searchParam);
            return Ok(brands);
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(List<BrandDTO>), 200)]
        public async Task<ActionResult<List<SupplierDTO>>> GetBrandsByIds([FromQuery] string ids)
        {
            var brands = await _productService.GetBrandsByIdsAsync(ids);
            return Ok(brands);
        }
    }
}