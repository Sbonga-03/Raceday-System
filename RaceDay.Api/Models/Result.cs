using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

[Table("Result")]
public class Result
{
    [Key]
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    [Required, MaxLength(30)]
    public string Status { get; set; } = "Finished";

    public EventEnrolment? Enrolment { get; set; }
}