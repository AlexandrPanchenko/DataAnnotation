using System;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class RfmCustomerSnapshot
    {
        public int CustomerId { get; set; }
        public int RfmId { get; set; }
        public DateOnly SnapshotDate { get; set; }
        public byte? BranchId { get; set; }
        public int? CityId { get; set; }
        public int? StoreId { get; set; }

        public Customer Customer { get; set; }
    }
}
