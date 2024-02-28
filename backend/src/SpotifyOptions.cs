
namespace ProjectMana;

public record class SpotifyOptions
{
	public const string SpotifyOptionsLabel = "Spotify";

	public string Id { get; set; } = string.Empty;
	public string Secret { get; set; } = string.Empty;
}