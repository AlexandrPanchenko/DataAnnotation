using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace JetFlight.Shared.Models.LogHistory
{
    public class SectionFieldLogHistoryDTO
    {
        public int? Id { get; set; } = null;
        public int? SectionId { get; set; } = null;
        public string? Section { get; set; } = null;
        public int? MediaFilesId { get; set; } = null;
        public string? Key { get; set; } = null;
        public string? Value { get; set; } = null;
        public string? Type { get; set; } = null;
        public string? Title { get; set; } = null;
        public int? Position { get; set; } = null;
        public bool? Extendable { get; set; } = null;
        public string? SubSectionTitle { get; set; } = null;
        public string? Placeholder { get; set; } = null;

        public string? RelatedTitle { get; set; } = null;
    }
}
