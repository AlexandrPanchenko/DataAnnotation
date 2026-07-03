using DocumentFormat.OpenXml.Wordprocessing;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using MimeDetective;
using Org.BouncyCastle.Utilities.Zlib;

namespace JetFlight.Service.Services
{
    public interface IMediaService
    {
        Task<Uri> UploadAsync(IFormFile file);

        Task<Uri> UploadAsync(byte[] file, string? originalFileName);

        Task<Uri> CopyAsync(string mediaName);
    }

    public class MediaService : IMediaService
    {
        private static readonly IContentInspector _inspector = new ContentInspectorBuilder()
        {
            Definitions = MimeDetective.Definitions.DefaultDefinitions.All()
        }.Build();

        public MediaService()
        {
        }


        public async Task<Uri> UploadAsync(IFormFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("Файл не може бути null");
            }

            var fileName = $"{DateTime.UtcNow.Ticks.ToString()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(StorageConstants.PhysicalPath, fileName);


            // Save the file to the specified path
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
            {
                Path = $"{StorageConstants.AppPath}/{fileName}"
            }.Uri;
        }

        public async Task<Uri> UploadAsync(byte[] fileData, string? originalFileName)
        {
            if (fileData == null || fileData.Length == 0)
            {
                throw new ArgumentNullException("Файл не може бути null");
            }

            var extension = !string.IsNullOrEmpty(originalFileName)
                ? Path.GetExtension(originalFileName)
                : ".bin";

            if (originalFileName == null)
            {
                var results = _inspector.Inspect(fileData);
                var fileType = results.FirstOrDefault();
                extension = fileType?.Definition.File.Extensions.FirstOrDefault() ?? "bin";
                extension = $".{extension}";
            }

            var fileName = $"{DateTime.UtcNow.Ticks}{extension}";
            var filePath = Path.Combine(StorageConstants.PhysicalPath, fileName);

            await File.WriteAllBytesAsync(filePath, fileData);

            return new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
            {
                Path = $"{StorageConstants.AppPath}/{fileName}"
            }.Uri;
        }


        public async Task<Uri> CopyAsync(string mediaName)
        {
            if (string.IsNullOrWhiteSpace(mediaName))
            {
                throw new ArgumentException("Файл не може бути null");
            }

            var sourceFilePath = Path.Combine(StorageConstants.PhysicalPath, mediaName);

            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("Файл не знайдено");
            }

            var fileExtension = Path.GetExtension(mediaName);
            var newFileName = $"{DateTime.UtcNow.Ticks}{fileExtension}";
            var destinationFilePath = Path.Combine(StorageConstants.PhysicalPath, newFileName);

            try
            {
                await using var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
                await using var destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write);
                await sourceStream.CopyToAsync(destinationStream);

                return new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
                {
                    Path = $"{StorageConstants.AppPath}/{newFileName}"
                }.Uri;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access denied when copying file: {mediaName}", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new InvalidOperationException($"Directory not found when copying file: {mediaName}", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"IO error occurred when copying file: {mediaName}", ex);
            }
        }
    }
}
