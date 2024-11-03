namespace Strength.Presentation.Users.Responses;

using Base;

internal sealed record VerifyResponse(string Feedback) : FeedbackResponse(Feedback);
