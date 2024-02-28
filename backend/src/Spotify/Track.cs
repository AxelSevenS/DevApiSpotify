
using System.Text.Json.Serialization;

namespace YSpotify;

/// <summary>
/// Model of a track belonged to a user in the database 
/// </summary>
public record class Track
{
    [JsonPropertyName("album")]
    public Album Album { get; set; }

    [JsonPropertyName("artists")]
    public Artist[] Artists { get; set; }
    
    [JsonPropertyName("duration_ms")]
    public uint DurationMs { get; set; }
    
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("href")]
    public string Href { get; set; }
    
    [JsonPropertyName("uri")]
    public string Uri { get; set; }
}
