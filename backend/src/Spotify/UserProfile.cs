
using System.Text.Json.Serialization;

namespace ProjectMana;

public record class UserProfile
{
	[JsonPropertyName("country")]
	public string Country { get; set; }

	[JsonPropertyName("display_name")]
	public string DisplayName { get; set; }

	[JsonPropertyName("email")]
	public string Email { get; set; }

	[JsonPropertyName("href")]
	public string Href { get; set; }

	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("product")]
	public string Product { get; set; }

	[JsonPropertyName("type")]
	public string Type { get; set; }

	[JsonPropertyName("uri")]
	public string Uri { get; set; }
}
