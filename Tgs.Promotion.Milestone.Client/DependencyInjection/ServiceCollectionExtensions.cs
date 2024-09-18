using Microsoft.Extensions.DependencyInjection;

namespace Tgs.Promotion.Milestone.Client.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPromotionMilestoneClient(this IServiceCollection services,
        Action<IHttpClientBuilder> httpClientConfigure)
    {
        httpClientConfigure(services.AddHttpClient<IPromotionMilestoneClient, PromotionMilestoneClient>());

        return services;
    }
}