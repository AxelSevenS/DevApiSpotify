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


namespace YSpotify;

[ApiController]
[Route("api/users")]
public class UserController(AppDbContext repository, JwtOptions jwtOptions) : Controller
{
	/// <summary>
	/// Get all users
	/// </summary>
	/// <response code="200">Returns all Users</response>
	/// <response code="204">If there are no Users</response>
	[HttpGet]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<ActionResult<IEnumerable<User>>> GetAll() =>
		await repository.Users
			.ToListAsync() switch
		{
			[] => NoContent(),
			[.. IEnumerable<User> many] => Ok(many)
		};

	/// <summary>
	/// Get a user by id
	/// </summary>
	/// <param name="id">The Id of the user</param>
	/// <response code="200">Returns the designated User</response>
	/// <response code="404">If the User could not be found</response>
	[HttpGet("{id}")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<User>> GetById(uint id) =>
		await repository.Users.FindAsync(id) switch
		{
			null => NotFound(),
			User user => Ok(user),
		};

	/// <summary>
	/// Delete a user
	/// </summary>
	/// <param name="id">The Id of the user</param>
	/// <response code="200">Returns the Deleted User</response>
	/// <response code="403">If the User is not authorized to delete the Account (if the User is another one)</response>
	/// <response code="404">If the User could not be found</response>
	[HttpDelete("{id}")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
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
	
	/// <summary>
	/// Authenticate as a user
	/// </summary>
	/// <param name="username">The Username of the user</param>
	/// <param name="password">The Password of the user</param>
	/// <response code="200">Returns the JWT for authentication with this Account</response>
	/// <response code="400">If the User could not be identified with the given Credentials</response>
	[HttpPost("login")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<string>> Login([FromForm]string username, [FromForm]string password)
	{
		password = jwtOptions.HashPassword(password);
		return await repository.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password) switch
		{
			User user => Ok( JsonSerializer.Serialize(jwtOptions.GenerateFrom(user).Write()) ),
			null => NotFound(),
		};
	}

	/// <summary>
	/// Register a user with a Given Password and unique Username
	/// </summary>
	/// <param name="username">The unique Username of the user</param>
	/// <param name="password">The Password of the user</param>
	/// <response code="201">The User was Created</response>
	/// <response code="400">If the Username is already taken or the Registration otherwise failed</response>
	[HttpPut("register")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<User>> Register([FromForm] string username, [FromForm] string password)
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
		return Created($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/users/{user.Id}", user);
	}

	/// <summary>
	/// Get a user's "personality" by id
	/// </summary>
	/// <param name="id">The Id of the user</param>
	/// <response code="200">The User was Created</response>
	/// <response code="400">If the Username is already taken or the Registration otherwise failed</response>
	[HttpGet("{id}/personality")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
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
}