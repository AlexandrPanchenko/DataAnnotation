using JetFlight.ApplicationDataAccess;
using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Shared.Models.Receipts;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.Service.Services
{
    public interface IReceiptService
    {
        Task<int> CreateAsync(CreateReceiptDto model);
        Task DeleteAsync(int id);
        Task DeleteAsync(string cardCode, string transactionNumber, string posTerminal, string storeCode, string receiptNumber);
        Task<int> DeactivateAsync(int receiptId);
    }

    public class ReceiptService : IReceiptService
    {
        private readonly IntegrationDataContext _integrationDataContext;
        private readonly ApplicationDataContext _applicationDataContext;
        private readonly ICustomerService _customerService;

        public ReceiptService(IntegrationDataContext integrationDataContext, ApplicationDataContext applicationDataContext, ICustomerService customerService)
        {
            _integrationDataContext = integrationDataContext;
            _applicationDataContext = applicationDataContext;
            _customerService = customerService;
        }

        public async Task<int> CreateAsync(CreateReceiptDto model)
        {
            // Check for duplicate receipt before processing
            var existingReceipt = await _integrationDataContext.Receipts
                .FirstOrDefaultAsync(x => x.CardCode == model.CardCode
                    && x.TransactionNumber == model.TransactionNumber
                    && x.PosTerminal == model.PosTerminal
                    && x.StoreCode == model.StoreCode
                    && x.ReceiptNumber == model.ReceiptNumber);

            if (existingReceipt != null)
            {
                // Return existing receipt ID if duplicate found
                return existingReceipt.Id;
            }

            var store = await _applicationDataContext.Stores.FirstOrDefaultAsync(x => x.Number == model.StoreCode);

            if (store == null)
            {
                throw new ArgumentException("Магазин не знайден");
            }

            var (customerId, branchIdFromCard) = await _customerService.TryGetCustomerAndBranchByCardCode(model.CardCode);
            byte branchId = (byte)(branchIdFromCard ?? store.BranchId);

            if (!branchIdFromCard.HasValue)
            {
                await _customerService.EnsureCustomerCardExists(model.CardCode, branchId);
            }

            var lineNums = new HashSet<int>();

            var receiptProducts = await CreateReceiptProductEntitiesAsync(model, lineNums);

            List<ReceiptCustomerCoupon> receiptCustomerCoupons;
            if (customerId.HasValue)
            {
                receiptCustomerCoupons = await CreateReceiptCustomerCouponEntitiesAsync(model, lineNums, customerId.Value);
            }
            else
            {
                if (model.CouponUsages != null && model.CouponUsages.Count > 0)
                    throw new ArgumentException("Купони можна застосувати лише для прив'язаної до клієнта картки");
                receiptCustomerCoupons = new List<ReceiptCustomerCoupon>();
            }

            var receipt = new Receipt
            {
                BranchId = branchId,
                CardCode = model.CardCode,
                IsReturn = model.IsReturn,
                CreatedAt = model.CreatedAt,
                StoreCode = model.StoreCode,
                ReceiptProducts = receiptProducts,
                PosTerminal = model.PosTerminal,
                TransactionNumber = model.TransactionNumber,
                ReceiptNumber = model.ReceiptNumber,
                ReceiptCustomerCoupons = receiptCustomerCoupons,
                TotalAmount = model.TotalAmount,
                TotalAmountWithDiscount = model.TotalAmountWithDiscount,
                AccumulatedBonusesSnapshot = model.AccumulatedBonuses,
                UsedBonusesSnapshot = model.UsedBonuses,
            };

            if (customerId.HasValue && model.AccumulatedBonuses != 0)
            {
                if (model.AccumulatedBonuses > 0)
                {
                    // Нарахування бонусів
                    receipt.CustomerBonusTransaction = new CustomerBonusTransaction
                    {
                        BranchId = branchId,
                        Amount = model.AccumulatedBonuses,
                        AmountRemaining = model.AccumulatedBonuses,
                        Description = "Нарахування за покупку",
                        ExpiredAt = model.CreatedAt.AddYears(1),
                        CardCode = model.CardCode,
                        TransactionDate = model.CreatedAt,
                    };
                }
                else
                {
                    // Повернення нарахованих бонусів (чек повернення)
                    await ProcessReturnAccumulatedBonusesAsync(model, customerId.Value, branchId);
                }
            }

            if (customerId.HasValue && model.UsedBonuses != 0)
            {
                // ВАЖЛИВО: UsedBonuses інтерпретуємо як дельту балансу
                // Продаж:  UsedBonuses < 0  => реальне списання бонусів
                // Повернення: UsedBonuses > 0 => повернення раніше списаних бонусів
                if (model.UsedBonuses < 0)
                {
                    // Списання бонусів (чек продажу)
                    await ProcessUsedBonusesAsync(model, customerId.Value);
                }
                else
                {
                    // Повернення списаних бонусів (чек повернення)
                    await ProcessReturnUsedBonusesAsync(model, customerId.Value);
                }
            }

            if (customerId.HasValue)
                await ProcessAccumulationCardsAsync(model, customerId.Value);

            await _integrationDataContext.Receipts.AddAsync(receipt);
            await _integrationDataContext.SaveChangesAsync();

            return receipt.Id;
        }

        private async Task<List<ReceiptProduct>> CreateReceiptProductEntitiesAsync(CreateReceiptDto model, HashSet<int> lineNums)
        {
            var receiptProducts = new List<ReceiptProduct>();

            foreach (var product in model.ReceiptLines)
            {
                if (product.LineNo == 0)
                {
                    throw new ArgumentException("LineNo не може бути 0");
                }

                if (!lineNums.Add(product.LineNo))
                {
                    throw new ArgumentException("LineNo має бути унікальним в чеку");
                }

                if (!await _integrationDataContext.Products.AnyAsync(x => x.Code == product.ProductCode))
                {
                    throw new ArgumentException($"Продукт не існує: {product.ProductCode}");
                }

                receiptProducts.Add(new ReceiptProduct
                {
                    ItemUnit = product.ItemUnit,
                    ProductCode = product.ProductCode,
                    Discount = product.Discount,
                    LineNo = product.LineNo,
                    Quantity = product.Quantity,
                    Price = product.Price,
                    LineTotalAmount = product.LineTotalAmount,
                    LineTotalAmountWithDiscount = product.LineTotalAmountWithDiscount,
                });
            }
            
            return receiptProducts;
        }

        private async Task<List<ReceiptCustomerCoupon>> CreateReceiptCustomerCouponEntitiesAsync(CreateReceiptDto model, HashSet<int> lineNums, int customerId)
        {
            var receiptCustomerCoupons = new List<ReceiptCustomerCoupon>();

            foreach (var couponUsage in model.CouponUsages)
            {
                if (couponUsage.LineNo != 0 && !lineNums.Contains(couponUsage.LineNo))
                {
                    throw new ArgumentException("Немає такого рядка в чеку");
                }

                if (!await _integrationDataContext.CustomerCoupons.AnyAsync(x => x.Id == couponUsage.CustomerCouponId && x.CustomerId == customerId))
                {
                    throw new ArgumentException("Кастомер не має такого купона");
                }

                receiptCustomerCoupons.Add(new ReceiptCustomerCoupon
                {
                    LineNo = couponUsage.LineNo,
                    CustomerCouponId = couponUsage.CustomerCouponId,
                });
            }

            return receiptCustomerCoupons;
        }

        private async Task ProcessReturnAccumulatedBonusesAsync(CreateReceiptDto model, int customerId, byte branchId)
        {
            // Для чека повернення - знаходимо та зменшуємо AmountRemaining в нарахованих бонусах
            var returnAmount = Math.Abs(model.AccumulatedBonuses);
            
            var customerBonusTransactions = await _integrationDataContext
                .CustomerBonusTransactions
                .Where(x =>
                    x.CustomerCard.CustomerId == customerId
                    && x.AmountRemaining > 0
                    && x.CardCode == model.CardCode
                    && x.BranchId == branchId
                    && (!x.ExpiredAt.HasValue || x.ExpiredAt > DateTime.UtcNow))
                .OrderByDescending(x => x.TransactionDate) // Спочатку найновіші
                .ToListAsync();

            var amount = returnAmount;

            foreach (var customerBonusTransaction in customerBonusTransactions)
            {
                var returnAmountFromTransaction = customerBonusTransaction.AmountRemaining > amount 
                    ? amount 
                    : customerBonusTransaction.AmountRemaining;
                
                customerBonusTransaction.AmountRemaining -= returnAmountFromTransaction;
                amount -= returnAmountFromTransaction;

                if (amount == 0)
                {
                    break;
                }
            }
        }

        private async Task ProcessReturnUsedBonusesAsync(CreateReceiptDto model, int customerId)
        {
            // Для чека повернення - знаходимо BonusUsage та відновлюємо бонуси
            // Працює без зв'язку з оригінальним чеком - шукаємо всі списані бонуси кастомера
            var returnAmount = Math.Abs(model.UsedBonuses);
            
            var bonusUsages = await _integrationDataContext.BonusUsages
                .Include(bu => bu.CustomerBonusTransaction)
                    .ThenInclude(cbt => cbt.CustomerCard)
                .Where(bu =>
                    bu.CustomerBonusTransaction.CustomerCard.CustomerId == customerId
                    && bu.CustomerBonusTransaction.CardCode == model.CardCode
                    && bu.Amount > 0) // Тільки ті, де ще є списана сума
                .OrderByDescending(bu => bu.Id) // Спочатку найновіші (LIFO)
                .ToListAsync();

            var amount = returnAmount;

            foreach (var bonusUsage in bonusUsages)
            {
                if (bonusUsage.CustomerBonusTransaction != null && amount > 0)
                {
                    var returnAmountFromUsage = bonusUsage.Amount > amount ? amount : bonusUsage.Amount;
                    
                    bonusUsage.CustomerBonusTransaction.AmountRemaining += returnAmountFromUsage;
                    bonusUsage.Amount -= returnAmountFromUsage;
                    
                    if (bonusUsage.Amount <= 0)
                    {
                        _integrationDataContext.BonusUsages.Remove(bonusUsage);
                    }
                    
                    amount -= returnAmountFromUsage;
                }
            }
        }

        private async Task ProcessUsedBonusesAsync(CreateReceiptDto model, int customerId)
        {
            // Перевіряємо чи вже є BonusUsage для цієї транзакції (idempotent behavior)
            var existingBonusUsages = await _integrationDataContext.BonusUsages
                .Include(bu => bu.CustomerBonusTransaction)
                .Where(bu =>
                    bu.StoreCode == model.StoreCode
                    && bu.PosTerminal == model.PosTerminal
                    && bu.TransactionNumber == model.TransactionNumber)
                .ToListAsync();

            // Якщо є існуючі записи - відновлюємо бонуси та видаляємо їх
            if (existingBonusUsages.Any())
            {
                foreach (var bonusUsage in existingBonusUsages)
                {
                    if (bonusUsage.CustomerBonusTransaction != null)
                    {
                        bonusUsage.CustomerBonusTransaction.AmountRemaining += bonusUsage.Amount;
                    }
                }
                _integrationDataContext.BonusUsages.RemoveRange(existingBonusUsages);
            }

            var customerBonusTransactions = await _integrationDataContext
                .CustomerBonusTransactions
                .Where(x =>
                    x.CustomerCard.CustomerId == customerId
                    && x.AmountRemaining > 0
                    && x.CardCode == model.CardCode
                    && (!x.ExpiredAt.HasValue || x.ExpiredAt > DateTime.UtcNow))
                .OrderBy(x => x.ExpiredAt.HasValue)
                .ThenBy(x => x.ExpiredAt)
                .ToListAsync();

            var amount = Math.Abs(model.UsedBonuses);

            foreach (var customerBonusTransaction in customerBonusTransactions)
            {
                var usedAmount = customerBonusTransaction.AmountRemaining > amount ? amount : customerBonusTransaction.AmountRemaining;
                customerBonusTransaction.AmountRemaining -= usedAmount;
                amount -= usedAmount;

                _integrationDataContext.BonusUsages.Add(new BonusUsage
                {
                    Amount = usedAmount,
                    CustomerBonusTransactionId = customerBonusTransaction.Id,
                    PosTerminal = model.PosTerminal,
                    TransactionNumber = model.TransactionNumber,
                    StoreCode = model.StoreCode
                });

                if (amount == 0)
                {
                    break;
                }
            }
        }

        private async Task ProcessAccumulationCardsAsync(CreateReceiptDto model, int customerId)
        {
            var productLinesWithCoupons = model.CouponUsages.Where(x => x.LineNo != 0).Select(x => x.LineNo).ToList();

            var possibleCardProductCodes = model.ReceiptLines.Where(x => !productLinesWithCoupons.Contains(x.LineNo)).Select(x => x.ProductCode).ToList();

            if (possibleCardProductCodes.Count != 0)
            {
                var customerAccumulationCards = await _integrationDataContext.CustomerAccumulationCards
                .Include(x => x.AccumulationCard)
                    .ThenInclude(x => x.Coupons)
                    .ThenInclude(x => x.CouponProductFixedPrice)
                .Select(x => new CustomerAccumulationCardAndCouponItem { CustomerAccumulationCard = x, Coupon = x.AccumulationCard.Coupons.First() })
                .Where(x => x.CustomerAccumulationCard.CustomerId == customerId
                    && x.CustomerAccumulationCard.Status == Shared.Models.AccumulationCard.CustomerAccumulationCardStatus.Active
                    && x.Coupon.Status == Shared.Models.Coupons.CouponStatus.Active
                    && x.CustomerAccumulationCard.Counter != x.CustomerAccumulationCard.AccumulationCard.CountToComplete
                    && x.Coupon.StartDate <= model.CreatedAt
                    && x.Coupon.ExpirationDate >= model.CreatedAt
                    && x.Coupon.StoreCodes.Any(x => x.StoreCode == model.StoreCode)
                    && x.CustomerAccumulationCard.AccumulationCard.Coupons.Any(x => possibleCardProductCodes.Contains(x.CouponProductFixedPrice.ProductCode)))
                .ToListAsync();

                var cards = customerAccumulationCards
                    .Select(x => new
                    {
                        x.CustomerAccumulationCard,
                        ProductCodes = x.CustomerAccumulationCard.AccumulationCard.Coupons.Select(x => x.CouponProductFixedPrice.ProductCode)
                    });

                var listUpdatedCardIds = new HashSet<int>();

                foreach (var productCode in possibleCardProductCodes)
                {
                    var cardToUpdate = cards
                        .FirstOrDefault(x => !listUpdatedCardIds.Contains(x.CustomerAccumulationCard.Id) && x.ProductCodes.Contains(productCode));

                    if (cardToUpdate != null)
                    {
                        cardToUpdate.CustomerAccumulationCard.Counter++;
                        listUpdatedCardIds.Add(cardToUpdate.CustomerAccumulationCard.Id);
                    }
                }
            }
        }

        public async Task DeleteAsync(string cardCode, string transactionNumber, string posTerminal, string storeCode, string receiptNumber)
        {
            var receipt = await _integrationDataContext.Receipts
                .Include(x => x.ReceiptCustomerCoupons)
                    .ThenInclude(x => x.CustomerCoupon)
                .FirstOrDefaultAsync(
                    x => x.CardCode == cardCode
                    && x.TransactionNumber == transactionNumber
                    && x.PosTerminal == posTerminal
                    && x.StoreCode == storeCode
                    && x.ReceiptNumber == receiptNumber);

            if (receipt != null)
            {
                // Find and remove related bonus usages
                var bonusUsages = await _integrationDataContext.BonusUsages
                    .Include(x=>x.CustomerBonusTransaction)
                    .Where(bu => bu.StoreCode == storeCode
                        && bu.PosTerminal == posTerminal
                        && bu.TransactionNumber == transactionNumber)
                    .ToListAsync();

                if (bonusUsages.Any())
                {
                    _integrationDataContext.BonusUsages.RemoveRange(bonusUsages);
                }

                await DeleteAsync(receipt);
            }
        }

        public async Task<int> DeactivateAsync(int receiptId)
        {
            var receipt = await _integrationDataContext.Receipts.FirstOrDefaultAsync(x => x.Id == receiptId);

            if (receipt == null)
                throw new ArgumentException("Чек не знайден");

            receipt.IsActive = false;
            await _integrationDataContext.SaveChangesAsync();

            return receipt.Id;
        }

        public async Task DeleteAsync(int id)
        {
            var receipt = await _integrationDataContext.Receipts
                .Include(x => x.ReceiptCustomerCoupons)
                    .ThenInclude(x => x.CustomerCoupon)
                .Include(x => x.CustomerBonusTransaction)
                .FirstOrDefaultAsync(x => x.Id == id);

            await DeleteAsync(receipt);
        }

        private async Task DeleteAsync(Receipt? receipt)
        {
            if (receipt == null)
                throw new ArgumentException("Чек не знайден");

            // Remove and restore bonuses for all BonusUsages matching this receipt
            var bonusUsages = await _integrationDataContext.BonusUsages
                .Include(bu => bu.CustomerBonusTransaction)
                .Where(bu =>
                    bu.StoreCode == receipt.StoreCode &&
                    bu.PosTerminal == receipt.PosTerminal &&
                    bu.TransactionNumber == receipt.TransactionNumber)
                .ToListAsync();

            foreach (var bonusUsage in bonusUsages)
            {
                if (bonusUsage.CustomerBonusTransaction != null)
                {
                    bonusUsage.CustomerBonusTransaction.AmountRemaining += bonusUsage.Amount;
                }
            }

            if (bonusUsages.Any())
            {
                _integrationDataContext.BonusUsages.RemoveRange(bonusUsages);
            }

            // Restore coupon usage counts
            var customerCoupons = receipt.ReceiptCustomerCoupons
                .Select(x => x.CustomerCoupon)
                .Where(x => x != null)
                .DistinctBy(x => x.Id);

            foreach (var customerCoupon in customerCoupons)
            {
                customerCoupon.UsedTimes = Math.Max(0, customerCoupon.UsedTimes - 1);
                customerCoupon.UsedAt = null;
            }

            // Remove related CustomerBonusTransaction if exists
            if (receipt.CustomerBonusTransaction != null)
            {
                _integrationDataContext.CustomerBonusTransactions.Remove(receipt.CustomerBonusTransaction);
            }

            _integrationDataContext.Receipts.Remove(receipt);
            await _integrationDataContext.SaveChangesAsync();
        }
    }
}
