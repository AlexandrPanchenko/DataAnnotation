using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetFlight.Shared.Models.AccumulationCard;
using JetFlight.Shared.Models.Coupons;
using JetFlight.Shared.Models.LogHistory;
using Newtonsoft.Json;
using static JetFlight.Shared.Constants.RouteConstants;

namespace JetFlight.Shared
{
    public static class LogHistoryHelper
    {
        public static List<LogMessage> GetPublishCouponLog(int couponId, int? adminId)
        {
            var from = new CouponLogHistoryDTO
            {
                Status = CouponStatus.Inactive,
            };

            var to = new CouponLogHistoryDTO()
            {
                Status = CouponStatus.Active,
            };

            return new List<LogMessage>
            {
                new LogMessage
                {
                    AdminId = adminId,
                    EntityType = "Coupons",
                    UpdatedFrom = JsonConvert.SerializeObject(from),
                    UpdatedTo = JsonConvert.SerializeObject(to),
                    EntityId = couponId,
                    Action = "Updated",
                    Date = DateTime.UtcNow,
                }
            };
        }

        public static List<LogMessage> GetPublishAccumulationCardLog(int accumulationCardId, int? adminId)
        {
            var from = new AccumulationCardLogHistoryDTO()
            {
                Status = AccumulationCardStatus.Inactive,
            };

            var to = new AccumulationCardLogHistoryDTO()
            {
                Status = AccumulationCardStatus.Active,
            };

            return new List<LogMessage>
            {
                new LogMessage
                {
                    AdminId = adminId,
                    EntityType = "AccumulationCards",
                    UpdatedFrom = JsonConvert.SerializeObject(from),
                    UpdatedTo = JsonConvert.SerializeObject(to),
                    EntityId = accumulationCardId,
                    Action = "Updated",
                    Date = DateTime.UtcNow,
                }
            };
        }
    }
}
