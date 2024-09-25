using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.Core.Infrastructures.Utility;
using Tgs.Promotion.Milestone.Client.Models;

namespace Tgs.Promotion.Milestone.Client;

public class PromotionMilestoneClient(HttpClient client, ILogger<PromotionMilestoneClient> logger) : IPromotionMilestoneClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true, DefaultBufferSize = 1 };

    public async IAsyncEnumerable<ConditionIncludeCompleted> GetConditionsIncludeCompletedListAsync(Guid promotionId, string playerCode)
    {
        var response = await client.GetAsync($"/api/milestone/conditions-Include-completed/{promotionId}/{playerCode}", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        var items = JsonSerializer.DeserializeAsyncEnumerable<ConditionIncludeCompleted>(
        responseStream,
            _jsonSerializerOptions);

        await foreach (var item in items)
        {
            yield return item!;
        }
    }

    public async ValueTask<PlayerCondition> GetPlayerConditionAsync(GetPlayerConditionRequest request)
    {
        logger.LogInformation("GetPlayerConditionAsync {brandId} {playerCode} {promotionId}", request.BrandId, request.PlayerCode, request.PromotionId);

        var path = ClientPath.AppendWithProperties("/api/milestone/condition?", new { request.BrandId, request.PlayerCode, request.PromotionId });

        logger.LogInformation("GetPlayerConditionAsync {path}", path);

        var response = await client.GetAsync(path).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PlayerCondition>(
            await response.Content.ReadAsStringAsync(),
            _jsonSerializerOptions)!;
    }
}