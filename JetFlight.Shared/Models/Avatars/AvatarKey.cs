using JetFlight.Shared.Models.Store;
using System.Runtime.Serialization;

namespace JetFlight.Shared.Models.Avatars
{
    public enum AvatarKey
    {
        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.BirdJet)]
        BirdJet1,
        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.BirdJet)]
        BirdJet2,
        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.BirdJet)]
        BirdJet3,
        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.BirdJet)]
        BirdJet4,
        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.BirdJet)]
        BirdJet5,
        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.BirdJet)]
        BirdJet6,
        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.BirdJet)]
        BirdJet7,

        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.CatJet)]
        CatJet1,
        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.CatJet)]
        CatJet2,
        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.CatJet)]
        CatJet3,
        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.CatJet)]
        CatJet4,
        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.CatJet)]
        CatJet5,
        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.CatJet)]
        CatJet6,
        [EnumMember(Value = nameof(AvatarType.Brand))]
        [AvatarFilter(BranchId = Branches.CatJet)]
        CatJet7,

        [EnumMember(Value = nameof(AvatarType.Symbolic))]
        Symblolic1,
        [EnumMember(Value = nameof(AvatarType.Symbolic))]
        Symblolic2,
        [EnumMember(Value = nameof(AvatarType.Symbolic))]
        Symblolic3,
        [EnumMember(Value = nameof(AvatarType.Symbolic))]
        Symblolic4,
        [EnumMember(Value = nameof(AvatarType.Symbolic))]
        Symblolic5,
        [EnumMember(Value = nameof(AvatarType.Symbolic))]
        Symblolic6,
        [EnumMember(Value = nameof(AvatarType.Symbolic))]
        Symblolic7,

        [EnumMember(Value = nameof(AvatarType.Food))]
        Food1,
        [EnumMember(Value = nameof(AvatarType.Food))]
        Food2,
        [EnumMember(Value = nameof(AvatarType.Food))]
        Food3,
        [EnumMember(Value = nameof(AvatarType.Food))]
        Food4,
        [EnumMember(Value = nameof(AvatarType.Food))]
        Food5,
        [EnumMember(Value = nameof(AvatarType.Food))]
        Food6,
        [EnumMember(Value = nameof(AvatarType.Food))]
        Food7,

        [EnumMember(Value = nameof(AvatarType.Memogi))]
        Memogi1,
        [EnumMember(Value = nameof(AvatarType.Memogi))]
        Memogi2,
        [EnumMember(Value = nameof(AvatarType.Memogi))]
        Memogi3,
        [EnumMember(Value = nameof(AvatarType.Memogi))]
        Memogi4,
        [EnumMember(Value = nameof(AvatarType.Memogi))]
        Memogi5,
        [EnumMember(Value = nameof(AvatarType.Memogi))]
        Memogi6,
        [EnumMember(Value = nameof(AvatarType.Memogi))]
        Memogi7,
    }

    public enum AvatarType
    {
        Brand,
        Symbolic,
        Food,
        Memogi,
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class AvatarFilterAttribute : Attribute
    {
        public Branches BranchId { get; set; }
    }
}
