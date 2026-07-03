using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.Shared
{
    public class FileUploadModel
    {
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
    }
}
