using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.Core.Infrastructures.Utility;
using Tgs.Promotion.Milestone.Client.Models;

namespace Tgs.Promotion.Milestone.Client;

public class PromotionMilestoneClient(HttpClient client, ILogger<PromotionMilestoneClient> logger) : IPromotionMilestoneClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true, DefaultBufferSize = 128 };

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

    public async ValueTask<PlayerCondition> GetPlayerConditionAsync(Guid brandId, string playerCode, DateTime from, DateTime to,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("GetPlayerConditionAsync {brandId} {playerCode} {from} {to}", brandId, playerCode, from,
            to);

        var path = ClientPath.AppendWithProperties("/api/milestone/condition?", new { BrandId =brandId, PlayerCode = playerCode, From = from, To = to });

        logger.LogInformation("GetPlayerConditionAsync {path}", path);

        var response = await client.GetAsync(path, cancellationToken).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PlayerCondition>(
            await response.Content.ReadAsStringAsync(cancellationToken),
            _jsonSerializerOptions)!;
    }
}