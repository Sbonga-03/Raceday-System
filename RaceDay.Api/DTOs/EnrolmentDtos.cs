using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Models;

namespace RaceDay.Api.DTOs;

public class ResultRequest
{
    public TimeSpan? FinishTime { get; set; }

    [Range(1, 100000)]
    public int? Position { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Finished";
}

public class ResultResponse
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public string Status { get; set; } = string.Empty;

    public static ResultResponse From(Result r) => new()
    {
        ResultId = r.ResultID,
        EnrolmentId = r.EnrolmentID,
        ParticipantName = r.Enrolment?.Participant?.FullName ?? string.Empty,
        CategoryName = r.Enrolment?.Category?.Name ?? string.Empty,
        FinishTime = r.FinishTime,
        Position = r.Position,
        Status = r.Status
    };
}

public class EnrolmentResponse
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool HasResult { get; set; }

    public static EnrolmentResponse From(EventEnrolment e) => new()
    {
        EnrolmentId = e.EnrolmentID,
        ParticipantId = e.ParticipantID,
        ParticipantName = e.Participant?.FullName ?? string.Empty,
        CategoryId = e.CategoryID,
        CategoryName = e.Category?.Name ?? string.Empty,
        EventId = e.Category?.EventID ?? 0,
        EventName = e.Category?.Event?.Name ?? string.Empty,
        EnrolmentDate = e.EnrolmentDate,
        Status = e.Status,
        HasResult = e.Result != null
    };
}