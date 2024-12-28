namespace Strength.Presentation.Muscle;

using Application.Muscle.Commands.UpdateMuscle;
using Domain.Entities.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("api/muscle")]
[Authorize(Roles = nameof(RoleName.Admin))]
[ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status500InternalServerError)]
public class MuscleController(ISender sender) : ApiController(sender)
{
    [HttpPut]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAsync(
        [FromBody] UpdateMuscleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok("Muscle successfully updated")
            : HandleFailure(result);
    }
}
