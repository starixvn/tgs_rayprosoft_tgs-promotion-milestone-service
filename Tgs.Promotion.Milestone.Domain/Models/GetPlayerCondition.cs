
using MediatR;

namespace Tgs.Promotion.Milestone.Domain.Models
{
    public record GetPlayerCondition(Guid BrandId, string PlayerCode, DateTime From, DateTime To)
        : IRequest<PlayerCondition>;
}
