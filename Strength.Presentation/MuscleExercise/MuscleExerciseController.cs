namespace Strength.Presentation.MuscleExercise;

using Application.MuscleExercise.Commands.CreateManyMuscleExercise;
using Domain.Entities.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("api/muscle-exercise")]
[Authorize(Roles = nameof(RoleName.Admin))]
[ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status500InternalServerError)]
public class MuscleExerciseController(ISender sender) : ApiController(sender)
{
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateManyAsync(
        [FromBody] CreateManyMuscleExerciseCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok("muscles and exercise successfully created")
            : HandleFailure(result);
    }
}
