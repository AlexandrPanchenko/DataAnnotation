using System.Xml.Serialization;
using JetFlight.Shared.Models;

namespace JetFlight.Shared.Models.Receipts
{
    [XmlType("CreateReceiptDto")]
    public class CreateReceiptDto
    {
        [XmlElement("cardCode")]
        public string CardCode { get; set; } = default!;
        
        [XmlElement("transactionNumber")]
        public string TransactionNumber { get; set; } = default!;
        
        [XmlElement("posTerminal")]
        public string PosTerminal { get; set; } = default!;
        
        [XmlElement("receiptNumber")]
        public string ReceiptNumber { get; set; } = default!;
        
        [XmlElement("storeCode")]
        public string StoreCode { get; set; } = default!;

        [XmlElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        // Загальна сума чека без знижок (∑ по всіх позиціях до знижок)
        [XmlElement("totalAmount")]
        public decimal TotalAmount { get; set; }

        // Загальна сума чека зі знижками (∑ по всіх позиціях після знижок)
        [XmlElement("totalAmountWithDiscount")]
        public decimal TotalAmountWithDiscount { get; set; }

        // Явна ознака: чек повернення (true) чи звичайний чек продажу (false).
        // Логіка на бекенді й далі орієнтується на знаки AccumulatedBonuses / UsedBonuses,
        // але це поле допомагає інтеграції та дебагу.
        [XmlElement("isReturn")]
        public bool IsReturn { get; set; }

        [XmlElement("accumulatedBonuses")]
        public decimal AccumulatedBonuses { get; set; }

        [XmlElement("usedBonuses")]
        public decimal UsedBonuses { get; set; }

        [XmlArray("receiptLines")]
        [XmlArrayItem("receiptLine")]
        public List<CreateReceiptProductDto> ReceiptLines { get; set; } = new List<CreateReceiptProductDto>();

        [XmlArray("couponUsages")]
        [XmlArrayItem("couponUsage")]
        public List<CouponUsageDto> CouponUsages { get; set; } = new List<CouponUsageDto>();
    }

    [XmlType("CouponUsageDto")]
    public class CouponUsageDto
    {
        [XmlElement("lineNo")]
        public int LineNo { get; set; }
        
        [XmlElement("customerCouponId")]
        public int CustomerCouponId { get; set; }
    }

    [XmlType("CreateReceiptProductDto")]
    public class CreateReceiptProductDto
    {
        [XmlElement("lineNo")]
        public int LineNo { get; set; }
        
        [XmlElement("productCode")]
        public string ProductCode { get; set; } = default!;
        
        [XmlElement("price")]
        public decimal Price { get; set; }
        
        [XmlElement("quantity")]
        public decimal Quantity { get; set; }
        
        [XmlElement("itemUnit")]
        public string ItemUnitString { get; set; } = string.Empty;
        
        [XmlIgnore]
        public ItemUnit ItemUnit => ParseItemUnit(ItemUnitString);
        
        private static ItemUnit ParseItemUnit(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return ItemUnit.Items;
            }
            
            var normalizedValue = value.Trim().ToUpperInvariant();
            
            // Українські текстові значення
            if (normalizedValue == "ШТ" || normalizedValue == "ШТ.")
            {
                return ItemUnit.Items;
            }
            
            if (normalizedValue == "КГ" || normalizedValue == "КГ.")
            {
                return ItemUnit.Kilograms;
            }
            
            if (normalizedValue == "ГР" || normalizedValue == "ГР." || normalizedValue == "100 ГР.")
            {
                return ItemUnit.Grams;
            }
            
            if (normalizedValue == "УП" || normalizedValue == "УП.")
            {
                return ItemUnit.Pack;
            }
            
            // Спробувати як число
            if (int.TryParse(normalizedValue, out int intValue))
            {
                if (Enum.IsDefined(typeof(ItemUnit), intValue))
                {
                    return (ItemUnit)intValue;
                }
            }
            
            // Спробувати як назву enum (англійська)
            if (Enum.TryParse<ItemUnit>(normalizedValue, true, out ItemUnit result))
            {
                return result;
            }
            
            // За замовчуванням
            return ItemUnit.Items;
        }
        
        [XmlElement("discount")]
        public decimal Discount { get; set; }

        // Загальна ціна за кількість однакових товарів без знижки (price * quantity до знижки)
        [XmlElement("lineTotalAmount")]
        public decimal LineTotalAmount { get; set; }

        // Загальна ціна за кількість однакових товарів зі знижкою
        [XmlElement("lineTotalAmountWithDiscount")]
        public decimal LineTotalAmountWithDiscount { get; set; }
    }
}
