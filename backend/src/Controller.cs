using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;

// using UserAuth = ProjectMana.User.Authorizations;

namespace ProjectMana;

public abstract class Controller<TData>(AppDbContext repository) : ControllerBase where TData : class
{
	protected readonly AppDbContext repository = repository;

	// /// <summary>
	// /// Check wether the user is authenticated and if the user holds the given <c>authorizations</c>
	// /// </summary>
	// /// <param name="authorizations"></param>
	// /// <returns>True if the user is authenticated and holds the authorization(s), False if the user is not authenticated or doesn't hold the authorization(s)</returns>
	// protected bool VerifyAuthorization(UserAuth authorizations)
	// {
	// 	if ( HttpContext.User.FindFirst(JwtOptions.RoleClaim)?.Value is string claim )
	// 	{
	// 		return (claim.GetAuths() & authorizations) == authorizations;
	// 	}

	// 	return false;
	// }

	/// <summary>
	/// Verifies wether the user is authenticated and if it is, set <c>id</c> to the authenticated user's Id.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	protected bool TryGetAuthenticatedUserId(out uint id)
	{
		id = 0;
		if (
			HttpContext.User.FindFirst(JwtRegisteredClaimNames.Sub) is Claim claim &&
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
	/// <param name="user"></param>
	/// <returns></returns>
	protected bool TryGetAuthenticatedUser(out User user)
	{
		user = null!;
		if ( ! TryGetAuthenticatedUserId(out uint userId) )
		{
			return false;
		}

		user = repository.Users.FirstOrDefault(u => u.Id == userId)!;
		return user is not null;
	}

	/// <summary>
	/// Verify if the current authenticated user exists and has the given Id as its own
	/// </summary>
	/// <param name="validId"></param>
	/// <returns>True if the Authenticated user exists and has the given Id, False if the user is not authenticated or doesn't fit the given Id</returns>
	protected bool VerifyAuthenticatedId(uint validId) =>
		TryGetAuthenticatedUserId(out uint authenticatedId) && authenticatedId == validId;

	/// <summary>
	/// Verifies wether the current authenticated user has the given authorizations, if not <c>result</c> will contain a StatusCodeResult corresponding to the error.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="neededAuthorizations">The authorizations the authenticated user needs to posess to validate the check</param>
	/// <param name="authId">The Id of the currently authenticated user, if the function returns True</param>
	/// <param name="result">The StatusCodeResult corresponding to the error, if the function returns False</param>
	/// <returns>
	/// True if the user is authenticated and posesses the given <c>authorizations</c>, otherwise False.
	/// </returns>
	// protected bool VerifyAuthZ<T>(UserAuth neededAuthorizations, out uint authId, out ActionResult<T> result) where T : class
	// {
	// 	if ( ! TryGetAuthenticatedUserId(out authId) )
	// 	{
	// 		result = Unauthorized();
	// 		return false;
	// 	}
	// 	if ( ! VerifyAuthorization(neededAuthorizations) )
	// 	{
	// 		result = Forbid();
	// 		return false;
	// 	}

	// 	result = null!;
	// 	return true;
	// }

	// /// <summary>
	// /// Verifies wether the current authenticated user has the given authorizations, if not <c>result</c> will contain a StatusCodeResult corresponding to the error.
	// /// </summary>
	// /// <typeparam name="T"></typeparam>
	// /// <param name="authId">The Id that the authenticated user needs to have to be deemed "Owner" of the resource</param>
	// /// <param name="neededAuthorizations">The authorizations the authenticated user needs to posess to validate the check and override the Ownership check</param>
	// /// <param name="result">The StatusCodeResult corresponding to the error if the verification was unsuccessful</param>
	// /// <returns>
	// /// True if the user is authenticated and posesses the given <c>authorizations</c> OR has <c>authId</c> as their Id, otherwise False.
	// /// </returns>
	// protected bool VerifyOwnershipOrAuthZ<T>(uint authId, UserAuth neededAuthorizations, out ActionResult<T> result) where T : class
	// {
	// 	if ( ! TryGetAuthenticatedUserId(out uint currentId) )
	// 	{
	// 		result = Unauthorized();
	// 		return false;
	// 	}
	// 	if ( authId != currentId && ! VerifyAuthorization(neededAuthorizations) )
	// 	{
	// 		result = Forbid();
	// 		return false;
	// 	}

	// 	result = null!;
	// 	return true;
	// }
}