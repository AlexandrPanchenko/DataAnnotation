using System.Collections.Generic;

namespace JetFlight.Shared.Models.Promotion;

public class PromotionDetailsClientDTO
{
    public PromotionDTO Promotion { get; set; }
    public List<PromotionStoreDTO> PromotionStores { get; set; }
    public List<PromotionsTagDTO> PromotionsTag { get; set; }
    public PromotionsTypeDTO PromotionsType { get; set; }
    public List<PromotionsCategoryDTO> PromotionsCategory { get; set; }
    public List<PromotionItemDTO> PromotionItems { get; set; }
}
