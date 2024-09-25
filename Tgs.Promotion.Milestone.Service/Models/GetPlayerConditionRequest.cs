namespace Tgs.Promotion.Milestone.Service.Models;

public record GetPlayerConditionRequest(Guid BrandId, Guid PromotionId, string PlayerCode);