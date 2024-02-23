using System.Collections.Specialized;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;


namespace ProjectMana;

[ApiController]
[Route("api/users")]
public class UserController(AppDbContext repo, JwtOptions jwtOptions, SpotifyOptions spotifyOptions) : Controller<User>(repo)
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
	/// Authenticate a user
	/// </summary>
	/// <param name="username">The username of the user</param>
	/// <param name="password">The password of the user</param>
	/// <returns>
	/// The JWT token of the user,
	///     or NotFound if the user does not exist
	/// </returns>
	[HttpPost("auth")]
	public async Task<ActionResult> AuthenticateUser([FromForm]string username, [FromForm]string password)
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
	[HttpPut]
	public ActionResult<User> RegisterUser([FromForm]string username, [FromForm]string password)
	{
		EntityEntry<User>? result = repository.Users.Add(
			new()
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
	/// Register a user
	/// </summary>
	/// <returns>
	/// The user,
	///    or BadRequest if the user already exists
	/// </returns>
	[HttpGet("spotify")]
	[Authorize]
	public async Task<ActionResult> LinkToSpotify()
	{
		if ( ! TryGetAuthenticatedUser(out User user) )
		{
			return Unauthorized();
		}


		using HttpClient http = new();
		string scope = "user-read-playback-state user-modify-playback-state user-read-currently-playing playlist-modify-public";

		NameValueCollection query = HttpUtility.ParseQueryString("https://accounts.spotify.com/authorize");
		query["response_type"] = "code";
		query["client_id"] = spotifyOptions.Id;
		query["scope"] = scope;
		query["redirect_uri"] = $"{HttpContext.Request.Host}/api/users";
		UriBuilder requestUri = new( query.ToString()! );


		HttpResponseMessage response = await http.GetAsync( requestUri.ToString() );
		Console.WriteLine(response);


		if ( ! response.IsSuccessStatusCode )
		{
			return Ok(response);
		}

		return Ok(response);
	}

	// /// <summary>
	// /// Update a user
	// /// </summary>
	// /// <param name="id">The id of the user</param>
	// /// <param name="user">The user to update</param>
	// /// <returns>
	// /// The updated user
	// /// </returns>
	// [HttpPatch("{id}")]
	// [Authorize]
	// public async Task<ActionResult<User>> UpdateUser(uint id, [FromForm] string? username, [FromForm] string? password)
	// {
	// 	if (username is null && password is null)
	// 	{
	// 		return BadRequest();
	// 	}

	// 	if ( ! VerifyOwnershipOrAuthZ(id, ProjectMana.User.Authorizations.EditAnyUser, out ActionResult<User> error))
	// 	{
	// 		return error;
	// 	}

	// 	User? user = await repository.Users.FindAsync(id);
	// 	if ( user is null )
	// 	{
	// 		return NotFound();
	// 	}

	// 	user.Username = username ?? user.Username;
	// 	user.Password = password is not null ? jwtOptions.HashPassword(password) : user.Password;

	// 	if ( roles is User.Authorizations auth && VerifyAuthorization(ProjectMana.User.Authorizations.EditUserAuths | auth) ) {
	// 		user.Auth = auth;
	// 	}

	// 	repository.SaveChanges();
	// 	return Ok(user);
	// }

	/// <summary>
	/// Delete a user
	/// </summary>
	/// <param name="id">The id of the user</param>
	/// <returns>
	/// The deleted user
	/// </returns>
	[HttpDelete("{id}")]
	[Authorize]
	public async Task<ActionResult<User>> DeleteUser(uint id)
	{
		if ( ! VerifyAuthenticatedId(id) )
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