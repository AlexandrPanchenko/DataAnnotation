using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Store;
using SendGrid.Helpers.Mail;

namespace JetFlight.Shared.Models.Shared;

public class EmailSettings
{
    public Dictionary<EmailFrom, EmailAddress> From { get; set;} = new();
    public Dictionary<Branches, string> SubscriptionList { get; set; } = new();
}