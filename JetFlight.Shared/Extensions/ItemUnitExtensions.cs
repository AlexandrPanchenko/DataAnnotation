namespace JetFlight.Shared.Models
{
    public static class ItemUnitExtensions
    {
        public static string ToDisplayString(this ItemUnit itemUnit)
        {
            return itemUnit switch
            {
                ItemUnit.Items => "шт.",
                ItemUnit.Grams => "100 гр.",
                ItemUnit.Kilograms => "кг",
                ItemUnit.Pack => "уп.",
                _ => "шт."
            };
        }
    }
}

