// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Style",
    "IDE0072:Add missing cases to switch expression",
    Justification = "No need to implement cases that are not beeing used",
    Scope = "member",
    Target = "~M:Strength.Presentation.Shared.ApiController.HandleFailure(Strength.Domain.Shared.Result)~Microsoft.AspNetCore.Mvc.IActionResult")]
