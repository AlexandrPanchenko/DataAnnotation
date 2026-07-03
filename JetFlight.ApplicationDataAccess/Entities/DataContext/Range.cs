namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class Range<T> : ISkipLogHistory
    {
        public int Id { get; set; }
        public T From { get; set; }
        public T To { get; set; }
    }
}
