
namespace Tgs.Promotion.Milestone.Client.Models
{
    public record ConditionIncludeCompleted
    {
        public int Level { get; set; }
        public decimal MinimumValidBet { get; set; }
        public decimal MinimumDeposit { get; set; }
        public decimal Bonus { get; set; }
        public bool IsSent { get; init; }
    }
}
