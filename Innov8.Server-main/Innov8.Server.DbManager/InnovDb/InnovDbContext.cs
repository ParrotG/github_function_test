#region

using Innov8.Server.SharedData.Models;
using Microsoft.EntityFrameworkCore;

#endregion

namespace Innov8.Server.DbManager.InnovDb;

public class InnovDbContext(
  DbContextOptions<InnovDbContext> options)
  : DbContext(options)
{
  public DbSet<JobPosting> JobPostings { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<JobPosting>()
      .Property(static j => j.ExperienceLevel)
      .HasConversion<string>();
  }
}
