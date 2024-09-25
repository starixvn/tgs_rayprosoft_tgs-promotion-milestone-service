namespace Tgs.Promotion.Milestone.Client.Models;

public record GetPlayerConditionRequest(Guid BrandId, Guid PromotionId, string PlayerCode);