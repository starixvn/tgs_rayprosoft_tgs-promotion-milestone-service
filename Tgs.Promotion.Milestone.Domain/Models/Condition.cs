
namespace Tgs.Promotion.Milestone.Domain.Models
{
    public record Condition
    {
        public int Level { get; set; }
        public decimal MinimumValidBet { get; set; }
        public decimal MinimumDeposit { get; set; }
        public decimal Bonus { get; set; }
        public decimal TurnoverMultiple { get; set; }
    }
}
