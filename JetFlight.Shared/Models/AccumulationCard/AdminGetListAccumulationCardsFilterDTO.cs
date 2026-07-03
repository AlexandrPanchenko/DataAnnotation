namespace JetFlight.Shared.Models.AccumulationCard
{
    public class AdminGetListAccumulationCardsFilterDTO
    {
        public int? offset { get; set; }
        public int? limit { get; set; }
        public string? searchParam { get; set; }
        public byte? branchId { get; set; }
        public int? cityId { get; set; }
        public int? storeId { get; set; }
        public DateOnly? date { get; set; }
        public AccumulationCardStatus? status { get; set; }
    }
}
