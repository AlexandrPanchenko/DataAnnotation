using JetFlight.Shared.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.Shared
{
    public class SmsSettings
    {
        public string BaseUrl { get; set; }
        public SmsAuthSettings Auth { get; set; }
        public FromSettings From { get; set; }
    }
    public class FromSettings
    {
        public SmsSender BirdJet { get; set; }
        public SmsSender CatJet { get; set; }
    }

    public class SmsSender
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
