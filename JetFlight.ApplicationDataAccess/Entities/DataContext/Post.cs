namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class Post
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? OriginId { get; set; }
        public string Subtitle { get; set; }
        public string Text { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool? Published { get; set; }
        public bool? Status { get; set; }
        public bool isActive { get; set; }

        public string ImageMimeType { get; set; }
        public string ImageName { get; set; }
        public string ImageSize { get; set; }
        public string ImageAlt { get; set; }
        public string ReadDurationMin { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public byte? BranchId { get; set; }
        public Post Origin { get; set; }
        public ICollection<PostToTag> PostTags { get; set; }

    }
}
