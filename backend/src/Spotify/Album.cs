
using System.Text.Json.Serialization;

namespace YSpotify;

public record class Album {
    [JsonPropertyName("album_type")]
    public string AlbumType { get; set; }

    [JsonPropertyName("total_tracks")]
    public uint TotalTracks { get; set; }
    
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("href")]
    public string Href { get; set; }
    
    [JsonPropertyName("uri")]
    public string Uri { get; set; }
}
