
using MediatR;

namespace Tgs.Promotion.Milestone.Domain.Models
{
    public record GetMilestoneRequirementsFulfilledList(Guid PromotionId, string PlayerCode)
        : IStreamRequest<ValidMilestone>;
}
