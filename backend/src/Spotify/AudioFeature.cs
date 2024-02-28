
using System.Text.Json.Serialization;

namespace ProjectMana;

public record AudioFeature
{
	[JsonPropertyName("danceability")]
	public float Danceability { get; set; }

	[JsonPropertyName("key")]
	public uint Key { get; set; }

	[JsonPropertyName("loudness")]
	public float Loudness { get; set; }

	[JsonPropertyName("speechiness")]
	public float Speechiness { get; set; }

	[JsonPropertyName("acousticness")]
	public float Acousticness { get; set; }

	[JsonPropertyName("instrumentalness")]
	public float Instrumentalness { get; set; }

	[JsonPropertyName("valence")]
	public float Valence { get; set; }

	[JsonPropertyName("tempo")]
	public float Tempo { get; set; }

	[JsonPropertyName("type")]
	public string Type { get; set; }

	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("uri")]
	public string Uri { get; set; }

	[JsonPropertyName("duration_ms")]
	public uint DurationMs { get; set; }
}

public record AudioFeatureResponse
{
	[JsonPropertyName("audio_features")]
	public AudioFeature[] AudioFeatures { get; set; }
}