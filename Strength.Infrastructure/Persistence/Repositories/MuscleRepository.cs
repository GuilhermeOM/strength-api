namespace Strength.Infrastructure.Persistence.Repositories;

using Base;
using Domain.Entities;
using Domain.Repositories;

internal sealed class MuscleRepository(AppDataContext context) : BaseRepository<Muscle>(context), IMuscleRepository;
