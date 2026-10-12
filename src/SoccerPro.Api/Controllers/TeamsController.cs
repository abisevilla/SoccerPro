using Microsoft.AspNetCore.Mvc;
using SoccerPro.Application;
using SoccerPro.Domain;

namespace SoccerPro.Api.Controllers;

[Route("api/teams")]
public sealed class TeamsController : ApiControllerBase
{
    private readonly ISoccerQueryService queries;
    private readonly TeamService service;

    public TeamsController(ISoccerQueryService queries, TeamService service)
    {
        this.queries = queries;
        this.service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "name", [FromQuery] string sortDirection = "asc", [FromQuery] string? name = null, CancellationToken cancellationToken = default) =>
        Ok(await queries.GetTeamsAsync(new PageRequest { PageNumber = pageNumber, PageSize = pageSize, SortBy = sortBy, SortDirection = sortDirection }, name, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var team = await queries.GetTeamAsync(id, cancellationToken);
        return team is null ? NotFound(new Error("Team was not found.", "not_found")) : Ok(team);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTeamRequest request, CancellationToken cancellationToken = default) =>
        From(await service.CreateAsync(request, cancellationToken), team => CreatedAtAction(nameof(GetById), new { id = team.Id }, new TeamDto { Id = team.Id, Name = team.Name, City = team.City, ModifiedAt = team.ModifiedAt }));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTeamRequest request, CancellationToken cancellationToken = default) =>
        From(await service.UpdateAsync(id, request, cancellationToken), team => Ok(new TeamDto { Id = team.Id, Name = team.Name, City = team.City, ModifiedAt = team.ModifiedAt }));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default) =>
        From(await service.DeleteAsync(id, cancellationToken), NoContent);
}
