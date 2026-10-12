using Microsoft.AspNetCore.Mvc;
using SoccerPro.Application;

namespace SoccerPro.Api.Controllers;

[Route("api/standings")]
public sealed class StandingsController : ApiControllerBase
{
    private readonly ISoccerQueryService queries;

    public StandingsController(ISoccerQueryService queries)
    {
        this.queries = queries;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default) =>
        Ok(await queries.GetStandingsAsync(cancellationToken));
}
