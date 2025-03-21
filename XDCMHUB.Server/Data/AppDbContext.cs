using Microsoft.EntityFrameworkCore;
using XDCMHUB.Server.Models;

namespace XDCMHUB.Server.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	public DbSet<User> Users { get; set; }
	public DbSet<Channel> Channels { get; set; }
	public DbSet<Message> Messages { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<User>()
			.HasIndex(u => u.Username)
			.IsUnique();

		modelBuilder.Entity<Channel>()
			.HasIndex(c => c.Name)
			.IsUnique();
	}
}

