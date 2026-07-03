using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class PostToTag
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int CategoryId { get; set; }

        public Post Post { get; set; }
        public PostTag Category { get; set; }
    }
}
