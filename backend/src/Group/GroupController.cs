using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;


namespace ProjectMana;

[ApiController]
[Route("api/groups")]
public class GroupController(AppDbContext repo) : Controller<Group>(repo)
{
	/// <summary>
	/// Get all groups
	/// </summary>
	/// <returns>
	/// All groups
	/// </returns>
	[HttpGet]
	[Authorize]
	public async Task<List<PublicGroupInfo>> GetAll() =>
		await repository.Groups.Include(g => g.Members)
			.Select(g => new PublicGroupInfo(g))
			.ToListAsync();

	/// <summary>
	/// Get a group by id
	/// </summary>
	/// <returns>
	/// The group with the given id
	/// </returns>
	[HttpGet("current")]
	[Authorize]
	public async Task<ActionResult<List<GroupMemberInfo>>> GetGroupMembers()
	{
		return await Task.Run(Process); 

		ActionResult<List<GroupMemberInfo>> Process()
		{
			if ( ! TryGetAuthenticatedUser(out User user) )
			{
				return Unauthorized();
			}

			if ( user.Group is null )
			{
				return Forbid("You do not belong to a Group");
			}

			return Ok(
				user.Group.Members
					.Select( m => m.GetGroupMemberInfo(user.Group) )
			);
		}
	}
	
	
	/// <summary>
	/// Add a song to a playlist
	/// </summary>
	/// <param name="groupName"></param>
	/// <returns></returns>
	[HttpPost("join/{groupName}")]
	[Authorize]
	public async Task<ActionResult<PublicGroupInfo>> JoinGroup(string groupName)
	{
		if ( ! TryGetAuthenticatedUser(out User user) )
		{
			return Unauthorized();
		}


		if ( user.Group is not null )
		{
			RemoveUserFromGroup(user.Group, user);
		}

		Group? group = await repository.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Name == groupName);
		if (group is null) {
			group = new Group{ 
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
	/// Register a song
	/// </summary>
	/// <returns>
	/// The song,
	///    or BadRequest if the song already exists
	/// </returns>
	[HttpPost("leave")]
	[Authorize]
	public async Task<ActionResult<PublicGroupInfo>> LeaveGroup()
	{
		if ( ! TryGetAuthenticatedUser(out User user) )
		{
			return Unauthorized();
		}

		if ( user.Group is null ) {
			return NotFound();
		}

		PublicGroupInfo res = user.Group.GetPublicGroupInfo();

		RemoveUserFromGroup(user.Group, user);

		await repository.SaveChangesAsync();
		return Ok(res);
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
			Random random = new();
			group.Leader = group.Members.ElementAt( random.Next(group.Members.Count) );
		}

		user.Group = null;
	}
}