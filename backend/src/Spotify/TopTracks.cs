
using System.Text.Json.Serialization;

namespace ProjectMana;

public record TopTracksResponse
{
	[JsonPropertyName("href")]
	public string Href { get; set; }

	[JsonPropertyName("limit")]
	public uint Limit { get; set; }

	[JsonPropertyName("next")]
	public string Next { get; set; }

	[JsonPropertyName("offset")]
	public uint Offset { get; set; }

	[JsonPropertyName("previous")]
	public string Previous { get; set; }

	[JsonPropertyName("total")]
	public uint Total { get; set; }

	[JsonPropertyName("items")]
	public Track[] Items { get; set; }
}
