using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AIService.Models;

namespace AIService.Services;

public class GeminiAiService : IGeminiAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeminiAiService> _logger;
    private readonly string _apiKey;
    private const string FlashModel = "gemini-1.5-flash";
    private const string ProModel = "gemini-1.5-pro";

    public GeminiAiService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<GeminiAiService> logger)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
    }

    public async Task<List<string>> ExtractKeywordsFromCaseAsync(string caseText)
    {
        var prompt = $$"""
A?a??daki hukuki olay metnini analiz et ve Yarg?tay kararlar?nda arama yapmak için en uygun anahtar kelimeleri ç?kar.
Anahtar kelimeler Türk hukuku terminolojisine uygun olmal?.

Olay metni:
{caseText}

Sadece anahtar kelimeleri virgülle ay?rarak listele. Aç?klama yazma.
Örnek format: "tazminat, sözle?me ihlali, maddi zarar, manevi tazminat"
""";
        try
        {
            var text = await SendPromptAsync(prompt, FlashModel);
            var kws = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                          .Select(k => k.Trim())
                          .Where(k => k.Length > 0)
                          .Distinct(StringComparer.OrdinalIgnoreCase)
                          .ToList();
            return kws;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Anahtar kelime ç?karma hatas?");
            return new List<string> { "tazminat", "hukuki sorumluluk" };
        }
    }

    public async Task<RelevanceResponse> AnalyzeDecisionRelevanceAsync(string caseText, string decisionText)
    {
        var truncated = decisionText.Length > 2000 ? decisionText[..2000] : decisionText;
        var prompt = $$"""
Olay metni ile Yarg?tay karar? aras?ndaki ili?kiyi analiz et.

OLAY METN?:
{caseText}

YARGITAY KARARI (k?salt?lm??):
{truncated}

A?a??daki formatta cevap ver:
PUAN: [0-100 aras? say?]
AÇIKLAMA: [K?sa aç?klama]
BENZERLIK: [Hangi konularda benzer]
""";
        try
        {
            var text = await SendPromptAsync(prompt, ProModel);
            return ParseAnalysisResponse(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Karar analiz hatas?");
            return new RelevanceResponse { Score = 50, Explanation = "Analiz s?ras?nda hata olu?tu", Similarity = "Belirlenemedi" };
        }
    }

    public async Task<string> GeneratePetitionTemplateAsync(string caseText, List<RelevantDecisionDto> relevantDecisions)
    {
        var sb = new StringBuilder();
        foreach (var d in relevantDecisions.Take(3))
        {
            var summary = d.Summary;
            if (!string.IsNullOrEmpty(summary) && summary.Length > 200) summary = summary[..200];
            sb.AppendLine($"- {d.Title ?? "Ba?l?k yok"}: {summary ?? "Özet yok"}");
        }
        var prompt = $$"""
A?a??daki bilgileri kullanarak hukuki dilekçe ?ablonu olu?tur.

OLAY METN?:
{caseText}

ALAKALI YARGITAY KARARLARI:
{sb}

Standart hukuki dilekçe format?nda, emsal kararlar? referans alan bir ?ablon üret.
Bölümler:
- Ba?l?k
- Taraflar
- Olaylar
- Hukuki Dayanak
- Emsal Kararlar
- Talep
""";
        try
        {
            var text = await SendPromptAsync(prompt, ProModel);
            return text.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dilekçe olu?turma hatas?");
            return "Dilekçe ?ablonu olu?turulamad?. Lütfen tekrar deneyin.";
        }
    }

    private async Task<string> SendPromptAsync(string prompt, string model)
    {
        if (string.IsNullOrWhiteSpace(_apiKey)) throw new InvalidOperationException("Gemini API key missing.");
        var client = _httpClientFactory.CreateClient("Gemini");
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]{ new { text = prompt } }
                    }
                }
            }), Encoding.UTF8, "application/json")
        };

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gemini API failure {Status}: {Body}", response.StatusCode, body);
            throw new HttpRequestException($"Gemini API error {(int)response.StatusCode}");
        }
        try
        {
            using var doc = JsonDocument.Parse(body);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            return text ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API response parse error: {Body}", body);
            return string.Empty;
        }
    }

    private RelevanceResponse ParseAnalysisResponse(string text)
    {
        var resp = new RelevanceResponse { Score = 50, Explanation = "Analiz tamamland?", Similarity = "Genel" };
        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line.StartsWith("PUAN:", StringComparison.OrdinalIgnoreCase) && int.TryParse(line.Split(':', 2)[1].Trim(), out var s))
                resp.Score = Math.Clamp(s, 0, 100);
            else if (line.StartsWith("AÇIKLAMA:", StringComparison.OrdinalIgnoreCase))
                resp.Explanation = line.Split(':', 2)[1].Trim();
            else if (line.StartsWith("BENZER", StringComparison.OrdinalIgnoreCase))
                resp.Similarity = line.Split(':', 2)[1].Trim();
        }
        return resp;
    }
}
