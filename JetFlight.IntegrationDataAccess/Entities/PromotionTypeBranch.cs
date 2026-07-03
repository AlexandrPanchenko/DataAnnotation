using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class PromotionTypeBranch
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int BranchId { get; set; }
        public PromotionType PromotionsType { get; set; }
    }
}
