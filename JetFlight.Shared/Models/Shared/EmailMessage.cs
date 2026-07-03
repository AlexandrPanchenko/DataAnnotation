namespace JetFlight.Shared.Models.Shared;

using JetFlight.Shared.Constants;
using System.Collections.Generic;


public class EmailMessage
{
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
    public EmailFrom From { get; set; }
    public List<string> To { get; set; } = default!;
    public List<string>? Cc { get; set; }
    
    public List<EmailAttachment>? Attachments { get; set; }
}

public class EmailAttachment
{
    public string FileName { get; set; } = default!;

    public string Base64Content { get; set; } = default!;

    public string? Cid { get; set; }
}
