using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace YSpotify;

public class AppDbContext(DbContextOptions options, SpotifyOptions spotifyOptions) : DbContext(options)
{
	public DbSet<User> Users { get; set; }
	public DbSet<Group> Groups { get; set; }


	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Initialization logic here
		// modelBuilder.Entity<Group>().HasMany(group => group.Members)
		// 	.WithOne(user => user.Group);

		// modelBuilder.Entity<User>().HasOne(user => user.Group)
		// 	.WithMany(group => group.Members);

		modelBuilder.Entity<Group>()
			.HasOne(group => group.Leader)
			.WithOne();

        modelBuilder.Entity<Group>()
            .HasMany(g => g.Members)
            .WithOne(u => u.Group)
            .HasForeignKey(u => u.GroupName);
	}

	
	public async Task<string?> GetAccessToken(User user)
	{
		long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		if (
			user.SpotifyAccessToken is not null &&
			user.SpotifyAccessTokenExpiration is not null &&
			user.SpotifyAccessTokenExpiration > currentTime
		)
		{
			return user.SpotifyAccessToken;
		}

		if (user.SpotifyRefreshToken is null)
		{
			return null;
		}


		using HttpClient http = new();
		
		string authorization = Convert.ToBase64String( Encoding.UTF8.GetBytes($"{spotifyOptions.Id}:{spotifyOptions.Secret}") );
		http.DefaultRequestHeaders.Authorization = new("Basic", authorization);

		HttpResponseMessage response = await http.PostAsync(
			"https://accounts.spotify.com/api/token", 
			new FormUrlEncodedContent(
				new Dictionary<string, string?>
				{
					["client_id"] = spotifyOptions.Id,
					["refresh_token"] = user.SpotifyRefreshToken,
					["grant_type"] = "refresh_token",
				}
			)
		);


		Console.WriteLine(response.StatusCode);
		Console.WriteLine(await response.Content.ReadAsStringAsync());
		if ( ! response.IsSuccessStatusCode )
		{
			return null;
		}

		TokenResponse? tokenResult = JsonSerializer.Deserialize<TokenResponse>(await response.Content.ReadAsStringAsync());
		if ( tokenResult is null )
		{
			return null;
		}


		user.SpotifyAccessTokenExpiration = currentTime + tokenResult.ExpiresIn;
		user.SpotifyAccessToken = tokenResult.AccessToken;

		await SaveChangesAsync();
		return user.SpotifyAccessToken;
	}

	public async Task<SpotifyUserInfo?> GetSpotifyUserInfo(User user)
	{
		string? accessToken = await GetAccessToken(user);
		if (accessToken is null)
		{
			return null;
		}


		using HttpClient http = new();
		http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


		HttpResponseMessage profileResponse = await http.GetAsync(
			"https://api.spotify.com/v1/me"
		);
		if ( ! profileResponse.IsSuccessStatusCode )
		{
			return null;
		}

		UserProfile? profileResult = JsonSerializer.Deserialize<UserProfile>(await profileResponse.Content.ReadAsStringAsync());
		if (profileResult is null)
		{
			return null;
		}

		
		HttpResponseMessage playbackResponse = await http.GetAsync(
			"https://api.spotify.com/v1/me/player"
		);

		PlaybackState? playbackResult = playbackResponse.StatusCode == System.Net.HttpStatusCode.OK
			? JsonSerializer.Deserialize<PlaybackState>(await playbackResponse.Content.ReadAsStringAsync()) 
			: null;



		return new SpotifyUserInfo(playbackResult, profileResult);
	}

	public async Task<GroupMemberInfo> GetGroupMemberInfo(Group group, User user)
	{
		if ( ! group.Members.Contains(user) || user.Group != group )
		{
			throw new ArgumentException("User does not belong to Group");
		}

		return new GroupMemberInfo{
			Username = user.Username,
			IsGroupLeader = group.Leader == user,
			SpotifyUser = await GetSpotifyUserInfo(user),
		};
	}
}