using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tgs.Promotion.Milestone.Domain.Entities;
using Tgs.Promotion.Milestone.Domain.Interfaces;

namespace Tgs.Promotion.Milestone.Repository.DependencyInjection;

public static class TgsRepositoryExtensions
{
    public static IServiceCollection AddMilestoneRepositories(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> dbConfigure)
    {
        return services
            .AddDbContext<tgsContext>(dbConfigure)
            .AddTransient<IMilestoneRepository, MilestoneRepository>();
    }
}