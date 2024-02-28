using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;


namespace ProjectMana;

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
	/// <responses>
	/// "200": 
	/// Ok
	/// </responses>
	[HttpGet]
	[Authorize]
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
	/// Get the group where the user is in
	/// </summary>
	/// <description> 
	/// If the user is not in a group, return nothing 
	/// </description>
	/// <returns>
	/// 
	/// </returns>
	[HttpGet("current")]
	[Authorize]
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
	/// <param name="groupName"></param>
	/// <returns> GetPublicGroupInfo (name, id, etc..)</returns>
	[HttpPost("join/{groupName}")]
	[Authorize]
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
		}

		user.Group = group;
		group.Members.Add(user);

		await repository.SaveChangesAsync();
		return Ok( group.GetPublicGroupInfo() );
	}

	/// <summary>
	/// Leave a group
	/// </summary>
	/// <returns>
	/// The group the user left
	/// </returns>
	[HttpPost("leave")]
	[Authorize]
	public async Task<ActionResult<PublicGroupInfo>> LeaveGroup()
	{
		if ( ! this.TryGetAuthenticatedUser(repository, out User user) )
		{
			return Unauthorized();
		}

		if ( user.Group is null ) {
			return NotFound();
		}

		PublicGroupInfo group = user.Group.GetPublicGroupInfo();

		RemoveUserFromGroup(user.Group, user);

		await repository.SaveChangesAsync();
		return Ok( group );
	}
	
	[HttpPost("synchronize")]
	[Authorize]
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