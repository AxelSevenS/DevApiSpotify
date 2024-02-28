using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace YSpotify;

[ApiController]
[Route("api/spotify")]
public class SpotifyController(AppDbContext repository, SpotifyOptions spotifyOptions) : Controller
{
	private string RedirectUri => $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/spotify/login/callback";


	[HttpGet("login")]
	[Authorize]
	public async Task<ActionResult> Login()
	{
		if ( ! this.TryGetAuthenticatedUser(repository, out User user) )
		{
			return Unauthorized();
		}


		using HttpClient http = new();

		HttpResponseMessage response = await http.GetAsync(
			QueryHelpers.AddQueryString(
				"https://accounts.spotify.com/authorize", 
				new Dictionary<string, string?>
				{
					["response_type"] = "code",
					["client_id"] = spotifyOptions.Id,
					["scope"] = "user-read-playback-state user-modify-playback-state user-read-currently-playing playlist-modify-public playlist-modify-private user-top-read user-library-read",
					["redirect_uri"] = RedirectUri,
					["state"] = $"{ Convert.ToBase64String( Encoding.ASCII.GetBytes(user.Id.ToString()) ) }.{user.Password}",
					["show_dialog"] = "true",
				}
			)
		);

		if ( ! response.IsSuccessStatusCode || response.RequestMessage is null )
		{
			return BadRequest();
		}


		return Ok(response.RequestMessage.RequestUri);
	}


	[HttpGet("login/callback")]
	public async Task<ActionResult<string>> LoginCallback([FromQuery] string code, [FromQuery] string state)
	{
		string[] split = state.Split('.');
		uint userId;

		try {
			if (split.Length != 2) throw new ArgumentException(state);
			string? converted = Encoding.ASCII.GetString(Convert.FromBase64String(split[0]));
			ArgumentNullException.ThrowIfNullOrWhiteSpace(converted);
			userId = uint.Parse(converted);
		} catch {
			return BadRequest();
		}


		User? user = await repository.Users.FindAsync(userId);
		if (user is null)
		{
			return NotFound();
		}

		if (split[1] != user.Password)
		{
			return Unauthorized();
		}


		using HttpClient http = new();
		
		string authorization = Convert.ToBase64String( Encoding.UTF8.GetBytes($"{spotifyOptions.Id}:{spotifyOptions.Secret}") );
		http.DefaultRequestHeaders.Authorization = new("Basic", authorization);

		HttpResponseMessage response = await http.PostAsync(
			"https://accounts.spotify.com/api/token",
			new FormUrlEncodedContent( 
				new Dictionary<string, string?>
				{
					["code"] = code,
					["redirect_uri"] = RedirectUri,
					["grant_type"] = "authorization_code",
				}
			)
		);


		if ( ! response.IsSuccessStatusCode )
		{
			return BadRequest();
		}

		TokenResponse? tokenResult = JsonSerializer.Deserialize<TokenResponse>(await response.Content.ReadAsStringAsync());
		if ( tokenResult is null )
		{
			return BadRequest();
		}


		user.SpotifyAccessTokenExpiration = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + tokenResult.ExpiresIn;
		user.SpotifyAccessToken = tokenResult.AccessToken;
		user.SpotifyRefreshToken = tokenResult.RefreshToken;

		await repository.SaveChangesAsync();
		return Ok();
	}

	[HttpPost("topTracksPlaylist")]
	[Authorize]
	public async Task<ActionResult<string>> CreateTopTracksPlaylist([FromForm] uint userId)
	{
		if ( ! this.TryGetAuthenticatedUser(repository, out User user) )
		{
			return Unauthorized();
		}

		string? userAccessToken = await repository.GetAccessToken(user);
		if ( userAccessToken is null )
		{
			return NotFound("Your account is not linked to Spotify.");
		}

		User? targetUser = await repository.Users.FindAsync(userId);
		if ( targetUser is null )
		{
			return NotFound($"The given User was not found.");
		}

		string? targetUserAccessToken = await repository.GetAccessToken(targetUser);
		if ( targetUserAccessToken is null )
		{
			return NotFound($"The given User's Account is not linked to Spotify.");
		}

	
		using HttpClient http = new();
		http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", targetUserAccessToken);


		HttpResponseMessage profileResponse = await http.GetAsync(
			"https://api.spotify.com/v1/me"
		);
		if ( ! profileResponse.IsSuccessStatusCode )
		{
			return NotFound("Your Profile could not be loaded.");
		}

		UserProfile? userProfile = JsonSerializer.Deserialize<UserProfile>(await profileResponse.Content.ReadAsStringAsync());
		if (userProfile is null)
		{
			return NotFound("Your Profile could not be processed.");
		}
		
		HttpResponseMessage topTracksResponse = await http.GetAsync(
			QueryHelpers.AddQueryString(
				"https://api.spotify.com/v1/me/top/tracks", 
				new Dictionary<string, string?>
				{
					["time_range"] = "medium_term",
					["limit"] = "10",
					["offset"] = "0",
				}
			)
		);

		if ( ! topTracksResponse.IsSuccessStatusCode || topTracksResponse.RequestMessage is null )
		{
			return NotFound("The given Users's Top Tracks could not be loaded.");
		}

		TopTracksResponse? topTracks = JsonSerializer.Deserialize<TopTracksResponse>(await topTracksResponse.Content.ReadAsStringAsync());
		if ( topTracks is null )
		{
			return NotFound("The given Users's Top Tracks could not be processed.");
		}

		if ( topTracks.Total == 0 )
		{
			return NoContent();
		}

		http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);

		HttpResponseMessage createPlaylistResponse = await http.PostAsync(
			$"https://api.spotify.com/v1/users/{userProfile.Id}/playlists",
			JsonContent.Create( 
				new Dictionary<string, string?>
				{
					["name"] = $"{userProfile.DisplayName}'s Favorites",
					["public"] = "false",
					["description"] = "Enjoy your fellow group member's favorites as much as you want!",
				}
			)
		);

		if ( ! createPlaylistResponse.IsSuccessStatusCode || createPlaylistResponse.RequestMessage is null )
		{
			return NotFound("The playlist could not be created.");
		}

		CreatePlaylistResponse? createdPlaylist = JsonSerializer.Deserialize<CreatePlaylistResponse>(await createPlaylistResponse.Content.ReadAsStringAsync());
		if ( createdPlaylist is null )
		{
			return NotFound("The playlist could not be processed.");
		}

		HttpResponseMessage addTracksResponse = await http.PostAsync(
			$"https://api.spotify.com/v1/playlists/{createdPlaylist.Id}/tracks",
			JsonContent.Create(
				new {
					uris = topTracks.Items.Select(t => t.Uri)
				}
			)
		);

		if ( ! addTracksResponse.IsSuccessStatusCode || addTracksResponse.RequestMessage is null )
		{
			return NotFound("The playlist could not be populated.");
		}


		return AcceptedAtRoute(createdPlaylist.Href);
	}
}
