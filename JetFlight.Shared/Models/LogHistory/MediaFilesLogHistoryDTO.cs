using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.LogHistory
{
    public class MediaFilesLogHistoryDTO
    {
        public int? Id { get; set; } = null;
        public string? MimeType { get; set; } = null;
        public string? Name { get; set; } = null;
        public string? Size { get; set; } = null;
        public string? Width { get; set; } = null;
        public string? Height { get; set; } = null;
    }
}
