using Tgs.Promotion.Milestone.Client.Models;

namespace Tgs.Promotion.Milestone.Client;

public interface IPromotionMilestoneClient
{
    IAsyncEnumerable<ConditionIncludeCompleted> GetConditionsIncludeCompletedListAsync(Guid promotionId,
        string playerCode);

    ValueTask<PlayerCondition> GetPlayerConditionAsync(GetPlayerConditionRequest request);
}