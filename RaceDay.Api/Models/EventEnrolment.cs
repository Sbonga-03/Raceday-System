using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

[Table("EventEnrolment")]
public class EventEnrolment
{
    [Key]
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public int CategoryID { get; set; }
    public DateTime EnrolmentDate { get; set; } = DateTime.Now;
    [Required, MaxLength(30)]
    public string Status { get; set; } = "Confirmed";

    public User? Participant { get; set; }
    public Category? Category { get; set; }
    public Result? Result { get; set; }
}