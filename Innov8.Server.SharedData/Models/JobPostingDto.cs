#region

using Innov8.Server.SharedData.Enum;

#endregion

namespace Innov8.Server.SharedData.Models;

public record JobPostingDto
{
  public required Guid JobId { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Location { get; set; } = string.Empty;
  public ExperienceLevel ExperienceLevel { get; set; }
  public Guid PostedBy { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
