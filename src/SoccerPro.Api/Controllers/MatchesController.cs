using Microsoft.AspNetCore.Mvc;
using SoccerPro.Application;
using SoccerPro.Domain;

namespace SoccerPro.Api.Controllers;

[Route("api/matches")]
public sealed class MatchesController : ApiControllerBase
{
    private readonly ISoccerQueryService queries;
    private readonly MatchService service;

    public MatchesController(ISoccerQueryService queries, MatchService service)
    {
        this.queries = queries;
        this.service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "date", [FromQuery] string sortDirection = "asc", [FromQuery] Guid? teamId = null, [FromQuery] DateTimeOffset? from = null, [FromQuery] DateTimeOffset? to = null, [FromQuery] string? status = null, CancellationToken cancellationToken = default) =>
        Ok(await queries.GetMatchesAsync(new PageRequest { PageNumber = pageNumber, PageSize = pageSize, SortBy = sortBy, SortDirection = sortDirection }, teamId, from, to, status, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var match = await queries.GetMatchAsync(id, cancellationToken);
        return match is null ? NotFound(new Error("Match was not found.", "not_found")) : Ok(match);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMatchRequest request, CancellationToken cancellationToken = default) =>
        From(await service.CreateAsync(request, cancellationToken), match => CreatedAtAction(nameof(GetById), new { id = match.Id }, ToDto(match)));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMatchRequest request, CancellationToken cancellationToken = default) =>
        From(await service.UpdateAsync(id, request, cancellationToken), match => Ok(ToDto(match)));

    [HttpPatch("{id:guid}/result")]
    public async Task<IActionResult> RecordResult(Guid id, RecordResultRequest request, CancellationToken cancellationToken = default) =>
        From(await service.RecordResultAsync(id, request, cancellationToken), match => Ok(ToDto(match)));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default) =>
        From(await service.DeleteAsync(id, cancellationToken), NoContent);

    private static MatchDto ToDto(Match match) => new()
    {
        Id = match.Id,
        HomeTeamId = match.HomeTeamId,
        AwayTeamId = match.AwayTeamId,
        ScheduledAt = match.ScheduledAt,
        HomeScore = match.HomeScore,
        AwayScore = match.AwayScore,
        ModifiedAt = match.ModifiedAt,
        Status = match.IsCompleted ? "completed" : "scheduled"
    };
}
