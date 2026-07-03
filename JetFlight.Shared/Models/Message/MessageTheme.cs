using System.Runtime.Serialization;

namespace JetFlight.Shared.Models.Message
{
    public enum MessageTheme
    {
        [EnumMember(Value = "Акційні та промо-листи")]
        Promotional_And_Promotional_Letters,

        [EnumMember(Value = "Новинки та поповнення асортименту")]
        New_Products_And_Replenishment_Of_The_Assortment,

        [EnumMember(Value = "Програма лояльності / бонуси")]
        Loyalty_Program_Bonuses,

        [EnumMember(Value = "Інформування / новини")]
        Information_News,

        [EnumMember(Value = "Святкові та тематичні")]
        Holiday_And_Thematic
    }
}
