using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JetFlight.Shared.Models.Posts;

public class PostUpdateRequest
{
    [Required]
    [JsonPropertyName("id")]
    public int id { get; set; }
    [JsonPropertyName("name")]
    public string? name { get; set; } = null;
    [JsonPropertyName("subtitle")]
    public string? subtitle { get; set; }
    [JsonPropertyName("publishedAt")]
    public DateTime? publishedAt { get; set; } = null;
    [JsonPropertyName("status")]
    public bool? status { get; set; } = null;
    [JsonPropertyName("text")]
    public string? text { get; set; } = null;
    [JsonPropertyName("readDurationMin")]
    public string? readDurationMin { get; set; } = null;
    [JsonPropertyName("imageAlt")]
    public string? imageAlt { get; set; } = null;
    [JsonPropertyName("branchId")]
    public byte? branchId { get; set; } = null;

    [JsonPropertyName("file")]
    public IFormFile? file { get; set; } = null;

    [JsonPropertyName("postTags")]
    [Required]
    public List<int?>? postTags { get; set; } = new List<int?>();

}