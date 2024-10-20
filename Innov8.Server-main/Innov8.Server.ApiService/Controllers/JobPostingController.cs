#region

using Asp.Versioning;
using Innov8.Server.DbManager.InnovDb;
using Innov8.Server.SharedData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

#endregion

namespace Innov8.Server.ApiService.Controllers;

[ApiVersion("1.0")]
[ApiVersion("2.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class JobPostingController(InnovDbContext context)
  : ApiControllerBase(context)
{
  // GET: api/v{version}/JobPosting
  [HttpGet]
  public async Task<ActionResult<IEnumerable<JobPostingDto>>> GetJobPostings()
  {
    return await Context.JobPostings.Select(static s => s.ToDto())
      .ToListAsync();
  }

  // GET: api/v{version}/JobPosting/5
  [HttpGet("{id:guid}")]
  public async Task<ActionResult<JobPostingDto>> GetJobPosting(Guid id)
  {
    var jobPosting = await Context.JobPostings.FindAsync(id);

    if (jobPosting == null)
    {
      return NotFound();
    }

    return jobPosting.ToDto();
  }

  // PUT: api/v{version}/JobPosting/5
  // To protect from over-posting attacks,
  // see https://go.microsoft.com/fwlink/?linkid=2123754
  [HttpPut("{id:guid}")]
  public async Task<IActionResult> PutJobPosting(Guid id,
    JobPostingDto jobPostingDto)
  {
    if (id != jobPostingDto.JobId)
    {
      return BadRequest();
    }

    Context.Entry(jobPostingDto).State = EntityState.Modified;

    try
    {
      await Context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
      if (!JobPostingExists(id))
      {
        return NotFound();
      }

      throw;
    }

    return NoContent();
  }

  // POST: api/v{version}/JobPosting
  // To protect from overposting attacks,
  // see https://go.microsoft.com/fwlink/?linkid=2123754
  [HttpPost]
  public async Task<ActionResult<JobPostingDto>> PostJobPosting(
    JobPostingDto jobPostingDto)
  {
    var jobPosting = new JobPosting
    {
      JobId = jobPostingDto.JobId,
      Title = jobPostingDto.Title,
      Description = jobPostingDto.Description,
      Location = jobPostingDto.Location,
      ExperienceLevel = jobPostingDto.ExperienceLevel,
      PostedBy = jobPostingDto.PostedBy,
      CreatedAt = jobPostingDto.CreatedAt,
      UpdatedAt = jobPostingDto.UpdatedAt,
      Secrect = "This is a secret."
    };
    Context.JobPostings.Add(jobPosting);
    await Context.SaveChangesAsync();

    return CreatedAtAction(
      "GetJobPosting",
      new { id = jobPostingDto.JobId },
      jobPostingDto);
  }

  // DELETE: api/v{version}/JobPosting/5
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteJobPosting(Guid id)
  {
    var jobPosting = await Context.JobPostings.FindAsync(id);
    if (jobPosting == null)
    {
      return NotFound();
    }

    Context.JobPostings.Remove(jobPosting);
    await Context.SaveChangesAsync();

    return NoContent();
  }

  private bool JobPostingExists(Guid id)
  {
    return Context.JobPostings.Any(e => e.JobId == id);
  }
}
