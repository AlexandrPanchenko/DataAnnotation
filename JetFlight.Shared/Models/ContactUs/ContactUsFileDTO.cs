using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.ContactUs
{
    public class ContactUsFileDTO
    {
        [Required]
        public string? name {get;set;}
        [Required]
        public IFormFile? file { get; set; } = null
;
    } 
}
