namespace RaceDay.Web.Models;

public class EventRosterViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public List<EnrolmentInfo> Enrolments { get; set; } = new();
}

// Matches a captured result as the API sends it back for the leaderboard
public class RecordedResult
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ResultsPageViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public bool IsConcluded { get; set; }
    public List<EnrolmentInfo> Pending { get; set; } = new();
    public List<RecordedResult> Recorded { get; set; } = new();
}

// What the organiser types into the result form
public class ResultEntryViewModel
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string? FinishTime { get; set; }
    public int? Position { get; set; }
    public string? Status { get; set; }
}