using System.Text.Json.Serialization;

namespace ProjectMana;

public record CreatePlaylistResponse
{
	[JsonPropertyName("collaborative")]
	public bool Collaborative { get; set; }
	
	[JsonPropertyName("description")]
	public string Description { get; set; }
	
	[JsonPropertyName("href")]
	public string Href { get; set; }
	
	[JsonPropertyName("id")]
	public string Id { get; set; }
	
	[JsonPropertyName("uri")]
	public string Uri { get; set; }
	
	[JsonPropertyName("snapshot_id")]
	public string SnapshotId { get; set; }
}