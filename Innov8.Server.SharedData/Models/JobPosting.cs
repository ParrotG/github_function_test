#region

using System.ComponentModel.DataAnnotations;
using Innov8.Server.SharedData.Enum;
using Microsoft.EntityFrameworkCore;

#endregion

namespace Innov8.Server.SharedData.Models;

[PrimaryKey("JobId")]
public record JobPosting
{
  [Required] [Key] public required Guid JobId { get; set; }
  [MaxLength(100)] public string Title { get; set; } = string.Empty;
  [MaxLength(3000)] public string Description { get; set; } = string.Empty;
  [MaxLength(100)] public string Location { get; set; } = string.Empty;
  public ExperienceLevel ExperienceLevel { get; set; }
  public Guid PostedBy { get; set; }
  public DateTime CreatedAt { get; set; }

  public DateTime UpdatedAt { get; set; }

  // attempt secrets string, to demonstrate how DTO workings,
  // if the data has no secret filed, we do not need to use DTO
  [MaxLength(30)] public string Secrect { get; set; } = string.Empty;

  public JobPostingDto ToDto()
  {
    return new JobPostingDto
    {
      JobId = JobId,
      Title = Title,
      Description = Description,
      Location = Location,
      ExperienceLevel = ExperienceLevel,
      PostedBy = PostedBy,
      CreatedAt = CreatedAt,
      UpdatedAt = UpdatedAt
    };
  }
}
