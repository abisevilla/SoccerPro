using Microsoft.AspNetCore.Mvc;
using SoccerPro.Domain;

namespace SoccerPro.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult From<T>(Result<T> result, Func<T, IActionResult> success)
    {
        return result.IsSuccess ? success(result.Value!) : Error(result.Error!);
    }

    protected IActionResult From(Result result, Func<IActionResult> success)
    {
        return result.IsSuccess ? success() : Error(result.Error!);
    }

    protected IActionResult Error(Error error)
    {
        return error.Code switch
        {
            "not_found" => NotFound(error),
            "conflict" => Conflict(error),
            _ => BadRequest(error)
        };
    }
}
