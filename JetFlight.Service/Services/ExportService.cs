using JetFlight.Shared.Models.Export;

namespace JetFlight.Service.Services
{
    public interface IExportService
    {
        Task<ExportFile> ExportAsync<T>(IQueryable<T> query, List<string> fields, Func<T, Dictionary<string, object>> toDictionary, string name);
    }
}
