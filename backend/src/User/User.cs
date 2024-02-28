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
	[JsonPropertyName("group_name")]
	public string? GroupName { get; set; } = null;

	/// <summary>
	/// Reference to the Group the User belongs to
	/// </summary>
	[ForeignKey(nameof(GroupName))]
	[JsonIgnore]
	public Group? Group { get; set; } = null;

	/// <summary>
	/// The expiration time of the Spotify Access token
	/// </summary>
	[Column("spotify_access_token_expiration")]
	[JsonIgnore]
	public long? SpotifyAccessTokenExpiration { get; set; } = null;

	/// <summary>
	/// Access token for the Spotify User the User is linked to or Null if the User hasn't linked their Spotify Account
	/// </summary>
	[Column("spotify_access_token")]
	[JsonIgnore]
	public string? SpotifyAccessToken { get; set; } = null;

	/// <summary>
	/// Refresh token of the Spotify User the User is linked to or Null if the User hasn't linked their Spotify Account
	/// </summary>
	[Column("spotify_refresh_token")]
	[JsonIgnore]
	public string? SpotifyRefreshToken { get; set; } = null;


	public record Personality
	{
		// 0.0 - 1.0
		[Column("likes_dance")]
		public float LikesDance { get; set; } = 0;

		[Column("tempo")]
		public float Tempo { get; set; } = 0;

		[Column("prefer_instrumental_over_vocal")]
		public bool PreferInstrumentalOverVocal { get; set; } = false;

		// 0.0 - 1.0
		[Column("valence")]
		public float Valence { get; set; } = 0;
	}
}

public record SpotifyUserInfo
{
	[JsonPropertyName("username")]
	public string Username { get; set; } = string.Empty;

	[JsonPropertyName("currentlyListeningTo")]
	public TrackSummary CurrentSong { get; set; }

	[JsonPropertyName("deviceName")]
	public string DeviceName { get; set; } = string.Empty;


	public SpotifyUserInfo() {}
	public SpotifyUserInfo(PlaybackState? playbackState, UserProfile userProfile)
	{
		if (playbackState is not null)
		{
			CurrentSong = new TrackSummary(playbackState.Track);
			DeviceName = playbackState.Device.Name;
		}
		Username = userProfile.DisplayName;
	}
}

public record GroupMemberInfo
{
	[JsonPropertyName("username")]
	public string Username { get; set; } = string.Empty;

	[JsonPropertyName("is_group_leader")]
	public bool IsGroupLeader { get; set; } = false;

	[JsonPropertyName("spotify_user")]
	public SpotifyUserInfo? SpotifyUser { get; set; } = null;
}

/// <summary>
/// Model representing a Spotify Song
/// </summary>
public record TrackSummary
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


	public TrackSummary(Track track)
	{
		AlbumName = track.Album.Name;
		ArtistName = string.Join(", ", track.Artists.Select(a => a.Name));
		Name = track.Name;
	}
}