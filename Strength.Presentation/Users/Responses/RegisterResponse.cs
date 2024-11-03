namespace Strength.Presentation.Users.Responses;

using Base;

internal sealed record RegisterResponse(string Feedback) : FeedbackResponse(Feedback);
