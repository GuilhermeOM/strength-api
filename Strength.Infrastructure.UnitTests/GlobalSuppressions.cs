// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Usage",
    "CA2201:Não gerar tipos de exceção reservados",
    Justification = "Generic exceptions simplyfies some test use cases",
    Scope = "member",
    Target = "~M:Strength.Infrastructure.UnitTests.Persistence.UnitOfWorkTests.BeginTransactionAsyncShouldRollbackWhenExceptionIsThrown~System.Threading.Tasks.Task")]
