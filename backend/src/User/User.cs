using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.EntityFrameworkCore;

namespace ProjectMana;

/// <summary>
/// Model of a User of the Service, identified by unsigned integer Id,
/// Contains login credentials as well as a reference to the Group the user belongs to.
/// The Model contains the spotify user Id, if the User is linked to a Spotify User.
/// </summary>
[Table("users")]
[Index(nameof(Username), IsUnique = true)]
public record User
{
	/// <summary>
	/// Identication of the User
	/// </summary>
	[Key] [Column("id")]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	[JsonPropertyName("id")]
	public uint Id { get; set; }

	/// <summary>
	/// Unique Username of the User
	/// </summary>
	[Required] [Column("username")]
	[JsonPropertyName("username")]
	public string Username { get; set; } = string.Empty;

	/// <summary>
	/// Hashed Password of the User
	/// </summary>
	[Required] [Column("password")]
	[JsonPropertyName("password")]
	public string Password { get; set; } = string.Empty;

	/// <summary>
	/// Secondary Key of the Group the User belongs to
	/// </summary>
	[Column("group_name")]
	[JsonPropertyName("groupName")]
	public string? GroupName { get; set; } = null;

	/// <summary>
	/// Reference to the Group the User belongs to
	/// </summary>
	[ForeignKey(nameof(GroupName))]
	[JsonIgnore]
	public Group? Group { get; set; } = null;

	/// <summary>
	/// Id of the Spotify User the User is linked to or Null if the User hasn't linked their Spotify Account
	/// </summary>
	[Column("spotify_user_id")]
	[JsonIgnore]
	public string? SpotifyUserCode { get; set; } = null;



	public async Task<SpotifyUserInfo?> GetSpotifyUserInfo()
	{
		if (SpotifyUserCode is null)
		{
			return null;
		}
		// using HttpClient http = new();

		// NameValueCollection query = HttpUtility.ParseQueryString("https://accounts.spotify.com/authorize");
		// UriBuilder requestUri = new(query.ToString() ?? "");

		// HttpResponseMessage qdqs = await http.GetAsync(
		// 	requestUri.ToString()
		// );

		return new SpotifyUserInfo{
			CurrentSong = new SpotifySong{
				AlbumName = "AlbumName",
				ArtistName = "ArtistName",
				Name = "SongName",
			},
			DeviceName = "DeviceName",
			Username = "Username",
		};
	}

	public async Task<GroupMemberInfo> GetGroupMemberInfo(Group group)
	{
		if ( ! group.Members.Contains(this) || Group != group )
		{
			throw new ArgumentException("User does not belong to Group");
		}

		return new GroupMemberInfo{
			Username = Username,
			IsGroupLeader = group.Leader == this,
			SpotifyUser = await GetSpotifyUserInfo(),
		};
	}
}

public record SpotifyUserInfo
{
	[JsonPropertyName("username")]
	public string Username { get; set; } = string.Empty;

	[JsonPropertyName("currentlyListeningTo")]
	public SpotifySong CurrentSong { get; set; }

	[JsonPropertyName("deviceName")]
	public string DeviceName { get; set; } = string.Empty;
}

public record GroupMemberInfo
{
	[JsonPropertyName("username")]
	public string Username { get; set; } = string.Empty;

	[JsonPropertyName("isGroupLeader")]
	public bool IsGroupLeader { get; set; } = false;

	[JsonPropertyName("spotifyUser")]
	public SpotifyUserInfo? SpotifyUser { get; set; } = null;
}

/// <summary>
/// Model representing a Spotify Song
/// </summary>
public record SpotifySong
{
	/// <summary>
	/// The Name of the Song
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The Name of the Artist
	/// </summary>
	[JsonPropertyName("artistName")]
	public string ArtistName { get; set; } = string.Empty;

	/// <summary>
	/// The Name of the Album the Song belongs to
	/// </summary>
	[JsonPropertyName("albumName")]
	public string AlbumName { get; set; } = string.Empty;
}