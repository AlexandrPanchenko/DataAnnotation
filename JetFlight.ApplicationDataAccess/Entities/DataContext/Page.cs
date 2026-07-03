using JetFlight.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class Page
    {
        public int Id { get; set; }
        public byte? BranchId { get; set; }
        public bool? Published { get; set; }
        public int? OriginId { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public RootPage? RootPage { get; set; }
        public bool IsActive { get; set; }
        public int? Order { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }

        public DateTime? ScheduledPublishDate { get; set; }

        public Page Origin { get; set; }  // Navigation Property

        public Page Parent { get; set; }

        public ICollection<Page> Versions { get; set; }

        public ICollection<Page> Children { get; set; }

        public ICollection<Section> Sections { get; set; } = new List<Section>();
    }
}
