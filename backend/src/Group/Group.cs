using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace ProjectMana;

[Table("groups")]
public record Group
{
	[Key] [Column("name")]
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[Required] [Column("leaderId")]
	[JsonIgnore]
	public uint LeaderId { get; set; } = 1;

	[ForeignKey(nameof(LeaderId))]
	[JsonPropertyName("leader")]
	public User Leader { get; set; } = null!;

	[JsonIgnore]
	public IList<User> Members { get; set; } = [];

	

	public PublicGroupInfo GetPublicGroupInfo() => new(this);
}

public record PublicGroupInfo
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("user_count")]
	public uint UserCount { get; set; } = 1;


	public PublicGroupInfo(Group info)
	{
		Name = info.Name;
		UserCount = (uint) info.Members.Count;
	}
}