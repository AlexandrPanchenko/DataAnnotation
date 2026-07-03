namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class RFM : ISkipLogHistory
    {
        public int Id { get; set; }
        public string Color { get; set; }
        public Range<int> Period { get; set; } 
        public Range<int> Amount { get; set; }
        public Range<int> Count { get; set; }
        public string Name { get; set; }
    }
}
