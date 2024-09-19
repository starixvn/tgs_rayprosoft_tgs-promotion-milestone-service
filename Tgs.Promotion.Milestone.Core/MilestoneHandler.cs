using System.Runtime.CompilerServices;
using MediatR;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Milestone.Domain.Interfaces;
using Tgs.Promotion.Milestone.Domain.Models;

namespace Tgs.Promotion.Milestone.Core;

public class MilestoneHandler(
    IPromotionServiceClient promotionServiceClient, 
    IMilestoneRepository milestoneRepository)
    : IStreamRequestHandler<GetConditionsIncludeCompletedList, ConditionIncludeCompleted>
{
    public async IAsyncEnumerable<ConditionIncludeCompleted> Handle(GetConditionsIncludeCompletedList request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var rule = await promotionServiceClient.GetPromotionRuleAsync<PromotionRule>(
            request.PromotionId,
            cancellationToken).ConfigureAwait(false);

        var list = await milestoneRepository.GetRequirementsFulfilledListAsync(request.PromotionId, request.PlayerCode, cancellationToken).ToArrayAsync(cancellationToken);

        foreach (var campaignDetailCondition in rule?.CampaignDetail.Conditions!)
        {
            yield return new ConditionIncludeCompleted
            {
                Level = campaignDetailCondition.Level,
                MinimumDeposit = campaignDetailCondition.MinimumDeposit,
                MinimumValidBet = campaignDetailCondition.MinimumValidBet,
                Bonus = campaignDetailCondition.Bonus,
                IsSent = list.FirstOrDefault(x => x.MilestoneLevel == campaignDetailCondition.Level)?.SentOn != null
            };
        }
    }
}