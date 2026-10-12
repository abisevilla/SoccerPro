using Microsoft.AspNetCore.Mvc;
using SoccerPro.Application;
using SoccerPro.Domain;

namespace SoccerPro.Api.Controllers;

[Route("api/players")]
public sealed class PlayersController : ApiControllerBase
{
    private readonly ISoccerQueryService queries;
    private readonly PlayerService service;

    public PlayersController(ISoccerQueryService queries, PlayerService service)
    {
        this.queries = queries;
        this.service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "name", [FromQuery] string sortDirection = "asc", [FromQuery] Guid? teamId = null, [FromQuery] string? name = null, [FromQuery] string? position = null, CancellationToken cancellationToken = default) =>
        Ok(await queries.GetPlayersAsync(new PageRequest { PageNumber = pageNumber, PageSize = pageSize, SortBy = sortBy, SortDirection = sortDirection }, teamId, name, position, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var player = await queries.GetPlayerAsync(id, cancellationToken);
        return player is null ? NotFound(new Error("Player was not found.", "not_found")) : Ok(player);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePlayerRequest request, CancellationToken cancellationToken = default) =>
        From(await service.CreateAsync(request, cancellationToken), player => CreatedAtAction(nameof(GetById), new { id = player.Id }, new PlayerDto { Id = player.Id, TeamId = player.TeamId, Name = player.Name, Position = player.Position, ModifiedAt = player.ModifiedAt }));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdatePlayerRequest request, CancellationToken cancellationToken = default) =>
        From(await service.UpdateAsync(id, request, cancellationToken), player => Ok(new PlayerDto { Id = player.Id, TeamId = player.TeamId, Name = player.Name, Position = player.Position, ModifiedAt = player.ModifiedAt }));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default) =>
        From(await service.DeleteAsync(id, cancellationToken), NoContent);
}
