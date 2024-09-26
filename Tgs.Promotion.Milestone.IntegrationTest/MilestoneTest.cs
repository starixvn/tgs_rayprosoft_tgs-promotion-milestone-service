using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Milestone.Client;
using Tgs.Promotion.Milestone.Domain.Entities;
using Tgs.Promotion.Milestone.Domain.Interfaces;
using Tgs.Promotion.Milestone.Domain.Models;

namespace Tgs.Promotion.Milestone.IntegrationTest;

public class MilestoneTest
{
    [Fact]
    public async Task Get()
    {
        var fakeDbContext = Substitute.For<tgsContext>();
        var fakePromotionMilestoneClient = Substitute.For<IPromotionMilestoneClient>();
        var fakeRepository = Substitute.For<IMilestoneRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeLogger = Substitute.For<ILogger<PromotionMilestoneClient>>();

        var brandId = Guid.NewGuid();
        var promotionId = Guid.NewGuid();
        var playerCode = Guid.NewGuid().ToString();

        fakeRepository
            .GetRequirementsFulfilledListAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValidMilestone[]
            {
                new()
                {
                    PlayerCode = playerCode,
                    PromotionId = promotionId,
                    Bonus = 10,
                    DepositAmount = 10,
                    MilestoneLevel = 1,
                    ValidBetAmount = 10,
                    CreatedOn = DateTime.Today,
                    SentOn = DateTime.Today
                }
            }.ToAsyncEnumerable());

        var rule = new PromotionRule(new MilestoneSetting([], [], new Condition[]
        {
            new() { Level = 1, MinimumDeposit = 10, MinimumValidBet = 10, TurnoverMultiple = 10, Bonus = 10 },
            new() { Level = 2, MinimumDeposit = 20, MinimumValidBet = 20, TurnoverMultiple = 20, Bonus = 20 }
        }));

        fakePromotionServiceClient
            .GetPromotionRuleAsync<PromotionRule>(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(rule);

        await using var web = new TestApplication(builder => builder.ConfigureServices(services =>
        {
            services.AddSingleton(fakeDbContext)
                .AddSingleton(fakePromotionMilestoneClient)
                .AddSingleton(fakeRepository)
                .AddSingleton(fakePromotionServiceClient)
                .AddSingleton(fakeLogger);
        }));
        var httpClient = web.CreateClient();

        var sut = new PromotionMilestoneClient(httpClient, fakeLogger);

        var response = await sut.GetConditionsIncludeCompletedListAsync(brandId, playerCode).ToArrayAsync();

        Assert.Equal(2, response.Length);
    }
}