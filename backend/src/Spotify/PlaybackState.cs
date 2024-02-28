
using System.Text.Json.Serialization;

namespace YSpotify;

public record PlaybackState
{
	[JsonPropertyName("device")]
	public Device Device { get; set; }

	[JsonPropertyName("item")]
	public Track Track { get; set; }

	[JsonPropertyName("context")]
	public Context? Context { get; set; }

	[JsonPropertyName("repeat_state")]
	public string RepeatState { get; set; }

	[JsonPropertyName("shuffle_state")]
	public bool ShuffleState { get; set; }

	[JsonPropertyName("timestamp")]
	public ulong Timestamp { get; set; }

	[JsonPropertyName("progress_ms")]
	public uint ProgressMs { get; set; }

	[JsonPropertyName("is_playing")]
	public bool IsPlaying { get; set; }

	[JsonPropertyName("currently_playing_type")]
	public string CurrentlyPlayingType { get; set; }
}


public record Context
{
	[JsonPropertyName("href")]
	public string Href { get; set; }
	
	[JsonPropertyName("uri")]
	public string Uri { get; set; }

	[JsonPropertyName("type")]
	public string Type { get; set; }

}