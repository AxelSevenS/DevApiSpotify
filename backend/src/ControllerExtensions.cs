using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// using UserAuth = ProjectMana.User.Authorizations;

namespace YSpotify;

public static class ControllerExtensions
{
	/// <summary>
	/// Verifies wether the user is authenticated and if it is, set <c>id</c> to the authenticated user's Id.
	/// </summary>
	/// <param name="controller"></param>
	/// <param name="id"></param>
	/// <returns></returns>
	public static bool TryGetAuthenticatedUserId(this Controller controller, out uint id)
	{
		id = 0;
		if (
			controller.HttpContext.User.FindFirst(JwtRegisteredClaimNames.Sub) is Claim claim &&
			uint.TryParse(Encoding.UTF8.GetBytes(claim.Value), out id)
		)
		{
			return id != 0;
		}

		return false;
	}

	/// <summary>
	/// Verifies wether the user is authenticated and if it is, set <c>id</c> to the authenticated user's Id.
	/// </summary>
	/// <param name="controller"></param>
	/// <param name="context"></param>
	/// <param name="user"></param>
	/// <returns></returns>
	public static bool TryGetAuthenticatedUser(this Controller controller, AppDbContext context, out User user)
	{
		user = null!;
		if ( ! controller.TryGetAuthenticatedUserId(out uint userId) )
		{
			return false;
		}

		user = context.Users
			.Include(u => u.Group)
			.FirstOrDefault(u => u.Id == userId)!;
			
		return user is not null;
	}

	/// <summary>
	/// Verify if the current authenticated user exists and has the given Id as its own
	/// </summary>
	/// <param name="controller"></param>
	/// <param name="validId"></param>
	/// <returns>True if the Authenticated user exists and has the given Id, False if the user is not authenticated or doesn't fit the given Id</returns>
	public static bool VerifyAuthenticatedId(this Controller controller, uint validId) =>
		controller.TryGetAuthenticatedUserId(out uint authenticatedId) && authenticatedId == validId;
}