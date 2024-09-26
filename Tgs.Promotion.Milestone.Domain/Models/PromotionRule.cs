using System.Text.Json.Serialization;

namespace Tgs.Promotion.Milestone.Domain.Models;

public record PromotionRule(
    [property: JsonPropertyName("compaignDetail")]
    MilestoneSetting CampaignDetail);