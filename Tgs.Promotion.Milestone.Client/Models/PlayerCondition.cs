namespace Tgs.Promotion.Milestone.Client.Models;

public class PlayerCondition
{
    public string PlayerCode { get; set; } = default!;
    public decimal ValidBetAmount { get; set; }
    public decimal DepositAmount { get; set; }
    public bool HavePhone { get; set; }
    public bool HaveRealName { get; set; }
    public bool HaveBirthdate { get; set; }
}