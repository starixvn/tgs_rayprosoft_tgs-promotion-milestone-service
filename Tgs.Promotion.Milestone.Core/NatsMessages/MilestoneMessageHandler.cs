using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using TGS.Core.Infrastructures.Nats.Interface;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Milestone.Domain;
using Tgs.Promotion.Milestone.Domain.Interfaces;
using Tgs.Promotion.Milestone.Domain.Models;
using Tgs.SerialNumberGenerator;

namespace Tgs.Promotion.Milestone.Core.NatsMessages;

public class MilestoneMessageHandler(
    IPromotionServiceClient promotionServiceClient,
    ISerialNumberGenerator serialNumberGenerator,
    IMilestoneRepository milestoneRepository,
    ILogger<MilestoneMessageHandler> logger) : IMessageHandler<string>
{
    public async Task Handle(string subject, string? queueGroup, NatsHeaders? natsHeaders, string? message,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Milestone promotion distribute to start.");

        var now = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8));

        var today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);

        var promotions = await promotionServiceClient
            .ListPromotionsByPromotionTypeAsync(Const.Milestone, cancellationToken)
            .Where(x => x.EventDuring.From <= today && x.EventDuring.To.Date >= today)
            .ToListAsync(cancellationToken);

        var dateTimeToHour = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, 0, 0, now.Offset);

        await Parallel.ForEachAsync(promotions, cancellationToken, async (promotion, ct) =>
        {
            var rule = await promotionServiceClient.GetPromotionRuleAsync<PromotionRule>(
                promotion.Id,
                ct).ConfigureAwait(false);

            if (rule?.CampaignDetail.Conditions == null) return;

            var totalBonus = 0M;

            var totalDistributeId = (Guid?)null;

            foreach (var playerCondition in await milestoneRepository.GetConditionsAsync(promotion.BrandId,
                         promotion.EventDuring.From,
                         dateTimeToHour.UtcDateTime,
                         today.DateTime, ct).ToArrayAsync(ct).ConfigureAwait(false))
            {
                var (bonus, distributeId) = await ProcessPlayerConditionAsync(promotion, rule, playerCondition, ct)
                    .ConfigureAwait(false);

                totalBonus += bonus;
                if (distributeId.HasValue) totalDistributeId = distributeId;
            }

            if (totalDistributeId is not null)
                await promotionServiceClient.UpdatePromotionUsedAmountAsync(promotion.Id, totalBonus,
                    totalDistributeId, ct).ConfigureAwait(false);
        });
    }

    private async Task<(decimal Bonus, Guid? DistributeId)> ProcessPlayerConditionAsync(
        PromotionMainInfo promotion,
        PromotionRule rule,
        PlayerCondition playerCondition,
        CancellationToken ct)
    {
        var bonus = 0M;
        var distributeId = (Guid?)null;

        foreach (var campaignDetailCondition in rule.CampaignDetail.Conditions)
        {
            var id = await serialNumberGenerator.GenerateAsync(ct)
                .ConfigureAwait(false);

            if (playerCondition.DepositAmount >= campaignDetailCondition.MinimumDeposit &&
                playerCondition.ValidBetAmount >= campaignDetailCondition.MinimumValidBet)
                try
                {
                    var milestone = await milestoneRepository.GetMilestoneByLevelAsync(promotion.BrandId,
                            promotion.Id, playerCondition.PlayerCode, campaignDetailCondition.Level, ct)
                        .ConfigureAwait(false);

                    var bucketAmount = campaignDetailCondition.TurnoverMultiple * campaignDetailCondition.Bonus;

                    if (milestone.id is null)
                    {
                        await milestoneRepository.CreateMilestoneAsync(new CreatePromotionMilestone
                        {
                            Id = id,
                            BrandId = promotion.BrandId,
                            PromotionId = promotion.Id,
                            MilestoneLevel = campaignDetailCondition.Level,
                            Bonus = campaignDetailCondition.Bonus,
                            BucketAmount = bucketAmount,
                            DepositAmount = playerCondition.DepositAmount,
                            ValidBetAmount = playerCondition.ValidBetAmount,
                            PlayerCode = playerCondition.PlayerCode
                        }, ct).ConfigureAwait(false);
                    }
                    else
                    {
                        id = milestone.id.Value;
                    }

                    if (milestone.promotionDistributeId is null)
                    {
                        var distPlan = await promotionServiceClient.GetPromotionDistributePlanAsync(
                            promotion.Id,
                            ComputeSha256Hash.ComputeSha256HashToLong(
                                $"{playerCondition.PlayerCode}{campaignDetailCondition.Level}"),
                            ct).ConfigureAwait(false);

                        distributeId = await promotionServiceClient.DistributePromotionBonusAsync(
                            distPlan.Id,
                            playerCondition.PlayerCode,
                            campaignDetailCondition.Bonus,
                            bucketAmount,
                            null,
                            id,
                            rule.CampaignDetail.BucketTypes,
                            ct).ConfigureAwait(false);

                        bonus += campaignDetailCondition.Bonus;

                        await milestoneRepository.UpdateMilestoneReferenceIdAsync(id, distributeId.Value,
                            ct).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    logger.LogInformation(
                        "Milestone promotion: {playerCondition} {campaignDetailCondition}", playerCondition,
                        campaignDetailCondition);

                    logger.LogError(e.Message);
                }
        }

        return (bonus, distributeId);
    }
}