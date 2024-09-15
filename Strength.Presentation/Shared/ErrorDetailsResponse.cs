namespace Strength.Presentation.Shared;

using Strength.Domain.Shared;

internal sealed record ErrorDetailsResponse(string Title, int Status, CustomError[] Errors);
