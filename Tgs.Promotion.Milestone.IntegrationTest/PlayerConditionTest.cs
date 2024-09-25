using Castle.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Promotion.Milestone.Client;
using Tgs.Promotion.Milestone.Domain.Entities;
using Tgs.Promotion.Milestone.Domain.Interfaces;

namespace Tgs.Promotion.Milestone.IntegrationTest
{
    public class PlayerConditionTest
    {
        [Fact]
        public async Task Get()
        {
            var fakeDbContext = Substitute.For<tgsContext>();
            var fakePromotionMilestoneClient = Substitute.For<IPromotionMilestoneClient>();
            var fakeRepository = Substitute.For<IMilestoneRepository>();
            var fakeLogger = Substitute.For<ILogger<PromotionMilestoneClient>>();

            var brandId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();
            var playerCode = Guid.NewGuid().ToString();

            fakeRepository.GetPlayerConditionAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime>(),
                Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns((10, 20));

            await using var web = new TestApplication(builder => builder.ConfigureServices(services =>
            {
                services.AddSingleton(fakeDbContext)
                    .AddSingleton(fakePromotionMilestoneClient)
                    .AddSingleton(fakeRepository)
                    .AddSingleton(fakeLogger);
            }));
            var httpClient = web.CreateClient();

            var sut = new PromotionMilestoneClient(httpClient, fakeLogger);

            await sut.GetPlayerConditionAsync(brandId, playerCode, DateTime.Today.AddDays(-5), DateTime.Today, default);

            _ = fakeRepository.ReceivedWithAnyArgs(1).GetPlayerConditionAsync(default, default!, default, default, default);
        }
    }
}
