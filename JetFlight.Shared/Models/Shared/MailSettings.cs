namespace JetFlight.Shared.Models.Shared;

public class MailSettings
{
    public string Mail { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public bool UseSSL { get; set; }
    public bool UseStartTls { get; set; }
}
