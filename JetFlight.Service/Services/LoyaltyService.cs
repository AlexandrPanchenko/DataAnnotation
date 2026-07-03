using JetFlight.IntegrationDataAccess;
using JetFlight.Shared.Models.Loyalty;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.Service.Services
{
    public interface ILoyaltyService
    {
        Task<LoyaltyDto> GetLoyaltyAsync(string cardCode);
    }

    public class LoyaltyService : ILoyaltyService
    {
        private readonly ICouponService _couponService;
        private readonly ICustomerService _customerService;
        private readonly IntegrationDataContext _integrationDataContext;

        public LoyaltyService(
            ICouponService couponService,
            ICustomerService customerService,
            IntegrationDataContext integrationDataContext)
        {
            _couponService = couponService;
            _customerService = customerService;
            _integrationDataContext = integrationDataContext;
        }

        public async Task<LoyaltyDto> GetLoyaltyAsync(string cardCode)
        {
            var (customerId, branchId) = await _customerService.GetCustomerAndBranchByCardCode(cardCode);

            var bonusAmount = await _customerService.GetAvailableBonusesAsync(customerId);
            var coupons = await _couponService.GetAllCustomerCouponsByCustomerId(customerId, branchId);
            var settings = await _integrationDataContext.CustomerSettings.FirstOrDefaultAsync(x => x.Id == customerId && x.BranchId == branchId);

            var dto = new LoyaltyDto
            {
                CustomerCoupons = coupons,
                Bonuses = bonusAmount,
                AccumulateRest = (settings?.AccumulateRest).GetValueOrDefault(),
                AutomaticWithdrawal = (settings?.AutomaticWithdrawal).GetValueOrDefault(),
            };

            return dto;
        }
    }
}
