using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;
using JetFlight.WebApi.Controllers;
using System.IO;

namespace JetFlight.WebApi.AdminControllers
{
    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class MediaController : BaseController
    {
        private readonly IMediaService _mediaService;
        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpPost]
        public async Task<Uri> UploadImage(IFormFile formFile)
        {
            return await _mediaService.UploadAsync(formFile);
        }

        [HttpPost("test-upload-original")]
        public async Task<Uri> UploadImageWithOriginalName(IFormFile formFile)
        {
            if (formFile == null || formFile.Length == 0)
            {
                throw new ArgumentException("Файл не може бути порожнім");
            }

            var originalName = Path.GetFileName(formFile.FileName);
            var fileName = string.IsNullOrWhiteSpace(originalName)
                ? $"{DateTime.UtcNow.Ticks}{Path.GetExtension(formFile.FileName)}"
                : originalName;

            var filePath = Path.Combine(StorageConstants.PhysicalPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(stream);
            }

            return new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
            {
                Path = $"{StorageConstants.AppPath}/{fileName}"
            }.Uri;
        }
    }
}
