using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models
{
    public enum SortingEnum
    {
        HighestPrice,       // Sort by highest price
        LowestPrice,        // Sort by lowest price
        NewestPromotions,   // Sort by newest promotions
        AlphabeticalAsc,    // Sort alphabetically (A-Z)
        AlphabeticalDesc,   // Sort alphabetically (Z-A)
        StartDateDesc,      // Sort by start date (newest first)
        StartDateAsc        // Sort by start date (oldest first)
    }
}
