using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Tgs.Promotion.Milestone.Core.DependencyInjection;

public static class CoreExtensions
{
    public static IServiceCollection AddPromotionMilestoneCore(this IServiceCollection services)
    {
        services.AddMediatR(r => r.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}