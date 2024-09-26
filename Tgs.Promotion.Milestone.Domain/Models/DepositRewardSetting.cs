namespace Tgs.Promotion.Milestone.Domain.Models;

public record DepositRewardSetting(
    decimal Percentage,
    decimal BonusAmountLimit,
    decimal TurnoverMultiple,
    int[] VipLevels,
    string[]? BucketTypes);