namespace JetFlight.Shared.Models.Shared
{
    public class SmsMessage
    {
        public List<string> Recievers { get; set; }

        public string Message { get; set; }
        public string Sender { get; set; }
    }
}
