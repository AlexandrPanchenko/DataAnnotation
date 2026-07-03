using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.Product
{
    public class UpdateProductDTO
    {
        public IFormFile? file { get; set; } = null;
    }
}
