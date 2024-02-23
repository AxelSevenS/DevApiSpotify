using Microsoft.EntityFrameworkCore;

namespace ProjectMana;

public class AppDbContext(DbContextOptions options) : DbContext(options)
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

		modelBuilder.Entity<Group>().HasOne(group => group.Leader)
			.WithOne();
	}
}