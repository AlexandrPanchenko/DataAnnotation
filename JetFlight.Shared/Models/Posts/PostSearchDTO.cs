using System;

namespace JetFlight.Shared.Models.Posts
{
    /// <summary>
    /// Коротка інформація про статтю для глобального пошуку.
    /// Повний контент можна отримати через /v1/client/Posts/published/{id}.
    /// </summary>
    public class PostSearchDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public string ImagePath { get; set; } = string.Empty;
    }
}

