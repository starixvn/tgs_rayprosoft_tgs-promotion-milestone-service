using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Milestone.Client;
using Tgs.Promotion.Milestone.Client.Models;
using Tgs.Promotion.Milestone.Domain.Entities;
using Tgs.Promotion.Milestone.Domain.Interfaces;

namespace Tgs.Promotion.Milestone.IntegrationTest;

public class PlayerConditionTest
{
    [Fact]
    public async Task Get()
    {
        var fakeDbContext = Substitute.For<tgsContext>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeRepository = Substitute.For<IMilestoneRepository>();
        var fakeLogger = Substitute.For<ILogger<PromotionMilestoneClient>>();

        var brandId = Guid.NewGuid();
        var promotionId = Guid.NewGuid();
        var playerCode = Guid.NewGuid().ToString();

        fakeRepository.GetPlayerConditionAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime>(),
            Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns((10, 20));

        fakePromotionServiceClient.GetPromotionMainInfoAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new PromotionMainInfo
            {
                EventDuring = new During { From = DateTime.Today, To = DateTime.Today }
            });

        await using var web = new TestApplication(builder => builder.ConfigureServices(services =>
        {
            services.AddSingleton(fakeDbContext)
                .AddSingleton(fakePromotionServiceClient)
                .AddSingleton(fakeRepository)
                .AddSingleton(fakeLogger);
        }));
        var httpClient = web.CreateClient();

        var sut = new PromotionMilestoneClient(httpClient, fakeLogger);

        await sut.GetPlayerConditionAsync(new GetPlayerConditionRequest(brandId, promotionId, playerCode));

        _ = fakeRepository.ReceivedWithAnyArgs(1).GetPlayerConditionAsync(default, default!, default, default, default);
    }
}