using System.Runtime.Serialization;
using JetFlight.Shared.Extensions;

namespace JetFlight.Shared.Models.Store
{
    public enum Branches
    {
        [EnumMember(Value = "BirdJet")]
        [EnumDisplay("BirdJet")]
        BirdJet = 1,

        [EnumMember(Value = "CatJet")]
        [EnumDisplay("CatJet")]
        CatJet
    }
}
