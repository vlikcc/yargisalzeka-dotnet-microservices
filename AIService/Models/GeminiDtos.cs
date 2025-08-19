using System.ComponentModel.DataAnnotations;

namespace AIService.Models;

public class KeywordRequest
{
    [Required]
    public string CaseText { get; set; } = string.Empty;
}

public class RelevanceRequest
{
    [Required]
    public string CaseText { get; set; } = string.Empty;
    [Required]
    public string DecisionText { get; set; } = string.Empty;
}

public class RelevanceResponse
{
    public int Score { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public string Similarity { get; set; } = string.Empty;
}

public class RelevantDecisionDto
{
    public string? Title { get; set; }
    public string? Summary { get; set; }
}

public class PetitionRequest
{
    [Required]
    public string CaseText { get; set; } = string.Empty;
    [Required]
    public List<RelevantDecisionDto> RelevantDecisions { get; set; } = new();
}
