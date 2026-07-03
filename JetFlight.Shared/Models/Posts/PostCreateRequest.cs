using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JetFlight.Shared.Models.Posts;

public class PostCreateRequest
{
    [Required]
    [JsonPropertyName("name")]
    public string name { get; set; }
    [JsonPropertyName("subtitle")]
    [Required]
    public string subtitle { get; set; }
    [JsonPropertyName("text")]
    [Required]
    public string text { get; set; }
    [JsonPropertyName("publishedAt")]
    public DateTime? publishedAt { get; set; } = null;
    [JsonPropertyName("status")]
    [Required]
    public bool status { get; set; }
    [JsonPropertyName("branchId")]
    public byte? branchId { get; set; }

    [Required]
    public string imageAlt { get; set; }

    [Required]
    [JsonPropertyName("file")]
    public IFormFile file { get; set; }


    [Required]
    [JsonPropertyName("postTags")]
    public List<int> postTags { get; set; }
}