using JetFlight.Shared.Models.Roles;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.LogHistory
{
    public class SeoMetaResponseDTO
    {

        public List<SeoMetaDTO> seoMetaDtos { get; set; } = default!;
    }
}
