namespace JetFlight.Shared.Models.Promotion;

public class PromotionDetailsAdminDTO
{
    public PromotionDTO Promotion { get; set; }
    public List<PromotionStoreDTO> PromotionStores { get; set; }
    public string StoresList { get; set; }
    public PromotionsTypeDTO PromotionsType { get; set; }
    public List<PromotionItemDTO> PromotionItems { get; set; }
    public int NumberOfProducts { get; set; }
}
