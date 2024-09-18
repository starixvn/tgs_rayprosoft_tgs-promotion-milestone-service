namespace Tgs.Promotion.Milestone.Domain.Models;

public class ValidMilestone
{
    public string PlayerCode { get; set; } = default!;
    public Guid PromotionId { get; set; }
    public int MilestoneLevel { get; set; }
    public decimal Bonus { get; set; }
    public decimal BucketAmount { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal ValidBetAmount { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? SentOn { get; set; }
}