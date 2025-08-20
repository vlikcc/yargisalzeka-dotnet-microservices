using AIService.Models;
using AIService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeminiController : ControllerBase
{
    private readonly IGeminiAiService _service;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public GeminiController(IGeminiAiService service, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _service = service;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("extract-keywords")]
    [ProducesResponseType(typeof(List<string>), 200)]
    public async Task<IActionResult> ExtractKeywords([FromBody] KeywordRequest request)
    {
        var result = await _service.ExtractKeywordsFromCaseAsync(request.CaseText);
        return Ok(result);
    }

    [HttpPost("analyze-relevance")]
    [ProducesResponseType(typeof(RelevanceResponse), 200)]
    public async Task<IActionResult> AnalyzeRelevance([FromBody] RelevanceRequest request)
    {
        var result = await _service.AnalyzeDecisionRelevanceAsync(request.CaseText, request.DecisionText);
        return Ok(result);
    }

    [HttpPost("generate-petition")]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> GeneratePetition([FromBody] PetitionRequest request)
    {
        var result = await _service.GeneratePetitionTemplateAsync(request.CaseText, request.RelevantDecisions);
        return Ok(result);
    }

    [HttpPost("search-decisions")]
    [ProducesResponseType(typeof(List<DecisionSearchResult>), 200)]
    public async Task<IActionResult> SearchDecisions([FromBody] SearchDecisionsRequest request)
    {
        var baseUrl = _configuration["SearchService:BaseUrl"] ?? "http://localhost:5043";
        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync($"{baseUrl}/api/search", request);
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }
        
        // SearchService'den gelen DecisionDto'yu DecisionSearchResult'a mapping yap
        var searchResults = await response.Content.ReadFromJsonAsync<List<SearchServiceDecisionDto>>();
        if (searchResults == null)
        {
            return Ok(new List<DecisionSearchResult>());
        }

        var results = searchResults.Select(d => new DecisionSearchResult
        {
            Id = d.Id,
            Title = $"Yargıtay {d.YargitayDairesi} - {d.EsasNo}/{d.KararNo}",
            Excerpt = d.KararMetni.Length > 300 ? d.KararMetni.Substring(0, 300) + "..." : d.KararMetni,
            DecisionDate = d.KararTarihi, // Nullable DateTime
            Court = d.YargitayDairesi
        }).ToList();

        return Ok(results);
    }

    [HttpPost("analyze-case")]
    [ProducesResponseType(typeof(CaseAnalysisResponse), 200)]
    public async Task<IActionResult> AnalyzeCase([FromBody] CaseAnalysisRequest request)
    {
        var result = await _service.AnalyzeCaseTextAsync(request.CaseText);
        return Ok(result);
    }

    // SearchService'den gelen DecisionDto için internal model
    private class SearchServiceDecisionDto
    {
        public long Id { get; set; }
        public string YargitayDairesi { get; set; } = string.Empty;
        public string EsasNo { get; set; } = string.Empty;
        public string KararNo { get; set; } = string.Empty;
        public DateTime KararTarihi { get; set; }
        public string KararMetni { get; set; } = string.Empty;
    }
}
