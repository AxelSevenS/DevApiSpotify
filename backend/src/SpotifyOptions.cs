
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ProjectMana;

public record class SpotifyOptions
{
	public const string SpotifyOptionsLabel = "Spotify";

	public string Id { get; set; } = string.Empty;
	public string Secret { get; set; } = string.Empty;
}