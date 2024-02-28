using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;


namespace ProjectMana;

[ApiController]
[Route("api/users")]
public class UserController(AppDbContext repository, JwtOptions jwtOptions) : Controller
{
	/// <summary>
	/// Get all users
	/// </summary>
	/// <returns>
	/// All users
	/// </returns>
	[HttpGet]
	[Authorize]
	public async Task<List<User>> GetAll() =>
		await repository.Users.ToListAsync();

	/// <summary>
	/// Get a user by id
	/// </summary>
	/// <param name="id">The id of the user</param>
	/// <returns>
	/// The user with the given id
	/// </returns>
	[HttpGet("{id}")]
	[Authorize]
	public async Task<ActionResult<User>> GetById(uint id) =>
		await repository.Users.FindAsync(id) switch
		{
			User user => Ok(user),
			null => NotFound(),
		};

	/// <summary>
	/// Get a user's "personality" by id
	/// </summary>
	/// <param name="id">The id of the user</param>
	/// <returns>
	/// The Personality of the user with the given id
	/// </returns>
	[HttpGet("{id}/personality")]
	[Authorize]
	public async Task<ActionResult<User.Personality>> GetPersonalityById(uint id)
	{
		User? user = await repository.Users.FindAsync(id);
		if (user is null)
		{
			return NotFound("The given User was not found.");
		}

		string? accessToken = await repository.GetAccessToken(user);
		if (accessToken is null)
		{
			return NotFound("The given User's Account is not linked to Spotify.");
		}

		using HttpClient http = new();
		http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

		HttpResponseMessage tracksResponse = await http.GetAsync( 
			QueryHelpers.AddQueryString(
				"https://api.spotify.com/v1/me/tracks", 
				new Dictionary<string, string?>
				{
					["offset"] = "0",
					["limit"] = "10",
				}
			)
		);
		
		if ( ! tracksResponse.IsSuccessStatusCode || tracksResponse.Content is null )
		{
			return BadRequest();
		}

		SavedTrackResponse? tracksResult = JsonSerializer.Deserialize<SavedTrackResponse>(await tracksResponse.Content.ReadAsStringAsync());
		if (tracksResult is null)
		{
			return BadRequest();
		}

		IEnumerable<Track> tracks = tracksResult.Items
			.Select(i => i.Track);


		HttpResponseMessage featuresResponse = await http.GetAsync(
			QueryHelpers.AddQueryString(
				"https://api.spotify.com/v1/audio-features",
				new Dictionary<string, string?>
				{
					["ids"] = string.Join(',', tracks.Select(t => t.Id)),
				}
			)
		);
		if ( ! featuresResponse.IsSuccessStatusCode || featuresResponse.Content is null )
		{
			return BadRequest();
		}

		AudioFeatureResponse? featuresResult = JsonSerializer.Deserialize<AudioFeatureResponse>(await featuresResponse.Content.ReadAsStringAsync());
		if (featuresResult is null)
		{
			return BadRequest();
		}

		if (featuresResult.AudioFeatures.Length == 0)
		{
			return NoContent();
		}


		return Ok(
			new User.Personality
			{
				LikesDance = featuresResult.AudioFeatures.Select(f => f.Danceability).Average() * 10f,
				Tempo = featuresResult.AudioFeatures.Select(f => f.Tempo).Average(),
				Valence = featuresResult.AudioFeatures.Select(f => f.Valence).Average(),
				PreferInstrumentalOverVocal = featuresResult.AudioFeatures.Select(f => f.Instrumentalness).Average() > featuresResult.AudioFeatures.Select(f => f.Speechiness).Average()
			}
		);
	}
	
	/// <summary>
	/// Authenticate a user
	/// </summary>
	/// <param name="username">The username of the user</param>
	/// <param name="password">The password of the user</param>
	/// <returns>
	/// The JWT token of the user,
	///    or NotFound if the user does not exist
	/// </returns>
	[HttpPost("login")]
	public async Task<ActionResult> Login([FromForm]string username, [FromForm]string password)
	{
		password = jwtOptions.HashPassword(password);
		return await repository.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password) switch
		{
			User user => Ok( JsonSerializer.Serialize(jwtOptions.GenerateFrom(user).Write()) ),
			null => NotFound(),
		};
	}

	/// <summary>
	/// Register a user
	/// </summary>
	/// <param name="username">The username of the user</param>
	/// <param name="password">The password of the user</param>
	/// <returns>
	/// The user,
	///    or BadRequest if the user already exists
	/// </returns>
	[HttpPut("register")]
	public async Task<ActionResult<User>> Register([FromForm]string username, [FromForm]string password)
	{
		User? existing = await repository.Users
			.Where(u => u.Username == username)
			.FirstOrDefaultAsync();
		if (existing is not null)
		{
			return BadRequest("Username already taken.");
		}

		EntityEntry<User>? result = repository.Users.Add(
			new User
			{
				Username = username,
				Password = jwtOptions.HashPassword(password)
			}
		);

		if (result.Entity is not User user)
		{
			return BadRequest();
		}

		repository.SaveChanges();
		return Ok(user);
	}

	/// <summary>
	/// Delete a user
	/// </summary>
	/// <param name="id">The id of the user</param>
	/// <returns>
	/// The deleted user
	/// </returns>
	[HttpDelete("{id}")]
	[Authorize]
	public async Task<ActionResult<User>> Delete(uint id)
	{
		if ( ! this.VerifyAuthenticatedId(id) )
		{
			return Forbid();
		}

		User? current = await repository.Users.FindAsync(id);
		if ( current is null )
		{
			return NotFound();
		}

		EntityEntry<User> deleted = repository.Users.Remove(current);

		repository.SaveChanges();
		return Ok(deleted.Entity);
	}
}