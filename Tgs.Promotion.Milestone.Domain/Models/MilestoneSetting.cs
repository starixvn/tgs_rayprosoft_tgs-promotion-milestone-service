namespace Tgs.Promotion.Milestone.Domain.Models;
public record MilestoneSetting(
	IEnumerable<int> VipLevels,
	IEnumerable<string>? BucketTypes,
    IEnumerable<Conditions> Conditions);
