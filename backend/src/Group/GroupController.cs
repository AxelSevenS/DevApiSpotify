using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace YSpotify;

[ApiController]
[Route("api/groups")]
public class GroupController(AppDbContext repository) : Controller
{
	/// <summary>
	/// Get all groups
	/// </summary>
	/// <description>
	/// All groups name in the database
	/// </description>
	/// <response code="200">Returns the public Info of the Group</response>
	/// <response code="204">If there are no Groups</response>
	[HttpGet]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<ActionResult<List<PublicGroupInfo>>> GetAll() =>
		await repository.Groups
			.Include(g => g.Members)
			.Select(g => new PublicGroupInfo(g))
			.ToListAsync() switch
			{
				[] => NoContent(),
				[.. IEnumerable<PublicGroupInfo> many] => Ok(many)
			};

	/// <summary>
	/// Get the group the authenticated User belongs to.
	/// </summary>
	/// <response code="200">Returns the Group Members List</response>
	/// <response code="401">If the User is not Authenticated</response>
	/// <response code="403">If the User is not in a Group</response>
	[HttpGet("current")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<List<GroupMemberInfo>>> GetGroupMembers()
	{
		if ( ! this.TryGetAuthenticatedUser(repository, out User user) )
		{
			return Unauthorized();
		}

		if ( user.Group is null )
		{
			return Forbid();
		}

		return Ok(
			await Task.WhenAll(
				user.Group.Members
					.Select( async member => await repository.GetGroupMemberInfo(user.Group, member) )
			)
		);
	}
	
	/// <summary>
	/// Join a group by the group name
	/// </summary>
	/// <param name="groupName">The Identifying name of the Group</param>
	/// <response code="200">Returns public Info of the Group the User joined</response>
	/// <response code="401">If the User is not Authenticated</response>
	[HttpPost("join/{groupName}")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<PublicGroupInfo>> JoinGroup(string groupName)
	{
		if ( ! this.TryGetAuthenticatedUser(repository, out User user) )
		{
			return Unauthorized();
		}


		if ( user.Group is not null )
		{
			RemoveUserFromGroup(user.Group, user);
		}

		Group? group = await repository.Groups
			.Where(g => g.Name == groupName)
			.Include(g => g.Members)
			.FirstOrDefaultAsync();

		if (group is null) {
			group = new Group { 
				Name = groupName,
				Leader = user,
			};

			await repository.Groups.AddAsync(group);
			await repository.SaveChangesAsync();
		}

		user.Group = group;
		group.Members.Add(user);

		await repository.SaveChangesAsync();
		return Ok( group.GetPublicGroupInfo() );
	}

	/// <summary>
	/// Leave a group
	/// </summary>
	/// <response code="200">Returns public Info of the Group the User left</response>
	/// <response code="401">If the User is not Authenticated</response>
	/// <response code="403">If the User is not in a Group</response>
	[HttpPost("leave")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<PublicGroupInfo>> LeaveGroup()
	{
		if ( ! this.TryGetAuthenticatedUser(repository, out User user) )
		{
			return Unauthorized();
		}

		if ( user.Group is null ) {
			return Forbid();
		}

		PublicGroupInfo group = user.Group.GetPublicGroupInfo();

		RemoveUserFromGroup(user.Group, user);

		await repository.SaveChangesAsync();
		return Ok( group );
	}
	
	/// <summary>
	/// As the Group Leader, Synchronize all the Group's Members' Players to the current
	/// Playback state of the Group Leader.
	/// </summary>
	/// <response code="200">Returns public Info of the Group the User left</response>
	/// <response code="401">If the User is not Authenticated</response>
	/// <response code="403">If the User is not the Group Leader</response>
	/// <response code="404">If A resource necessary to Synchronize could not be found</response>
	[HttpPost("synchronize")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<PublicGroupInfo>> SynchronizePlayer()
	{
		if ( ! this.TryGetAuthenticatedUser(repository, out User user) )
		{
			return Unauthorized();
		}

		if ( user.Group is null ) {
			return NotFound();
		}

		if ( user.Group.Leader != user)
		{
			return Forbid("You are not the Group Leader.");
		}

		string? accessToken = await repository.GetAccessToken(user);
		if ( accessToken is null )
		{
			return NotFound("Your account is not linked to Spotify.");
		}

		
		using HttpClient http = new();
		http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
		
		HttpResponseMessage stateResponse = await http.GetAsync(
			"https://api.spotify.com/v1/me/player"
		);

		if (stateResponse.StatusCode != System.Net.HttpStatusCode.OK)
		{
			return NotFound("Your account has no currently active Playback.");
		}

		PlaybackState? state = JsonSerializer.Deserialize<PlaybackState>(await stateResponse.Content.ReadAsStringAsync());
		if ( state is null )
		{
			return NotFound("Your currently active Playback could not be processed.");
		}

		JsonContent instruction = JsonContent.Create( 
			new {
				context_uri = state.Context?.Uri,
				position_ms = state.ProgressMs,
				uris = new
				{
					state.Track.Uri
				}
			}
		);

		foreach (User member in user.Group.Members)
		{
			string? memberAccessToken = await repository.GetAccessToken(member);
			if ( memberAccessToken is null )
			{
				continue;
			}

			http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberAccessToken);
			await http.PutAsync(
				"https://api.spotify.com/v1/me/player",
				instruction
			);
		}


		return Ok();
	}

	private void RemoveUserFromGroup(Group group, User user)
	{
		group.Members.Remove(user);

		// If the group is empty, remove it.
		if (group.Members.Count == 0)
		{
			repository.Groups.Remove(group);
			return;
		}

		// Choose random member to become the new Leader
		if (group.Leader == user)
		{
			group.Leader = group.Members.ElementAt( Random.Shared.Next(group.Members.Count) );
		}

		user.Group = null;
	}
}