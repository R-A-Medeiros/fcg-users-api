using FIAP.Application.UseCases.User.ChangeRole;
using FIAP.Application.UseCases.User.Register;
using FIAP.Domain.Communication.Requests;
using FIAP.Domain.Communication.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.UsersAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ResponseRegisteredUserJson), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromServices] IRegisterUserUseCase useCase,
        [FromBody] RequestRegisterUserJson request)
    {
        var response = await useCase.Execute(request);

        return Created(string.Empty, response);
    }

    /// <summary>Changes a user's role. Restricted to administrators.</summary>
    [HttpPatch("{userIdentifier:guid}/role")]
    [Authorize(Roles = "administrator")]
    [ProducesResponseType(typeof(ResponseUserRoleJson), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeRole(
        [FromServices] IChangeUserRoleUseCase useCase,
        [FromRoute] Guid userIdentifier,
        [FromBody] RequestChangeRoleJson request)
    {
        var response = await useCase.Execute(userIdentifier, request);

        return Ok(response);
    }
}
