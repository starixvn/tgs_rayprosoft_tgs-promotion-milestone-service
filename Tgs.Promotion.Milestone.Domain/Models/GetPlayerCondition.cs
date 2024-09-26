using MediatR;

namespace Tgs.Promotion.Milestone.Domain.Models;

public record GetPlayerCondition(Guid BrandId, Guid PromotionId, string PlayerCode)
    : IRequest<PlayerCondition>;