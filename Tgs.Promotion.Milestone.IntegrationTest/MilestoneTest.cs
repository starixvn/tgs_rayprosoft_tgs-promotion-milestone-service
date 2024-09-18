using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tgs.Promotion.Milestone.Client;
using Tgs.Promotion.Milestone.Domain.Entities;
using Tgs.Promotion.Milestone.Domain.Interfaces;
using Tgs.Promotion.Milestone.Domain.Models;

namespace Tgs.Promotion.Milestone.IntegrationTest
{
    public class MilestoneTest
    {
        [Fact]
        public async Task Get()
        {
            var fakeDbContext = Substitute.For<tgsContext>();
            var fakePromotionMilestoneClient = Substitute.For<IPromotionMilestoneClient>();
            var fakeRepository = Substitute.For<IMilestoneRepository>();

            var brandId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();
            var playerCode = Guid.NewGuid().ToString();

            fakeRepository
                .GetRequirementsFulfilledListAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new ValidMilestone[]
                {
                    new ValidMilestone
                    {
                        PlayerCode = playerCode,
                        PromotionId = promotionId,
                        Bonus = 10,
                        DepositAmount = 10,
                        MilestoneLevel = 1,
                        ValidBetAmount = 10
                    }
                }.ToAsyncEnumerable());

            await using var web = new TestApplication(builder => builder.ConfigureServices(services =>
            {
                services.AddSingleton(fakeDbContext)
                    .AddSingleton(fakePromotionMilestoneClient)
                    .AddSingleton(fakeRepository);
            }));
            var httpClient = web.CreateClient();

            var sut = new PromotionMilestoneClient(httpClient);

            var response = await sut.GetMilestoneRequirementsFulfilledListAsync(brandId, playerCode).ToArrayAsync();

            Assert.Equal(response.Length, 1);
        }
    }
}
