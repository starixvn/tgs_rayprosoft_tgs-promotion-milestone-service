using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Milestone.Core.NatsMessages;
using Tgs.Promotion.Milestone.Domain;
using Tgs.Promotion.Milestone.Domain.Interfaces;
using Tgs.Promotion.Milestone.Domain.Models;
using Tgs.SerialNumberGenerator;

namespace Tgs.Promotion.Milestone.UnitTest
{
    public class MessageHandlerTest
    {
        [Theory]
        [InlineData(-1, 5)]
        [InlineData(0, 5)]
        [InlineData(-10, 0)]
        public async Task Get_Valid_Promotions(int from, int to)
        {
            var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
            var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
            var fakeMilestoneRepository = Substitute.For<IMilestoneRepository>();
            var fakeLogger = Substitute.For<ILogger<MilestoneMessageHandler>>();

            var sut = new MilestoneMessageHandler(
                fakePromotionServiceClient,
                fakeSerialNumberGenerator,
                fakeMilestoneRepository,
                fakeLogger);

            var brandId = Guid.NewGuid();
            var promotionId1 = Guid.NewGuid();
            var promotionId2 = Guid.NewGuid();

            var promotions = new PromotionMainInfo[]
            {
                new PromotionMainInfo
                {
                    BrandId = brandId, Id = promotionId1,
                    EventDuring = new During { From = DateTime.Today.AddDays(-1), To = DateTime.Today.AddDays(5) }
                },
                new PromotionMainInfo
                {
                    BrandId = brandId, Id = promotionId2,
                    EventDuring = new During { From = DateTime.Today.AddDays(from), To = DateTime.Today.AddDays(to) }
                },
            };

            fakePromotionServiceClient
                .ListPromotionsByPromotionTypeAsync(Arg.Is(Const.Milestone), Arg.Any<CancellationToken>())
                .Returns(promotions.ToAsyncEnumerable());

            await sut.Handle("subject", "group", default, "1", default);

            await fakePromotionServiceClient.Received(2)
                .GetPromotionRuleAsync<PromotionRule>(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(2, 5)]
        [InlineData(3, 10)]
        public async Task Get_InValid_Promotions(int from, int to)
        {
            var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
            var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
            var fakeMilestoneRepository = Substitute.For<IMilestoneRepository>();
            var fakeLogger = Substitute.For<ILogger<MilestoneMessageHandler>>();

            var sut = new MilestoneMessageHandler(
                fakePromotionServiceClient,
                fakeSerialNumberGenerator,
                fakeMilestoneRepository,
                fakeLogger);

            var brandId = Guid.NewGuid();
            var promotionId1 = Guid.NewGuid();

            var promotions = new PromotionMainInfo[]
            {
                new PromotionMainInfo
                {
                    BrandId = brandId, Id = promotionId1,
                    EventDuring = new During { From = DateTime.Today.AddDays(from), To = DateTime.Today.AddDays(to) }
                },
            };

            fakePromotionServiceClient
                .ListPromotionsByPromotionTypeAsync(Arg.Is(Const.Milestone), Arg.Any<CancellationToken>())
                .Returns(promotions.ToAsyncEnumerable());

            await sut.Handle("subject", "group", default, "1", default);

            await fakePromotionServiceClient.Received(0)
                .GetPromotionRuleAsync<PromotionRule>(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Create_Promotions()
        {
            var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
            var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
            var fakeMilestoneRepository = Substitute.For<IMilestoneRepository>();
            var fakeLogger = Substitute.For<ILogger<MilestoneMessageHandler>>();

            var sut = new MilestoneMessageHandler(
                fakePromotionServiceClient,
                fakeSerialNumberGenerator,
                fakeMilestoneRepository,
                fakeLogger);

            var brandId = Guid.NewGuid();
            var promotionId1 = Guid.NewGuid();

            var promotions = new PromotionMainInfo[]
            {
                new PromotionMainInfo
                {
                    BrandId = brandId, Id = promotionId1,
                    EventDuring = new During { From = DateTime.Today.AddDays(-1), To = DateTime.Today.AddDays(5) }
                }
            };

            var playerConditions = new PlayerCondition[]
            {
                new PlayerCondition
                {
                    PlayerCode = "p1",
                    DepositAmount = 10,
                    ValidBetAmount = 10,
                },
                new PlayerCondition
                {
                    PlayerCode = "p2",
                    DepositAmount = 11,
                    ValidBetAmount = 11,
                },
                new PlayerCondition
                {
                    PlayerCode = "p3",
                    DepositAmount = 8,
                    ValidBetAmount = 11,
                }
            };

            fakePromotionServiceClient
                .ListPromotionsByPromotionTypeAsync(Arg.Is(Const.Milestone), Arg.Any<CancellationToken>())
                .Returns(promotions.ToAsyncEnumerable());

            fakePromotionServiceClient
                .GetPromotionRuleAsync<PromotionRule>(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
                    new PromotionRule(new MilestoneSetting([1, 2, 3], ["Slot", "Sports"],
                    [
                        new Conditions
                            { Bonus = 10, Level = 1, MinimumDeposit = 10, MinimumValidBet = 10, TurnoverMultiple = 10 }
                    ])));

            fakeMilestoneRepository.GetConditionsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(),
                Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(playerConditions.ToAsyncEnumerable());

            fakeMilestoneRepository.GetMilestoneByLevelAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((null, null));

            await sut.Handle("subject", "group", default, "1", default);

            await fakePromotionServiceClient.Received(1)
                .GetPromotionRuleAsync<PromotionRule>(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

            await fakeMilestoneRepository.Received(2).CreateMilestoneAsync(Arg.Any<CreatePromotionMilestone>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Distribute_Promotions()
        {
            var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
            var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
            var fakeMilestoneRepository = Substitute.For<IMilestoneRepository>();
            var fakeLogger = Substitute.For<ILogger<MilestoneMessageHandler>>();

            var sut = new MilestoneMessageHandler(
                fakePromotionServiceClient,
                fakeSerialNumberGenerator,
                fakeMilestoneRepository,
                fakeLogger);

            var brandId = Guid.NewGuid();
            var promotionId1 = Guid.NewGuid();

            var promotions = new PromotionMainInfo[]
            {
                new PromotionMainInfo
                {
                    BrandId = brandId, Id = promotionId1,
                    EventDuring = new During { From = DateTime.Today.AddDays(-1), To = DateTime.Today.AddDays(5) }
                }
            };

            var playerConditions = new PlayerCondition[]
            {
                new PlayerCondition
                {
                    PlayerCode = "p1",
                    DepositAmount = 10,
                    ValidBetAmount = 10,
                },
                new PlayerCondition
                {
                    PlayerCode = "p2",
                    DepositAmount = 11,
                    ValidBetAmount = 11,
                },
                new PlayerCondition
                {
                    PlayerCode = "p3",
                    DepositAmount = 8,
                    ValidBetAmount = 11,
                }
            };

            fakePromotionServiceClient
                .ListPromotionsByPromotionTypeAsync(Arg.Is(Const.Milestone), Arg.Any<CancellationToken>())
                .Returns(promotions.ToAsyncEnumerable());

            fakePromotionServiceClient
                .GetPromotionRuleAsync<PromotionRule>(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
                    new PromotionRule(new MilestoneSetting([1, 2, 3], ["Slot", "Sports"],
                    [
                        new Conditions
                            { Bonus = 10, Level = 1, MinimumDeposit = 10, MinimumValidBet = 10, TurnoverMultiple = 10 }
                    ])));

            fakeMilestoneRepository.GetConditionsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(),
                Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(playerConditions.ToAsyncEnumerable());

            fakeMilestoneRepository.GetMilestoneByLevelAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((null, null));

            fakePromotionServiceClient
                .GetPromotionDistributePlanAsync(Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
                .Returns(new PromotionDistributePlanInfo(Guid.NewGuid(), brandId, promotionId1, 123));

            await sut.Handle("subject", "group", default, "1", default);

            await fakePromotionServiceClient.Received(1)
                .GetPromotionRuleAsync<PromotionRule>(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

            await fakeMilestoneRepository.Received(2).CreateMilestoneAsync(Arg.Any<CreatePromotionMilestone>(), Arg.Any<CancellationToken>());

            await fakePromotionServiceClient.Received(2).DistributePromotionBonusAsync(Arg.Any<Guid>(), Arg.Any<string>(),
                Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<string?>(), Arg.Any<Guid?>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Distribute_Promotions_With_Exists_Milestone_Record()
        {
            var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
            var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
            var fakeMilestoneRepository = Substitute.For<IMilestoneRepository>();
            var fakeLogger = Substitute.For<ILogger<MilestoneMessageHandler>>();

            var sut = new MilestoneMessageHandler(
                fakePromotionServiceClient,
                fakeSerialNumberGenerator,
                fakeMilestoneRepository,
                fakeLogger);

            var brandId = Guid.NewGuid();
            var promotionId1 = Guid.NewGuid();

            var promotions = new PromotionMainInfo[]
            {
                new PromotionMainInfo
                {
                    BrandId = brandId, Id = promotionId1,
                    EventDuring = new During { From = DateTime.Today.AddDays(-1), To = DateTime.Today.AddDays(5) }
                }
            };

            var playerConditions = new PlayerCondition[]
            {
                new PlayerCondition
                {
                    PlayerCode = "p1",
                    DepositAmount = 10,
                    ValidBetAmount = 10,
                },
                new PlayerCondition
                {
                    PlayerCode = "p2",
                    DepositAmount = 11,
                    ValidBetAmount = 11,
                },
                new PlayerCondition
                {
                    PlayerCode = "p3",
                    DepositAmount = 8,
                    ValidBetAmount = 11,
                }
            };

            fakePromotionServiceClient
                .ListPromotionsByPromotionTypeAsync(Arg.Is(Const.Milestone), Arg.Any<CancellationToken>())
                .Returns(promotions.ToAsyncEnumerable());

            fakePromotionServiceClient
                .GetPromotionRuleAsync<PromotionRule>(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
                    new PromotionRule(new MilestoneSetting([1, 2, 3], ["Slot", "Sports"],
                    [
                        new Conditions
                            { Bonus = 10, Level = 1, MinimumDeposit = 10, MinimumValidBet = 10, TurnoverMultiple = 10 }
                    ])));

            fakeMilestoneRepository.GetConditionsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(),
                Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(playerConditions.ToAsyncEnumerable());

            fakeMilestoneRepository.GetMilestoneByLevelAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((Guid.NewGuid(), null));

            fakePromotionServiceClient
                .GetPromotionDistributePlanAsync(Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
                .Returns(new PromotionDistributePlanInfo(Guid.NewGuid(), brandId, promotionId1, 123));

            await sut.Handle("subject", "group", default, "1", default);

            await fakePromotionServiceClient.Received(1)
                .GetPromotionRuleAsync<PromotionRule>(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

            await fakeMilestoneRepository.Received(0).CreateMilestoneAsync(Arg.Any<CreatePromotionMilestone>(), Arg.Any<CancellationToken>());

            await fakePromotionServiceClient.Received(2).DistributePromotionBonusAsync(Arg.Any<Guid>(), Arg.Any<string>(),
                Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<string?>(), Arg.Any<Guid?>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Distribute_Promotions_When_Already_Sent()
        {
            var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
            var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
            var fakeMilestoneRepository = Substitute.For<IMilestoneRepository>();
            var fakeLogger = Substitute.For<ILogger<MilestoneMessageHandler>>();

            var sut = new MilestoneMessageHandler(
                fakePromotionServiceClient,
                fakeSerialNumberGenerator,
                fakeMilestoneRepository,
                fakeLogger);

            var brandId = Guid.NewGuid();
            var promotionId1 = Guid.NewGuid();

            var promotions = new PromotionMainInfo[]
            {
                new PromotionMainInfo
                {
                    BrandId = brandId, Id = promotionId1,
                    EventDuring = new During { From = DateTime.Today.AddDays(-1), To = DateTime.Today.AddDays(5) }
                }
            };

            var playerConditions = new PlayerCondition[]
            {
                new PlayerCondition
                {
                    PlayerCode = "p1",
                    DepositAmount = 10,
                    ValidBetAmount = 10,
                },
                new PlayerCondition
                {
                    PlayerCode = "p2",
                    DepositAmount = 11,
                    ValidBetAmount = 11,
                },
                new PlayerCondition
                {
                    PlayerCode = "p3",
                    DepositAmount = 8,
                    ValidBetAmount = 11,
                }
            };

            fakePromotionServiceClient
                .ListPromotionsByPromotionTypeAsync(Arg.Is(Const.Milestone), Arg.Any<CancellationToken>())
                .Returns(promotions.ToAsyncEnumerable());

            fakePromotionServiceClient
                .GetPromotionRuleAsync<PromotionRule>(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
                    new PromotionRule(new MilestoneSetting([1, 2, 3], ["Slot", "Sports"],
                    [
                        new Conditions
                            { Bonus = 10, Level = 1, MinimumDeposit = 10, MinimumValidBet = 10, TurnoverMultiple = 10 }
                    ])));

            fakeMilestoneRepository.GetConditionsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(),
                Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(playerConditions.ToAsyncEnumerable());

            fakeMilestoneRepository.GetMilestoneByLevelAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((Guid.NewGuid(), Guid.NewGuid()));

            fakePromotionServiceClient
                .GetPromotionDistributePlanAsync(Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
                .Returns(new PromotionDistributePlanInfo(Guid.NewGuid(), brandId, promotionId1, 123));

            await sut.Handle("subject", "group", default, "1", default);

            await fakePromotionServiceClient.Received(1)
                .GetPromotionRuleAsync<PromotionRule>(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

            await fakeMilestoneRepository.Received(0).CreateMilestoneAsync(Arg.Any<CreatePromotionMilestone>(), Arg.Any<CancellationToken>());

            await fakePromotionServiceClient.Received(0).DistributePromotionBonusAsync(Arg.Any<Guid>(), Arg.Any<string>(),
                Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<string?>(), Arg.Any<Guid?>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        }
    }
}