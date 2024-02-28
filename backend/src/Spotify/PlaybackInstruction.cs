using System.Text.Json.Serialization;


namespace ProjectMana;

public record PlaybackInstruction
{
	[JsonPropertyName("context_uri")]
	public string ContextUri { get; set; }
	
	[JsonPropertyName("uris")]
	public string[] Uris { get; set; }

	[JsonPropertyName("position_ms")]
	public uint PositionMs { get; set; }
}