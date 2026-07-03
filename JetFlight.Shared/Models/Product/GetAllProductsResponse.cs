using JetFlight.Shared.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.Product
{
    public class GetAllProductsResponse
    {
        public int Total { get; set; }
        public List<ProductDTO> Items { get; set; }
    }
}
