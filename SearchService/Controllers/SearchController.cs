using Microsoft.AspNetCore.Mvc;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
	private readonly ISearchProvider _searchProvider;
	private readonly ILogger<SearchController> _logger;

	public SearchController(ISearchProvider searchProvider, ILogger<SearchController> logger)
	{
		_searchProvider = searchProvider;
		_logger = logger;
	}

	[HttpPost]
	[ProducesResponseType(typeof(List<DecisionDto>), 200)]
	public async Task<IActionResult> Search([FromBody] SearchRequest request, CancellationToken cancellationToken)
	{
		if (request.Keywords == null || request.Keywords.Count == 0)
		{
			return BadRequest("Anahtar kelimeler gerekli.");
		}

		var results = await _searchProvider.SearchAsync(request.Keywords, cancellationToken);
		return Ok(results);
	}
}


