using Tgs.Promotion.Milestone.Domain.Models;

namespace Tgs.Promotion.Milestone.Domain.Interfaces;

public interface IMilestoneRepository
{
    IAsyncEnumerable<PlayerCondition> GetConditionsAsync(Guid brandId, DateTime from, DateTime to, DateTime today,
        CancellationToken cancellationToken);

    IAsyncEnumerable<ValidMilestone> GetRequirementsFulfilledListAsync(Guid promotionId, string playerCode,
        CancellationToken cancellationToken);

    Task<ValidMilestone?> GetRequirementsFulfilledByIdAsync(Guid promotionId, Guid milestoneId,
        CancellationToken cancellationToken);

    Task CreateMilestoneAsync(CreatePromotionMilestone model, CancellationToken cancellationToken);

    Task UpdateMilestoneReferenceIdAsync(Guid id, Guid distributeId, CancellationToken cancellationToken);

    Task<(Guid? id, Guid? promotionDistributeId)> GetMilestoneByLevelAsync(Guid brandId, Guid promotionId,
        string playerCode,
        int milestoneLevel, CancellationToken cancellationToken);

    Task<(decimal depositAmount, decimal validBetAmount)> GetPlayerConditionAsync(Guid brandId, string playerCode,
        DateTime from, DateTime to, CancellationToken cancellationToken);
}