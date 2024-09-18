namespace Tgs.Promotion.Milestone.Service.Models;

public record GetPlayerConditionRequest(Guid BrandId, string PlayerCode, DateTime From, DateTime To);