using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
	public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

	public DbSet<Role> Roles => Set<Role>();
	public DbSet<User> Users => Set<User>();
	public DbSet<Event> Events => Set<Event>();
	public DbSet<Category> Categories => Set<Category>();
	public DbSet<EventEnrolment> EventEnrolments => Set<EventEnrolment>();
	public DbSet<Result> Results => Set<Result>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Role>().HasIndex(r => r.RoleName).IsUnique();
		modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

		modelBuilder.Entity<Event>()
			.HasOne(e => e.Organiser).WithMany()
			.HasForeignKey(e => e.OrganiserID).OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<EventEnrolment>()
			.HasOne(e => e.Participant).WithMany()
			.HasForeignKey(e => e.ParticipantID).OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<EventEnrolment>()
			.HasIndex(e => new { e.ParticipantID, e.CategoryID }).IsUnique();

		modelBuilder.Entity<Result>()
			.HasOne(r => r.Enrolment).WithOne(e => e.Result)
			.HasForeignKey<Result>(r => r.EnrolmentID);
		modelBuilder.Entity<Result>().HasIndex(r => r.EnrolmentID).IsUnique();
	}
}