
using System.Text.Json.Serialization;

namespace ProjectMana;

public record Device
{
	[JsonPropertyName("id")]
	public string Id { get; set; }
	
    [JsonPropertyName("is_active")]
	public bool IsActive { get; set; }
	
    [JsonPropertyName("is_private_session")]
	public bool IsPrivateSession { get; set; }
	
    [JsonPropertyName("is_restricted")]
	public bool IsRestricted { get; set; }
	
    [JsonPropertyName("name")]
	public string Name { get; set; }

    [JsonPropertyName("type")]
	public string Type { get; set; }
	
    [JsonPropertyName("volume_percent")]
	public uint VolumePercent { get; set; }
	
    [JsonPropertyName("supports_volume")]
	public bool SupportsVolume { get; set; }
	
}
