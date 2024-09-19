
using MediatR;

namespace Tgs.Promotion.Milestone.Domain.Models
{
    public record GetConditionsIncludeCompletedList(Guid PromotionId, string PlayerCode)
        : IStreamRequest<ConditionIncludeCompleted>;
}
