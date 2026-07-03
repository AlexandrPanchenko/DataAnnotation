using JetFlight.Shared.Models.Mouseflow;
using Refit;

namespace JetFlight.Service.Services
{
    public interface IMouseflowApi
    {
        [Get("/websites/{websiteId}/pagelist?limit={limit}&offset={offset}&dateFrom={dateFrom}&dateTo={dateTo}")]
        public Task<ApiResponse<MouseflowPageListResponse>> GetWebsitePageListAsync(string websiteId, int? offset, int? limit, [Query(Format = "yyyy-MM-ddTHH:mm:ss")] DateTime dateFrom, [Query(Format = "yyyy-MM-ddTHH:mm:ss")] DateTime dateTo);

        [Get("/websites/{websiteId}/recordings?limit={limit}&offset={offset}&dateFrom={dateFrom}&dateTo={dateTo}")]
        public Task<ApiResponse<MouseflowRecordingListResponse>> GetWebsiteRecordingsListAsync(string websiteId, int? offset, int? limit, [Query(Format = "yyyy-MM-ddTHH:mm:ss")] DateTime dateFrom, [Query(Format = "yyyy-MM-ddTHH:mm:ss")] DateTime dateTo);

    }
}
