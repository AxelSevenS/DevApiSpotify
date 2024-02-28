
using System.Text.Json.Serialization;

namespace ProjectMana;

public record SavedTrack
{
	[JsonPropertyName("added_at")]
	public DateTime AddedAt { get; set; }

	[JsonPropertyName("track")]
	public Track Track { get; set; }
}

public record SavedTrackResponse
{
	[JsonPropertyName("href")]
	public string Href { get; set; }

	[JsonPropertyName("limit")]
	public uint Limit { get; set; }

	[JsonPropertyName("offset")]
	public uint Offset { get; set; }

	[JsonPropertyName("previous")]
	public string Previous { get; set; }

	[JsonPropertyName("total")]
	public uint Total { get; set; }

	[JsonPropertyName("items")]
	public SavedTrack[] Items { get; set; }
}