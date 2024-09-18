using Tgs.Promotion.Milestone.Domain.Enums;

namespace Tgs.Promotion.Milestone.Domain.Models;

public class CreatePromotionMilestone
{
    public Guid Id { get; set; }
    public Guid BrandId { get; set; }
    public Guid PromotionId { get; set; }
    public int MilestoneLevel { get; set; }
    public decimal Bonus { get; set; }
    public decimal BucketAmount { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal ValidBetAmount { get; set; }
    public string PlayerCode { get; set; } = default!;
}
