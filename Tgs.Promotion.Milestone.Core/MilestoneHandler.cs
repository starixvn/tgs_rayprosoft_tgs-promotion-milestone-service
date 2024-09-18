using MediatR;
using Tgs.Promotion.Milestone.Domain.Interfaces;
using Tgs.Promotion.Milestone.Domain.Models;

namespace Tgs.Promotion.Milestone.Core;

public class MilestoneHandler(IMilestoneRepository milestoneRepository)
    : IStreamRequestHandler<GetMilestoneRequirementsFulfilledList, ValidMilestone>
{
    public IAsyncEnumerable<ValidMilestone> Handle(GetMilestoneRequirementsFulfilledList request,
        CancellationToken cancellationToken)
    {
        return milestoneRepository.GetRequirementsFulfilledListAsync(request.PromotionId, request.PlayerCode, cancellationToken);
    }
}